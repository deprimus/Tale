#if UNITY_EDITOR
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

namespace TaleUtil
{
    public partial class Editor
    {
        const string TALE_PREFAB_PATH = "Assets/Prefabs/TaleMaster.prefab";
        const string TALE_CONFIG_PATH = "Assets/TaleConfig.asset";

        static bool TaleWasSetUp()
        {
            return File.Exists(TALE_PREFAB_PATH);
        }

        static GameObject FindTaleMaster()
        {
            if (!TagExists("TaleMaster"))
            {
                return null;
            }

            return GameObject.FindGameObjectWithTag("TaleMaster");
        }

        static GameObject GetTaleMasterPrefab()
        {
            if (!TaleWasSetUp())
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(TALE_PREFAB_PATH);
        }

        static void CreateTaleMasterPrefab(GameObject obj)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(TALE_PREFAB_PATH));

            PrefabUtility.SaveAsPrefabAssetAndConnect(obj, TALE_PREFAB_PATH, InteractionMode.UserAction);
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
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TALE_PREFAB_PATH));
        }

        static async void CaptureSceneThumbnails()
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            var count = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < count; ++i)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);

                if (path == System.IO.Path.Combine("Assets/", Config.Setup.ASSET_ROOT_SCENE, "SceneSelector.unity").Replace('\\', '/'))
                {
                    continue; // Ignore scene selector
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
            text.color = new Color32(200, 200, 200, 255);
            text.alignment = alignment;
            text.overflowMode = TextOverflowModes.Ellipsis;

            RectTransform tform = obj.GetComponent<RectTransform>();
            tform.anchorMin = anchor;
            tform.anchorMax = tform.anchorMin;
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.sizeDelta = size;
            tform.anchoredPosition = pos;

            return text;
        }

        static void CreateSplashScene(string name, Sprite logo, List<AudioClip> soundVariants, int buildIndex = -1)
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;

            string root = System.IO.Path.Combine("Assets/", Config.Setup.ASSET_ROOT_SCENE, "Splash").Replace('\\', '/');
            string scenePath = string.Format("{0}/{1}.unity", root, name);

            Directory.CreateDirectory(root);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);

            scene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

            GameObject canvas = CreateCanvas("Canvas", 0);
            GameObject bg = CreateDarkness("Darkness");
            GameObjectUtility.SetParentAndAlign(bg, canvas);

            GameObject obj = new GameObject("Logo");
            GameObjectUtility.SetParentAndAlign(obj, canvas);

            GameObject curtain = CreateDarkness("Curtain");
            GameObjectUtility.SetParentAndAlign(curtain, canvas);

            Image img = obj.AddComponent<Image>();
            img.color = Color.white;
            img.preserveAspect = true;
            img.sprite = logo;

            RectTransform tform = obj.GetComponent<RectTransform>();

            float factor;

            if (logo.bounds.size.x > logo.bounds.size.y)
            {
                // width > height
                // resize based on width
                factor = (0.6f * TaleUtil.Config.Setup.REFERENCE_WIDTH) / logo.bounds.size.x;
            }
            else
            {
                // height > width
                // resize based on height
                factor = (0.6f * TaleUtil.Config.Setup.REFERENCE_HEIGHT) / logo.bounds.size.y;
            }

            tform.sizeDelta = new Vector2(factor * logo.bounds.size.x, factor * logo.bounds.size.y);
            tform.anchoredPosition = new Vector2(0f, 0f);

            obj = new GameObject("Splash Master");
            Splash splash = obj.AddComponent<Splash>();

            if (soundVariants != null) {
                List<string> variants = new List<string>();

                foreach (AudioClip clip in soundVariants)
                {
                    variants.Add(AssetDatabase.GetAssetPath(clip));
                }

                splash.soundVariants = variants;
            }

            splash.curtain = curtain;

            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TALE_PREFAB_PATH));

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

            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TALE_PREFAB_PATH));

            AddSceneToBuild(scenePath, buildIndex);

            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.SaveOpenScenes();

            if (currentScenePath != null && currentScenePath.Length > 0)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
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
                EditorBuildSettingsScene tmp = buildScenes[index];
                buildScenes[index] = buildScenes[currentIndex];
                buildScenes[currentIndex] = tmp;
            }

            EditorSceneManager.SaveOpenScenes();
            EditorBuildSettings.scenes = buildScenes;
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
            GameObject canvas = CreateCanvas(string.Format("Transition {0} Canvas", name), TaleUtil.Config.Setup.TRANSITION_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);

            Animator anim = AddAnimator(canvas);

            canvas.SetActive(false);

            GameObject darkness = CreateDarkness("Darkness");
            darkness.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
            GameObjectUtility.SetParentAndAlign(darkness, canvas);

            CreateCompleteTriangleAnimator(anim, string.Format("Transition{0}", name),
                string.Format(TaleUtil.Config.Setup.TRANSITION_ANIMATOR_STATE_FORMAT, "In"),
                string.Format(TaleUtil.Config.Setup.TRANSITION_ANIMATOR_STATE_FORMAT, "Out"),
                string.Format(TaleUtil.Config.Setup.TRANSITION_ANIMATOR_TRIGGER_FORMAT, "In"),
                string.Format(TaleUtil.Config.Setup.TRANSITION_ANIMATOR_TRIGGER_FORMAT, "Out"),
                TaleUtil.Config.Setup.TRANSITION_ANIMATOR_TRIGGER_NEUTRAL,
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

        static GameObject CreateCanvas(string name, int sortOrder)
        {
            if (!Object.FindObjectOfType<EventSystem>())
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
            scaler.referenceResolution = new Vector2(TaleUtil.Config.Setup.REFERENCE_WIDTH, TaleUtil.Config.Setup.REFERENCE_HEIGHT);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100;

            return obj;
        }
    }
}
#endif