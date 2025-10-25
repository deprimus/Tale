#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using TaleUtil.Scripts;

namespace TaleUtil
{
    public partial class Editor
    {
        static void SetupCreateMasterObject(bool dialog = true, bool audio = true, bool transitions = true, bool choice = true, bool cinematic = true, bool debug = true)
        {
            if (File.Exists(TALE_MASTER_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("Tale Master already created", "Tale Master prefab already exists.\n\nIf you want to regenerate it, delete the prefab at:\n\n" + TALE_MASTER_PREFAB_PATH, "Ok");
                return;
            }

            Scene s = EditorSceneManager.GetActiveScene();

            GameObject master = new GameObject("Tale Master", typeof(TaleMaster));

            if (File.Exists(TALE_CONFIG_PATH))
            {
                Log.Warning("Tale Config already exists; if you want to regenerate it, delete the config at " + TALE_CONFIG_PATH);
            }
            else
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TaleUtil.Config>(), TALE_CONFIG_PATH);
            }

            var script = master.GetComponent<TaleMaster>();
            script.props = new TaleMaster.InspectorProps();
            script.config = AssetDatabase.LoadAssetAtPath<TaleUtil.Config>(TALE_CONFIG_PATH);

            if (dialog)
            {
                SetupDialog(master);
            }

            if (audio)
            {
                SetupAudio(master);
            }

            SetupAdvance(master);

            if (transitions)
            {
                SetupTransitions(master);
            }

            if (choice) {
                SetupChoice(master);
            }

            if (cinematic)
            {
                SetupCinematic(master);
            }

            if (debug)
            {
                SetupDebug(master);
            }

            CreateTag("TaleMaster");
            master.tag = "TaleMaster";

            Undo.RegisterCreatedObjectUndo(master, "Create " + master.name);
            Selection.activeGameObject = master;

            CreatePrefab(master, TALE_MASTER_PREFAB_PATH);

            if (s.path != null && s.path.Length > 0)
            {
                EditorSceneManager.SaveScene(s, s.path);
            }
        }

        static void SetupDialog(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            // Canvas
            GameObject canvas = CreateCanvas("Dialog Canvas", TaleUtil.Config.Editor.DIALOG_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);

            // Animations
            Animator anim = AddAnimator(canvas);

            CreateCompleteTriangleAnimator(anim, "Dialog",
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_STATE_IN,
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_STATE_OUT,
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN,
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT,
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL,
                "Panel", typeof(Image), "m_Color.a",
                AnimationCurve.Linear(0f, 0f, 0.5f, 0.5f),
                AnimationCurve.Linear(0f, 0.5f, 0.5f, 0f));

            tale.props.dialogCanvas = canvas.GetComponent<Canvas>();
            tale.props.dialogCanvas.enabled = false;

            tale.props.dialogAnimator = anim;

            // Panel
            GameObject panel = new GameObject("Panel");
            GameObjectUtility.SetParentAndAlign(panel, canvas);

            Image image = panel.AddComponent<Image>();
            image.color = new Color32(0, 0, 0, 0); // Initial alpha must be zero, aka the initial state of the In animation

            RectTransform tform = panel.GetComponent<RectTransform>();
            tform.anchorMin = new Vector2(0.5f, 0f);
            tform.anchorMax = tform.anchorMin;
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.sizeDelta = new Vector2(1496f, 352f);
            tform.anchoredPosition = new Vector2(0f, 212f);

            // Actor
            GameObject actor = new GameObject("Actor");
            GameObjectUtility.SetParentAndAlign(actor, panel);

            // TODO: Fira Sans
            TextMeshProUGUI text = actor.AddComponent<TextMeshProUGUI>();
            text.fontSize = 48f;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.overflowMode = TextOverflowModes.Ellipsis;

            tform = actor.GetComponent<RectTransform>();
            tform.anchorMin = new Vector2(0.5f, 1f);
            tform.anchorMax = tform.anchorMin;
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.sizeDelta = new Vector2(1496f - 192f - 30f, 64f);
            tform.anchoredPosition = new Vector2(81f, -56f);

            tale.props.dialogActor = actor;

            // Content
            GameObject content = new GameObject("Content");
            GameObjectUtility.SetParentAndAlign(content, panel);

            text = content.AddComponent<TextMeshProUGUI>();
            text.fontSize = 38f;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.overflowMode = TextOverflowModes.ScrollRect;

            tform = content.GetComponent<RectTransform>();
            tform.anchorMin = new Vector2(0.5f, 0.5f);
            tform.anchorMax = tform.anchorMin;
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.sizeDelta = new Vector2(1496f - 192f - 30f, 212f);
            tform.anchoredPosition = new Vector2(81f, -42f);

            tale.props.dialogContent = content;

            // Avatar
            GameObject avatar = new GameObject("Avatar");
            GameObjectUtility.SetParentAndAlign(avatar, canvas);

            image = avatar.AddComponent<Image>();
            image.color = new Color32(255, 255, 255, 0);  // Initial alpha must be zero, aka the initial state of the In animation

            tform = avatar.GetComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = tform.anchorMin;
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.sizeDelta = new Vector2(352f, 352f);
            tform.anchoredPosition = new Vector2(176f + 32f, 212f);

            anim = AddAnimator(avatar);

            CreateCompleteTriangleAnimator(anim, "DialogAvatar",
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_STATE_IN,
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_STATE_OUT,
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN,
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT,
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL,
                "", typeof(Image), "m_Color.a",
                AnimationCurve.Linear(0f, 0f, 0.5f, 1f),
                AnimationCurve.Linear(0f, 1f, 0.5f, 0f));

            tale.props.dialogAvatar = avatar;

            // CTC
            GameObject ctc = new GameObject("CTC");
            GameObjectUtility.SetParentAndAlign(ctc, panel);
            ctc.SetActive(false);

            image = ctc.AddComponent<Image>();
            image.color = Color.green;

            tform = ctc.GetComponent<RectTransform>();
            tform.sizeDelta = new Vector2(32f, 32f);
            tform.anchoredPosition = new Vector2(0f, -300f); // Outside the screen

            anim = AddAnimator(ctc);

            CreateCompleteLoopingAnimator(anim, "DialogCTC",
                "CTCIdle",
                "", typeof(RectTransform), "m_LocalEulerAngles.z",
                AnimationCurve.Linear(0f, 0f, 1.5f, -360f));

            tale.props.dialogCtc = ctc;

            // ACTC
            GameObject actc = new GameObject("ACTC");
            GameObjectUtility.SetParentAndAlign(actc, panel);
            actc.SetActive(false);

            image = actc.AddComponent<Image>();
            image.color = Color.yellow;

            tform = actc.GetComponent<RectTransform>();
            tform.sizeDelta = new Vector2(32f, 32f);

            tform.anchoredPosition = new Vector2(0f, -300f); // Outside the screen

            anim = AddAnimator(actc);

            CreateCompleteLoopingAnimator(anim, "DialogACTC",
                "ACTCIdle",
                "", typeof(RectTransform), "m_LocalEulerAngles.z",
                AnimationCurve.Linear(0f, 0f, 1.5f, -360f));

            tale.props.dialogActc = actc;
        }

        static void SetupAudio(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject group = new GameObject("Audio");
            GameObjectUtility.SetParentAndAlign(group, master);
            group.SetActive(false);

            tale.props.audioGroup = group;

            // Music
            GameObject music = CreateAudioSource("Music");
            GameObjectUtility.SetParentAndAlign(music, group);
            music.SetActive(false);

            tale.props.audioMusic = music.GetComponent<AudioSource>();

            // 4 Sound Channels
            GameObject sound = new GameObject("Sound");
            GameObjectUtility.SetParentAndAlign(sound, group);
            sound.SetActive(false);

            tale.props.audioSoundGroup = sound;
            tale.props.audioSound = new AudioSource[4];

            for (int i = 0; i < 4; ++i)
            {
                GameObject channel = CreateAudioSource("Channel" + i);
                GameObjectUtility.SetParentAndAlign(channel, sound);
                channel.SetActive(false);

                tale.props.audioSound[i] = channel.GetComponent<AudioSource>();
            }

            // Voice
            GameObject voice = CreateAudioSource("Voice");
            GameObjectUtility.SetParentAndAlign(voice, group);
            AddAudioReverbFilter(voice);

            tale.props.audioVoice = voice.GetComponent<AudioSource>();
        }

        static void SetupAdvance(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject canvas = CreateCanvas("Advance Canvas", TaleUtil.Config.Editor.ADVANCE_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);

            canvas.SetActive(false);

            tale.props.advanceCanvas = canvas;
        }

        static void SetupTransitions(GameObject master)
        {
            CreateTaleTransition(master, "Fade");
        }

        static void SetupChoice(GameObject master) {
            var tale = master.GetComponent<TaleMaster>();

            var group = new GameObject("Choice");
            GameObjectUtility.SetParentAndAlign(group, master);

            var canvas = CreateCanvas("Choice Default", TaleUtil.Config.Editor.CHOICE_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, group);

            canvas.AddComponent<GraphicRaycaster>();
            canvas.GetComponent<Canvas>().enabled = false;

            var title = new GameObject("Title");
            GameObjectUtility.SetParentAndAlign(title, canvas);

            var tmp = title.AddComponent<TextMeshProUGUI>();
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18f;
            tmp.fontSizeMax = 72f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            var tform = title.GetComponent<RectTransform>();
            tform.anchorMin = new Vector2(0.5f, 0.5f);
            tform.anchorMax = new Vector2(0.5f, 0.5f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.sizeDelta = new Vector2(1165f, 92f);
            tform.anchoredPosition = new Vector2(0f, 0f);

            var script = canvas.AddComponent<TaleUtil.Scripts.Choice.Default.ChoiceMaster>();
            script.enabled = false;

            var ys = new float[] { -40f, 70f, 180f, 290f, 400f };

            script.title = tmp;
            script.choiceObjs = new TaleUtil.Scripts.Choice.Default.ChoiceObj[ys.Length];

            for (int i = 0; i < ys.Length; ++i) {
                var choice = new GameObject("Choice " + (i + 1));
                GameObjectUtility.SetParentAndAlign(choice, canvas);

                var image = choice.AddComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0.5f);

                tform = choice.GetComponent<RectTransform>();
                tform.sizeDelta = new Vector2(1165f, 83f);
                tform.anchoredPosition = new Vector2(0f, ys[i]);

                var obj = choice.AddComponent<TaleUtil.Scripts.Choice.Default.ChoiceObj>();
                script.choiceObjs[i] = obj;

                var text = new GameObject("Text");
                GameObjectUtility.SetParentAndAlign(text, choice);

                tmp = text.AddComponent<TextMeshProUGUI>();
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 18f;
                tmp.fontSizeMax = 72f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                tmp.overflowMode = TextOverflowModes.Ellipsis;

                tform = text.GetComponent<RectTransform>();
                tform.anchorMin = new Vector2(0f, 0f);
                tform.anchorMax = new Vector2(1f, 1f);
                tform.pivot = new Vector2(0.5f, 0.5f);
                tform.sizeDelta = new Vector2(-72f, -17.24f);
                tform.anchoredPosition = new Vector2(0f, 0f);
            }

            tale.props.choiceStyles = new Props.ChoiceStyle[1];

            var style = tale.props.choiceStyles[0] = new Props.ChoiceStyle();
            style.name = "Default";
            style.obj = canvas;
        }

        static void SetupCinematic(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject canvas = CreateCanvas("Cinematic Canvas", TaleUtil.Config.Editor.CINEMATIC_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);
            canvas.SetActive(false);

            tale.props.cinematicCanvas = canvas;

            // Darkness
            GameObject darkness = CreateDarkness("Darkness");
            GameObjectUtility.SetParentAndAlign(darkness, canvas);

            // Background
            GameObject group = new GameObject("Background Group");
            GameObjectUtility.SetParentAndAlign(group, canvas);
            StretchTransform(group.AddComponent<RectTransform>());
            AddAnimator(group);

            tale.props.cinematicBackgroundGroupAnimator = group.GetComponent<Animator>();

            GameObject bg2 = CreateDarkness("Background 2");
            GameObjectUtility.SetParentAndAlign(bg2, group);
            bg2.SetActive(false);

            tale.props.cinematicBackgroundAlt = bg2;

            GameObject bg1 = CreateDarkness("Background 1");
            GameObjectUtility.SetParentAndAlign(bg1, group);

            tale.props.cinematicBackground = bg1;

            // Video
            group = new GameObject("Video Group");
            GameObjectUtility.SetParentAndAlign(group, canvas);
            StretchTransform(group.AddComponent<RectTransform>());
            group.SetActive(false);

            tale.props.cinematicVideoGroup = group;

            RenderTexture texture = new RenderTexture(TaleUtil.Config.Editor.REFERENCE_WIDTH, TaleUtil.Config.Editor.REFERENCE_HEIGHT, 24, RenderTextureFormat.Default);
            texture.Create();

            string dir = "Assets/RenderTextures/Tale";
            string file = "VideoRenderTexture.renderTexture";

            Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(texture, Path.NormalizeAssetPath(dir, file, true));

            GameObject video = new GameObject("Video Player");
            GameObjectUtility.SetParentAndAlign(video, group);

            VideoPlayer player = video.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = texture;
            player.audioOutputMode = VideoAudioOutputMode.AudioSource;

            tale.props.cinematicVideoPlayer = player;

            GameObject img = new GameObject("Raw Image");
            GameObjectUtility.SetParentAndAlign(img, group);
            StretchTransform(img.AddComponent<RectTransform>());

            RawImage image = img.AddComponent<RawImage>();
            image.texture = texture;

            GameObject src = CreateAudioSource("Audio Source");
            GameObjectUtility.SetParentAndAlign(src, group);

            tale.props.cinematicVideoAudioSource = src.GetComponent<AudioSource>();

            // Subtitles
            group = new GameObject("Subtitle Group");
            GameObjectUtility.SetParentAndAlign(group, canvas);
            StretchTransform(group.AddComponent<RectTransform>());
            group.SetActive(false);

            tale.props.cinematicSubtitlesGroup = group;

            GameObject bg = new GameObject("Subtitle Background");
            GameObjectUtility.SetParentAndAlign(bg, group);

            tale.props.cinematicSubtitlesBackground = bg;

            Image i = bg.AddComponent<Image>();
            i.color = Color.black;

            RectTransform tform = bg.GetComponent<RectTransform>();
            tform.sizeDelta = new Vector2(0f, 0f);
            tform.anchoredPosition = new Vector2(0f, -300f);

            GameObject subtitles = new GameObject("Subtitle Text");
            GameObjectUtility.SetParentAndAlign(subtitles, group);

            tale.props.cinematicSubtitles = subtitles;

            TextMeshProUGUI text = subtitles.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Top;

            tform = subtitles.GetComponent<RectTransform>();
            tform.sizeDelta = new Vector2(512, 60);
            tform.anchoredPosition = new Vector2(0f, -300f);
        }

        static void SetupDebug(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject obj = new GameObject("DebugMaster");
            GameObjectUtility.SetParentAndAlign(obj, master);
            DebugMaster debugMaster = obj.AddComponent<DebugMaster>();
            obj.SetActive(true);

            GameObject canvas = CreateCanvas("DebugInfo", TaleUtil.Config.Editor.DEBUG_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, obj);

            DebugInfo debugInfo = canvas.AddComponent<DebugInfo>();
            debugInfo.fps = CreateDebugInfoText("FPS", canvas, TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(638f, 50f), new Vector2(339f, -30f));
            debugInfo.sceneInfo = CreateDebugInfoText("SceneInfo", canvas, TextAlignmentOptions.Top, new Vector2(0.5f, 1f), new Vector2(603f, 50f), new Vector2(0f, -30f));
            debugInfo.actionInfo = CreateDebugInfoText("ActionInfo", canvas, TextAlignmentOptions.TopLeft, new Vector2(1f, 1f), new Vector2(551f, 50f), new Vector2(-382f, -30f));
            debugInfo.actionCountInfo = CreateDebugInfoText("ActionCountInfo", canvas, TextAlignmentOptions.TopRight, new Vector2(1f, 1f), new Vector2(87f, 50f), new Vector2(-63f, -30f));
            canvas.SetActive(false);

            debugMaster.debugInfo = canvas;

            tale.props.debugMaster = debugMaster;
        }

        static void SetupTaleSplashScene()
        {
            CreateSplashScene("Tale", Resources.Load<Sprite>("Tale/Logo"), new List<AudioClip> { Resources.Load<AudioClip>("Tale/Splash") }, 0);
        }

        static void SetupSceneSelectorItemPrefab()
        {
            GameObject obj = new GameObject("TaleSceneSelectorItem");

            var tform = obj.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(0f, 0f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 0f);
            tform.sizeDelta = new Vector2(240f, 180f);

            var script = obj.AddComponent<SceneSelectorItem>();

            var preview = new GameObject("Preview");
            GameObjectUtility.SetParentAndAlign(preview, obj);

            tform = preview.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 1f);
            tform.anchorMax = new Vector2(1f, 1f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, -67.5f);
            tform.sizeDelta = new Vector2(0f, 135f);

            var outline = new GameObject("Outline");
            GameObjectUtility.SetParentAndAlign(outline, preview);

            tform = outline.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 1f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 0f);
            tform.sizeDelta = new Vector2(-6f, -6f);

            script.outlineTform = tform;

            var img = outline.AddComponent<Image>();
            img.color = Color.white;

            script.outline = img;

            var thumbnail = new GameObject("Thumbnail");
            GameObjectUtility.SetParentAndAlign(thumbnail, preview);

            tform = thumbnail.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 1f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 0f);
            tform.sizeDelta = new Vector2(-10f, -10f);

            img = thumbnail.AddComponent<Image>();
            img.color = Color.black;

            script.thumbnail = img;

            var name = new GameObject("Name");
            GameObjectUtility.SetParentAndAlign(name, obj);

            tform = name.AddComponent<RectTransform>();
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 0f);
            tform.pivot = new Vector2(0.5f, 0.5f);
            tform.anchoredPosition = new Vector2(0f, 25f);
            tform.sizeDelta = new Vector2(0f, 50f);

            var text = name.AddComponent<TextMeshProUGUI>();
            text.text = "???";
            text.fontSize = 36;
            text.enableAutoSizing = true;
            text.fontSizeMin = 20;
            text.fontSizeMax = 36;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Bottom;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;

            script.name = text;

            CreatePrefab(obj, TALE_SCENE_SELECTOR_ITEM_PREFAB_PATH);
        }

        static void SetupSceneSelector(int buildIndex = -1)
        {
            string scenePath = System.IO.Path.Combine("Assets", Config.Editor.ASSET_ROOT_SCENE, "SceneSelector.unity").Replace('\\', '/');

            if (File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("Scene Selector already created", "Scene Selector scene already exists.\n\nIf you want to regenerate it, delete the scene at:\n\n" + scenePath, "Ok");
                return;
            }

            if (File.Exists(TALE_SCENE_SELECTOR_ITEM_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("Scene Selector already created", "Scene Selector item prefab already exists.\n\nIf you want to regenerate it, delete the prefab at:\n\n" + TALE_SCENE_SELECTOR_ITEM_PREFAB_PATH, "Ok");
                return;
            }

            var currentScenePath = EditorSceneManager.GetActiveScene().path;

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(scenePath));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);

            scene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

            SetupSceneSelectorItemPrefab();

            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TALE_MASTER_PREFAB_PATH));

            var canvas = CreateCanvas("Canvas", 0, true);

            var selector = canvas.AddComponent<SceneSelectorMaster>();
            selector.sceneItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TALE_SCENE_SELECTOR_ITEM_PREFAB_PATH);

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
    }
}
#endif