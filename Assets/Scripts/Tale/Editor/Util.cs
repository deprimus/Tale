#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using TMPro;
using TaleUtil.Scripts;

namespace TaleUtil
{
    public partial class Editor
    {
        static bool TaleWasSetUp()
        {
            return File.Exists(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB);
        }

        static GameObject FindTaleMaster()
        {
            if (!TagExists("TaleMaster"))
            {
                return null;
            }

            return GameObject.FindGameObjectWithTag("TaleMaster");
        }

        static void CreatePrefab(GameObject obj, string path)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

            PrefabUtility.SaveAsPrefabAssetAndConnect(obj, path, InteractionMode.UserAction);
        }

        static bool TagExists(string name)
        {
            return UnityEditorInternal.InternalEditorUtility.tags.Contains(name);
        }

        static void CreateTag(string name)
        {
            if (!TagExists(name))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");

                int index = tagsProp.arraySize;
                tagsProp.InsertArrayElementAtIndex(index);
                SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(index);
                newTag.stringValue = name;

                tagManager.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void InstantiateTaleMasterPrefab()
        {
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB));
        }

        static async void CaptureSceneThumbnails()
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            var count = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < count; ++i)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);

                if (!File.Exists(path))
                {
                    continue; // Deleted scene, but still in build settings
                }

                if (path == System.IO.Path.Combine("Assets", Config.Editor.ASSET_ROOT_SCENE, "SceneSelector.unity").Replace('\\', '/'))
                {
                    continue; // Ignore scene selector
                }

                var thumbnail = SceneThumbnailGenerator.GetThumbnailPathForScenePath(path);

                if (File.Exists(thumbnail))
                {
                    Log.Warning("Thumbnail Generator", string.Format("Skipping scene {0} since it already has a thumbnail; if you want to regenerate it, delete the thumbnail at {1}", path, thumbnail));
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                await SceneThumbnailGenerator.CaptureThumbnail();
            }

            if (currentScenePath != null && currentScenePath.Length > 0)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }

        static TextMeshProUGUI CreateDebugInfoText(string name, GameObject parent, TextAlignmentOptions alignment, Vector2 anchor, Vector2 size, Vector2 pos)
        {
            GameObject obj = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(obj, parent);

            TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 12f;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = alignment;
            text.overflowMode = TextOverflowModes.Ellipsis;

            RectTransform tform = obj.GetComponent<RectTransform>();
            tform.anchorMin = anchor;
            tform.anchorMax = tform.anchorMin;
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.sizeDelta = size;
            tform.anchoredPosition = pos;

            Material material;

            if (File.Exists(Config.Editor.RESOURCE_DEBUG_INFO_MATERIAL)) {
                material = AssetDatabase.LoadAssetAtPath<Material>(Config.Editor.RESOURCE_DEBUG_INFO_MATERIAL);
            } else {
                material = new Material(text.fontMaterial);
                material.SetColor("_OutlineColor", Color.black);
                material.SetFloat("_OutlineWidth", 0.3f);
                material.EnableKeyword("OUTLINE_ON");
                AssetDatabase.CreateAsset(material, TaleUtil.Config.Editor.RESOURCE_DEBUG_INFO_MATERIAL);
            }

            text.fontMaterial = material;

            return text;
        }

        static string GetSplashScenePath(string name) {
            return System.IO.Path.Combine("Assets", Config.Editor.ASSET_ROOT_SCENE, Config.Editor.SPLASH_SCENE_DIR, string.Format("{0}.unity", name)).Replace('\\', '/');
        }

        static void CreateSplashScene(string name, Sprite logo, List<AudioClip> soundVariants, int buildIndex = -1)
        {
            var currentScenePath = EditorSceneManager.GetActiveScene().path;

            var scenePath = GetSplashScenePath(name);

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(scenePath));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);

            scene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

            var canvas = CreateCanvas("Canvas", 0);
            var bg = CreateDarkness("Darkness");
            GameObjectUtility.SetParentAndAlign(bg, canvas);

            var obj = new GameObject("Logo");
            GameObjectUtility.SetParentAndAlign(obj, canvas);

            var curtain = CreateDarkness("Curtain");
            GameObjectUtility.SetParentAndAlign(curtain, canvas);

            var img = obj.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.sprite = logo;

            var tform = obj.GetComponent<RectTransform>();

            float factor;

            if (logo.bounds.size.x > logo.bounds.size.y)
            {
                // width > height
                // resize based on width
                factor = (0.6f * TaleUtil.Config.Editor.REFERENCE_WIDTH) / logo.bounds.size.x;
            }
            else
            {
                // height > width
                // resize based on height
                factor = (0.6f * TaleUtil.Config.Editor.REFERENCE_HEIGHT) / logo.bounds.size.y;
            }

            tform.sizeDelta = new Vector2(factor * logo.bounds.size.x, factor * logo.bounds.size.y);
            tform.anchoredPosition = new Vector2(0f, 0f);

            obj = new GameObject("Splash Master");
            var splash = obj.AddComponent<TaleUtil.Scripts.Splash>();

            if (soundVariants != null) {
                List<string> variants = new List<string>();

                foreach (AudioClip clip in soundVariants)
                {
                    variants.Add(AssetDatabase.GetAssetPath(clip));
                }

                splash.soundVariants = variants;
            }

            splash.curtain = curtain;

            AddSceneToBuild(scenePath, buildIndex);

            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.SaveOpenScenes();

            if (currentScenePath != null && currentScenePath.Length > 0)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }

        static void CreateStoryScene(string name, string script, int buildIndex = -1)
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            string root = "Assets/Scenes";
            string scenePath = string.Format("{0}/{1}.unity", root, name);

            Directory.CreateDirectory(root);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);

            scene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

            GameObject story = new GameObject("Story Master");
            story.AddComponent(AssetDatabase.LoadAssetAtPath<MonoScript>(script).GetClass());

            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB));

            AddSceneToBuild(scenePath, buildIndex);

            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.SaveOpenScenes();

            if (currentScenePath != null && currentScenePath.Length > 0)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }

        static void CreateSceneSelector(string scenePath, int buildIndex) {
            if (File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("Scene Selector already created", "Scene Selector scene already exists.\n\nIf you want to regenerate it, delete the scene at:\n\n" + scenePath, "Ok");
                return;
            }

            if (File.Exists(TaleUtil.Config.Editor.RESOURCE_SCENE_SELECTOR_ITEM_PREFAB))
            {
                EditorUtility.DisplayDialog("Scene Selector already created", "Scene Selector item prefab already exists.\n\nIf you want to regenerate it, delete the prefab at:\n\n" + TaleUtil.Config.Editor.RESOURCE_SCENE_SELECTOR_ITEM_PREFAB, "Ok");
                return;
            }

            var currentScenePath = EditorSceneManager.GetActiveScene().path;

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(scenePath));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);

            scene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

            SetupSceneSelectorItemPrefab();

            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB));

            var canvas = CreateCanvas("Canvas", 0, true);

            var selector = canvas.AddComponent<SceneSelectorMaster>();
            selector.sceneItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TaleUtil.Config.Editor.RESOURCE_SCENE_SELECTOR_ITEM_PREFAB);

            var bg = CreateDarkness("Background");
            GameObjectUtility.SetParentAndAlign(bg, canvas);

            var title = new GameObject("Title");
            GameObjectUtility.SetParentAndAlign(title, canvas);

            TextMeshProUGUI text = title.AddComponent<TextMeshProUGUI>();
            text.text = "Scenes";
            text.fontSize = 96;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            var tform = title.GetComponent<RectTransform>();
            tform.anchorMin = new Vector2(0.5f, 0.5f);
            tform.anchorMax = new Vector2(0.5f, 0.5f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 426f);
            tform.sizeDelta = new Vector2(350f, 110f);

            var group = new GameObject("Scenes Group");
            GameObjectUtility.SetParentAndAlign(group, canvas);

            group.AddComponent<RectTransform>();

            var scroll = new GameObject("Scroll");
            GameObjectUtility.SetParentAndAlign(scroll, group);

            tform = scroll.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 0f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 50f);
            tform.sizeDelta = new Vector2(1420f, 574f);

            var rect = scroll.AddComponent<ScrollRect>();
            rect.horizontal = false;
            rect.vertical = true;
            rect.movementType = ScrollRect.MovementType.Elastic;
            rect.elasticity = 0.05f;
            rect.inertia = true;
            rect.decelerationRate = 0.135f;
            rect.scrollSensitivity = 32f;
            rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            var mask = scroll.AddComponent<RectMask2D>();
            mask.padding = new Vector4(0f, 0f, 0f, -25f);
            mask.softness = new Vector2Int(0, 50);

            var viewport = new GameObject("Viewport");
            GameObjectUtility.SetParentAndAlign(viewport, scroll);

            tform = viewport.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 1f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 0f);
            tform.sizeDelta = new Vector2(0f, 0f);

            rect.viewport = tform;

            var scenes = new GameObject("Scenes");
            GameObjectUtility.SetParentAndAlign(scenes, viewport);

            tform = scenes.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0.5f, 0.5f);
            tform.anchorMax = new Vector2(0.5f, 0.5f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 0f);
            tform.sizeDelta = new Vector2(1520f, 0f);

            rect.content = tform;
            selector.sceneItemParent = tform;

            var layout = scenes.AddComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.cellSize = new Vector2(240f, 180f);
            layout.spacing = new Vector2(50f, 17f);
            layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.Flexible;

            var fitter = scenes.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // TODO: Add canvas renderer?

            var img = scenes.AddComponent<Image>();
            img.color = Color.black;

            var scrollbar = new GameObject("Scrollbar");
            GameObjectUtility.SetParentAndAlign(scrollbar, group);

            tform = scrollbar.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0.5f, 0.5f);
            tform.anchorMax = new Vector2(0.5f, 0.5f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(756f, 0f);
            tform.sizeDelta = new Vector2(5f, 574f);

            var bar = scrollbar.AddComponent<Scrollbar>();
            bar.interactable = true;
            bar.transition = Selectable.Transition.ColorTint;

            var colors = new ColorBlock();
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;

            bar.colors = colors;
            bar.navigation = Navigation.defaultNavigation;
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.value = 0f;
            bar.size = 1f;
            bar.numberOfSteps = 0;

            var script = scrollbar.AddComponent<SceneSelectorScrollbar>();
            selector.scrollbar = script;

            var area = new GameObject("Sliding Area");
            GameObjectUtility.SetParentAndAlign(area, scrollbar);

            tform = area.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 1f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 0f);
            tform.sizeDelta = new Vector2(0f, 0f);

            var handle = new GameObject("Handle");
            GameObjectUtility.SetParentAndAlign(handle, area);

            tform = handle.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 1f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 0f);
            tform.sizeDelta = new Vector2(0f, 0f);

            img = handle.AddComponent<Image>();
            img.color = Color.white;

            script.handleImage = img;
            bar.targetGraphic = img;
            bar.handleRect = tform;
            rect.verticalScrollbar = bar;

            var obj = new GameObject("Logo");
            GameObjectUtility.SetParentAndAlign(obj, canvas);

            img = obj.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.sprite = Resources.Load<Sprite>("Tale/Logo");

            tform = obj.GetComponent<RectTransform>();
            tform.anchorMin = new Vector2(0.5f, 0f);
            tform.anchorMax = new Vector2(0.5f, 0f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 117f);
            tform.sizeDelta = new Vector2(396, 171f);

            AddSceneToBuild(scenePath, buildIndex);

            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.SaveOpenScenes();

            if (currentScenePath != null && currentScenePath.Length > 0)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }

        static void DeleteSceneSelector(string path) {
            DeleteScene(path);
            DeleteAsset(TaleUtil.Config.Editor.RESOURCE_SCENE_SELECTOR_ITEM_PREFAB, true);
        }

        // EditorSceneManager.SaveOpenScenes does not work
        static void SaveCurrentScene() {
            Scene scene = EditorSceneManager.GetActiveScene();

            if (scene.path != null && scene.path.Length > 0) {
                EditorSceneManager.SaveScene(scene, scene.path);
            }
        }

        static void DeleteScene(string scenePath, bool deleteEmptyDir = false) {
            RemoveSceneFromBuild(scenePath);
            DeleteAsset(scenePath, deleteEmptyDir);
        }

        static void RemoveSceneFromBuild(string scenePath) {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

            for (int i = 0; i < buildScenes.Length; ++i) {
                if (buildScenes[i].path == scenePath) {
                    ArrayUtility.Remove(ref buildScenes, buildScenes[i]);
                    EditorBuildSettings.scenes = buildScenes;
                    break;
                }
            }
        }

        static void AddSceneToBuild(string scenePath, int index)
        {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

            EditorBuildSettingsScene target = null;

            int currentIndex = -1;

            for (int i = 0; i < buildScenes.Length; ++i)
            {
                if (buildScenes[i].path == scenePath)
                {
                    target = buildScenes[i];
                    currentIndex = i;
                }
            }

            if (target != null)
            {
                // Scene exists in build settings
                target.enabled = true;
            }
            else
            {
                // Add it to the build settings
                EditorBuildSettingsScene s = new EditorBuildSettingsScene(scenePath, true);
                ArrayUtility.Add(ref buildScenes, s);
                currentIndex = buildScenes.Length - 1;
            }

            if (currentIndex != index && index != -1)
            {
                // TODO: this is incorrect since any scene could be at 'index'.
                // Therefore, it could mess up the user's scene order, which is not very cash money.
                EditorBuildSettingsScene tmp = buildScenes[index];
                buildScenes[index] = buildScenes[currentIndex];
                buildScenes[currentIndex] = tmp;
            }

            EditorSceneManager.SaveOpenScenes();
            EditorBuildSettings.scenes = buildScenes;
        }

        static void DeleteAsset(string path, bool deleteEmptyDir = false) {
            if (File.Exists(path)) {
                File.Delete(path);
            }

            var meta = path + ".meta";

            if (File.Exists(meta)) {
                File.Delete(path + ".meta");
            }

            if (deleteEmptyDir) {
                var dir = System.IO.Path.GetDirectoryName(path);

                if (Directory.Exists(dir)) {
                    if (!Directory.EnumerateFiles(dir).Any()) {
                        Directory.Delete(dir, false);

                        meta = dir + ".meta";

                        if (File.Exists(meta)) {
                            File.Delete(dir + ".meta");
                        }
                    }
                }
            }
        }

        static GameObject CreateAudioSource(string name)
        {
            GameObject obj = new GameObject(name);
            AudioSource src = obj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.priority = 0;
            src.dopplerLevel = 0;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.maxDistance = 10100;
            src.minDistance = 10000;

            return obj;
        }

        static GameObject CreateDarkness(string name)
        {
            GameObject darkness = new GameObject(name);
            Image image = darkness.AddComponent<Image>();
            image.color = Color.black;

            StretchTransform(darkness.GetComponent<RectTransform>());

            return darkness;
        }

        static void CreateTaleTransition(GameObject master, string name)
        {
            if (master == null)
            {
                EditorUtility.DisplayDialog("Tale not set up", "Please set up Tale before creating transitions:\n\nTale -> Setup -> Run Full Setup", "Ok");
                return;
            }

            TaleMaster tale = master.GetComponent<TaleMaster>();

            // Fade
            GameObject canvas = CreateCanvas(string.Format("Transition {0} Canvas", name), TaleUtil.Config.Editor.TRANSITION_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);

            Animator anim = AddAnimator(canvas);

            canvas.SetActive(false);

            GameObject darkness = CreateDarkness("Darkness");
            darkness.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
            GameObjectUtility.SetParentAndAlign(darkness, canvas);

            CreateCompleteTriangleAnimator(anim, string.Format("Transition{0}", name),
                string.Format(TaleUtil.Config.Editor.TRANSITION_ANIMATOR_STATE_FORMAT, "In"),
                string.Format(TaleUtil.Config.Editor.TRANSITION_ANIMATOR_STATE_FORMAT, "Out"),
                string.Format(TaleUtil.Config.Editor.TRANSITION_ANIMATOR_TRIGGER_FORMAT, "In"),
                string.Format(TaleUtil.Config.Editor.TRANSITION_ANIMATOR_TRIGGER_FORMAT, "Out"),
                TaleUtil.Config.Editor.TRANSITION_ANIMATOR_TRIGGER_NEUTRAL,
                "Darkness", typeof(Image), "m_Color.a",
                AnimationCurve.Linear(0f, 1f, 1f, 0f),
                AnimationCurve.Linear(0f, 0f, 1f, 1f));

            if (tale.props.transitions == null)
            {
                tale.props.transitions = new Props.Transition[1];
            }
            else
            {
                Props.Transition[] transitions = new Props.Transition[tale.props.transitions.Length + 1];
                System.Array.Copy(tale.props.transitions, transitions, tale.props.transitions.Length);

                tale.props.transitions = transitions;
            }

            int index = tale.props.transitions.Length - 1;

            tale.props.transitions[index] = new Props.Transition();
            tale.props.transitions[index].name = name;
            tale.props.transitions[index].data = new Props.TransitionData(canvas, canvas.GetComponent<Animator>());
        }

        // Generates the states, triggers and clips for an Animator which are required by Tale
        static void CreateCompleteTriangleAnimator(Animator anim, string controllerName, string stateIn, string stateOut, string triggerIn, string triggerOut, string triggerNeutral, string animatedPath, System.Type animatedType, string animatedProperty, AnimationCurve curveIn, AnimationCurve curveOut)
        {
            string root = "Assets/Animations/Tale";
            Directory.CreateDirectory(root);

            AnimatorController ctrl = new AnimatorController();
            AssetDatabase.CreateAsset(ctrl, string.Format("{0}/{1}.controller", root, controllerName));
            ctrl.name = controllerName;

            ctrl.AddLayer("Base Layer");

            ctrl.AddParameter(triggerIn, AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter(triggerOut, AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter(triggerNeutral, AnimatorControllerParameterType.Trigger);

            if (ctrl.layers[0].stateMachine == null)
            {
                ctrl.layers[0].stateMachine = new AnimatorStateMachine();
            }

            AnimatorStateMachine states = ctrl.layers[0].stateMachine;

            AnimatorState idle = states.AddStateNoWriteDefaults("Idle");
            states.defaultState = idle;

            AnimatorState animStateIn = states.AddStateNoWriteDefaults(stateIn);

            AnimationClip clip = new AnimationClip();
            clip.name = controllerName + "In";
            clip.SetLoop(false);
            clip.SetCurve(animatedPath, animatedType, animatedProperty, curveIn);
            AssetDatabase.CreateAsset(clip, string.Format("{0}/{1}.anim", root, clip.name));

            animStateIn.motion = clip;

            AnimatorState animStateOut = states.AddStateNoWriteDefaults(stateOut);

            clip = new AnimationClip();
            clip.name = controllerName + "Out";
            clip.SetLoop(false);
            clip.SetCurve(animatedPath, animatedType, animatedProperty, curveOut);
            AssetDatabase.CreateAsset(clip, string.Format("{0}/{1}.anim", root, clip.name));

            animStateOut.motion = clip;

            CreateAnimatorTransitions(idle, animStateIn, animStateOut, triggerIn, triggerOut, triggerNeutral);

            anim.runtimeAnimatorController = ctrl;
        }

        static void CreateCompleteLoopingAnimator(Animator anim, string controllerName, string state, string animatedPath, System.Type animatedType, string animatedProperty, AnimationCurve curve)
        {
            string root = "Assets/Animations/Tale";
            Directory.CreateDirectory(root);

            AnimatorController ctrl = new AnimatorController();
            AssetDatabase.CreateAsset(ctrl, string.Format("{0}/{1}.controller", root, controllerName));
            ctrl.name = controllerName;

            ctrl.AddLayer("Base Layer");

            AnimatorStateMachine states = ctrl.layers[0].stateMachine;

            AnimatorState idle = states.AddStateNoWriteDefaults(state);
            states.defaultState = idle;

            AnimationClip clip = new AnimationClip();
            clip.name = state;
            clip.SetLoop(true);
            clip.SetCurve(animatedPath, animatedType, animatedProperty, curve);
            AssetDatabase.CreateAsset(clip, string.Format("{0}/{1}.anim", root, state));

            idle.motion = clip;

            anim.runtimeAnimatorController = ctrl;
        }

        static void StretchTransform(RectTransform tform)
        {
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 1f);
            tform.sizeDelta = new Vector2(0f, 0f);
        }

        static Animator AddAnimator(GameObject obj)
        {
            Animator anim = obj.AddComponent<Animator>();
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;

            return anim;
        }

        static AudioReverbFilter AddAudioReverbFilter(GameObject obj)
        {
            AudioReverbFilter filter = obj.AddComponent<AudioReverbFilter>();
            filter.reverbPreset = AudioReverbPreset.User;
            filter.dryLevel = 0f;
            filter.room = 0f;
            filter.roomHF = 0f;
            filter.roomLF = 0f;
            filter.decayTime = 0.5f;
            filter.decayHFRatio = 1.5f;
            filter.reflectionsLevel = -1300f;
            filter.reflectionsDelay = 0f;
            filter.reverbLevel = -1000f;
            filter.reverbDelay = 0f;
            filter.hfReference = 5000f;
            filter.lfReference = 250f;
            filter.diffusion = 60f;
            filter.density = 50f;

            return filter;
        }

        static void CreateAnimatorTransitions(AnimatorState idle, AnimatorState stateIn, AnimatorState stateOut, string triggerIn, string triggerOut, string triggerNeutral)
        {
            AnimatorStateTransition transition = idle.AddTransition(stateIn);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerIn);

            transition = idle.AddTransition(stateOut);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerOut);

            transition = stateIn.AddTransition(idle);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerNeutral);

            transition = stateOut.AddTransition(idle);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerNeutral);
        }

        static GameObject CreateCanvas(string name, int sortOrder, bool raycast = false)
        {
            if (!UnityEngine.Object.FindFirstObjectByType<EventSystem>())
            {
                GameObject system = new GameObject("EventSystem", typeof(EventSystem));
                system.AddComponent<StandaloneInputModule>();
            }

            GameObject obj = new GameObject(name);

            Canvas canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

            CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(TaleUtil.Config.Editor.REFERENCE_WIDTH, TaleUtil.Config.Editor.REFERENCE_HEIGHT);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100;

            if (raycast)
            {
                var raycaster = obj.AddComponent<GraphicRaycaster>();
                raycaster.ignoreReversedGraphics = true;
                raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                raycaster.blockingMask = ~0; // Everything
            }

            return obj;
        }
    }
}
#endif