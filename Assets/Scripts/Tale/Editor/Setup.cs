#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using TMPro;
using TaleUtil.Scripts;

namespace TaleUtil
{
    public partial class Editor
    {
        static void SetupCreateMasterObject(SetupFlag dialog, SetupFlag audio, SetupFlag transitions, SetupFlag choice, SetupFlag cinematic, SetupFlag debug)
        {
            GameObject master;

            if (File.Exists(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB)) {
                master = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB)) as GameObject;
            } else {
                master = new GameObject("Tale Master", typeof(TaleMaster));

                if (File.Exists(TaleUtil.Config.Editor.RESOURCE_CONFIG)) {
                    Log.Warning("Tale Config already exists; if you want to regenerate it, delete the config at " + TaleUtil.Config.Editor.RESOURCE_CONFIG);
                } else {
                    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TaleUtil.Config>(), TaleUtil.Config.Editor.RESOURCE_CONFIG);
                }

                var script = master.GetComponent<TaleMaster>();
                script.props = new TaleMaster.InspectorProps();
                script.config = AssetDatabase.LoadAssetAtPath<TaleUtil.Config>(TaleUtil.Config.Editor.RESOURCE_CONFIG);

                SetupAdvance(master);

                CreateTag("TaleMaster");
                master.tag = "TaleMaster";
            }

            if (dialog.HasChanged())
            {
                if (dialog.should) {
                    SetupDialog(master);
                } else {
                    RemoveDialog(master);
                }
            }

            if (audio.HasChanged())
            {
                if (audio.should) {
                    SetupAudio(master);
                } else {
                    RemoveAudio(master);
                }
            }

            if (transitions.HasChanged())
            {
                if (transitions.should) {
                    SetupTransitions(master);
                } else {
                    RemoveTransitions(master);
                }
            }

            if (choice.HasChanged()) {
                if (choice.should) {
                    SetupChoice(master);
                } else {
                    RemoveChoice(master);
                }
            }

            if (cinematic.HasChanged())
            {
                if (cinematic.should) {
                    SetupCinematic(master);
                } else {
                    RemoveCinematic(master);
                }
            }

            if (debug.HasChanged())
            {
                if (debug.should) {
                    SetupDebug(master);
                } else {
                    RemoveDebug(master);
                }
            }

            Undo.RegisterCreatedObjectUndo(master, "Set up " + master.name);
            Selection.activeGameObject = master;

            if (File.Exists(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB)) {
                PrefabUtility.ApplyPrefabInstance(master, InteractionMode.UserAction);
            } else {
                CreatePrefab(master, TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB);
            }

            GameObject.DestroyImmediate(master);

            SaveCurrentScene();
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
            text.textWrappingMode = TextWrappingModes.Normal;
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

        static void RemoveDialog(GameObject master) {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject.DestroyImmediate(tale.props.dialogCanvas.gameObject);

            tale.props.dialogCanvas = null;
            tale.props.dialogAnimator = null;
            tale.props.dialogAvatar = null;
            tale.props.dialogActor = null;
            tale.props.dialogContent = null;
            tale.props.dialogCtc = null;
            tale.props.dialogActc = null;
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

        static void RemoveAudio(GameObject master) {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject.DestroyImmediate(tale.props.audioGroup);

            tale.props.audioGroup = null;
            tale.props.audioSoundGroup = null;
            tale.props.audioSound = null;
            tale.props.audioMusic = null;
            tale.props.audioVoice = null;
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

        static void RemoveTransitions(GameObject master) {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            foreach (var transition in tale.props.transitions) {
                GameObject.DestroyImmediate(transition.data.canvas);
            }

            tale.props.transitions = null;
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

        static void RemoveChoice(GameObject master) {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            foreach (var choice in tale.props.choiceStyles) {
                GameObject.DestroyImmediate(choice.obj.transform.parent.gameObject);
            }

            tale.props.choiceStyles = null;
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
            AssetDatabase.CreateAsset(texture, Path.NormalizeResourcePath(dir, file, true));

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

        static void RemoveCinematic(GameObject master) {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject.DestroyImmediate(tale.props.cinematicCanvas);

            tale.props.cinematicCanvas = null;
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

        static void RemoveDebug(GameObject master) {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject.DestroyImmediate(tale.props.debugMaster.gameObject);

            tale.props.debugMaster = null;
        }

        static void SetupTaleSplashScene(SetupFlag setup)
        {
            if (!setup.HasChanged()) {
                return;
            }

            if (setup.should) {
                CreateSplashScene("Tale", Resources.Load<Sprite>("Tale/Logo"), new List<AudioClip> { Resources.Load<AudioClip>("Tale/Splash") }, 0);
            } else {
                DeleteScene(GetSplashScenePath("Tale"), true);
            }
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

            CreatePrefab(obj, TaleUtil.Config.Editor.RESOURCE_SCENE_SELECTOR_ITEM_PREFAB);
        }

        static void SetupSceneSelector(SetupFlag setup, int buildIndex = -1)
        {
            if (!setup.HasChanged()) {
                return;
            }

            string scenePath = System.IO.Path.Combine("Assets", Config.Editor.ASSET_ROOT_SCENE, "SceneSelector.unity").Replace('\\', '/');

            if (setup.should) {
                CreateSceneSelector(scenePath, buildIndex);
            } else {
                DeleteSceneSelector(scenePath);
            }
        }

        public static void CleanTale() {
            SetupTaleSplashScene(false);
            SetupSceneSelector(false);

            var master = GameObject.Find("Tale Master");
            if (master != null) {
                GameObject.DestroyImmediate(master);
            }

            if (System.IO.File.Exists(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB)) {
                DeleteAsset(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB, true);
            }

            SaveCurrentScene();
            AssetDatabase.Refresh();
        }
    }
}
#endif