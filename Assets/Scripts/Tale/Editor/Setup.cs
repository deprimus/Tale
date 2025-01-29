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

namespace TaleUtil
{
    public partial class Editor
    {
        static void SetupCreateMasterObject(bool dialog = true, bool audio = true, bool transitions = true, bool cinematic = true, bool debug = true)
        {
            if (File.Exists(TALE_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("Tale Master already created", "Tale Master prefab already exists.\n\nIf you want to regenerate it, delete the prefab at:\n\n" + TALE_PREFAB_PATH, "Ok");
                return;
            }

            Scene s = EditorSceneManager.GetActiveScene();

            GameObject master = new GameObject("Tale Master", typeof(TaleMaster));

            if (!File.Exists(TALE_CONFIG_PATH))
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

            CreateTaleMasterPrefab(master);

            if (s.path != null && s.path.Length > 0)
            {
                EditorSceneManager.SaveScene(s, s.path);
            }
        }

        static void SetupDialog(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            // Canvas
            GameObject canvas = CreateCanvas("Dialog Canvas", TaleUtil.Config.Setup.DIALOG_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);

            // Animations
            Animator anim = AddAnimator(canvas);

            CreateCompleteTriangleAnimator(anim, "Dialog",
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_STATE_IN,
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_STATE_OUT,
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN,
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT,
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL,
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
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_STATE_IN,
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_STATE_OUT,
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN,
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT,
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL,
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

            GameObject canvas = CreateCanvas("Advance Canvas", TaleUtil.Config.Setup.ADVANCE_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);

            canvas.SetActive(false);

            tale.props.advanceCanvas = canvas;
        }

        static void SetupTransitions(GameObject master)
        {
            CreateTaleTransition(master, "Fade");
        }

        static void SetupCinematic(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject canvas = CreateCanvas("Cinematic Canvas", TaleUtil.Config.Setup.CINEMATIC_SORT_ORDER);
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

            RenderTexture texture = new RenderTexture(TaleUtil.Config.Setup.REFERENCE_WIDTH, TaleUtil.Config.Setup.REFERENCE_HEIGHT, 24, RenderTextureFormat.Default);
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

            GameObject canvas = CreateCanvas("DebugInfo", TaleUtil.Config.Setup.DEBUG_SORT_ORDER);
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
    }
}
#endif