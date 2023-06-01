#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

namespace TaleUtil
{
    public partial class Editor
    {
        static void SetupDialog(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            // Canvas
            GameObject canvas = CreateCanvas("Dialog Canvas", Config.DIALOG_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);

            // Animations
            Animator anim = AddAnimator(canvas);

            CreateCompleteTriangleAnimator(anim, "Dialog",
                Config.DIALOG_CANVAS_ANIMATOR_STATE_IN,
                Config.DIALOG_CANVAS_ANIMATOR_STATE_OUT,
                Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN,
                Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT,
                Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL,
                "Panel", typeof(Image), "m_Color.a",
                AnimationCurve.Linear(0f, 0f, 0.5f, 0.5f),
                AnimationCurve.Linear(0f, 0.5f, 0.5f, 0f));
            
            canvas.SetActive(false);

            tale.dialogCanvas = canvas;
            tale.dialogAnimator = anim;

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

            tale.dialogActor = actor;

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

            tale.dialogContent = content;

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
                Config.DIALOG_AVATAR_ANIMATOR_STATE_IN,
                Config.DIALOG_AVATAR_ANIMATOR_STATE_OUT,
                Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN,
                Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT,
                Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL,
                "", typeof(Image), "m_Color.a",
                AnimationCurve.Linear(0f, 0f, 0.5f, 1f),
                AnimationCurve.Linear(0f, 1f, 0.5f, 0f));

            tale.dialogAvatar = avatar;

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

            tale.dialogCtc = ctc;

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

            tale.dialogActc = actc;
        }

        static void SetupAudio(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject group = new GameObject("Audio");
            GameObjectUtility.SetParentAndAlign(group, master);
            group.SetActive(false);

            tale.audioGroup = group;

            // Music
            GameObject music = CreateAudioSource("Music");
            GameObjectUtility.SetParentAndAlign(music, group);
            music.SetActive(false);

            tale.audioMusic = music.GetComponent<AudioSource>();

            // 4 Sound Channels
            GameObject sound = new GameObject("Sound");
            GameObjectUtility.SetParentAndAlign(sound, group);
            sound.SetActive(false);

            tale.audioSoundGroup = sound;
            tale.audioSound = new AudioSource[4];

            for (int i = 0; i < 4; ++i)
            {
                GameObject channel = CreateAudioSource("Channel" + i);
                GameObjectUtility.SetParentAndAlign(channel, sound);
                channel.SetActive(false);

                tale.audioSound[i] = channel.GetComponent<AudioSource>();
            }

            // Voice
            GameObject voice = CreateAudioSource("Voice");
            GameObjectUtility.SetParentAndAlign(voice, group);
            AddAudioReverbFilter(voice);

            tale.audioVoice = voice.GetComponent<AudioSource>();
        }

        static void SetupTransitions(GameObject master)
        {
            CreateTaleTransition(master, "Fade");
        }

        static void SetupCinematic(GameObject master)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            GameObject canvas = CreateCanvas("Cinematic Canvas", Config.CINEMATIC_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);
            canvas.SetActive(false);

            tale.cinematicCanvas = canvas;

            // Darkness
            GameObject darkness = CreateDarkness("Darkness");
            GameObjectUtility.SetParentAndAlign(darkness, canvas);

            // Background
            GameObject group = new GameObject("Background Group");
            GameObjectUtility.SetParentAndAlign(group, canvas);
            AddAnimator(group);

            tale.cinematicBackgroundGroupAnimator = group.GetComponent<Animator>();

            GameObject bg2 = CreateDarkness("Background 2");
            GameObjectUtility.SetParentAndAlign(bg2, group);
            bg2.SetActive(false);

            tale.cinematicBackgroundAlt = bg2;

            GameObject bg1 = CreateDarkness("Background 1");
            GameObjectUtility.SetParentAndAlign(bg1, group);

            tale.cinematicBackground = bg1;

            // Video
            group = new GameObject("Video Group");
            GameObjectUtility.SetParentAndAlign(group, canvas);
            StretchTransform(group.AddComponent<RectTransform>());
            group.SetActive(false);

            tale.cinematicVideoGroup = group;

            RenderTexture texture = new RenderTexture(Config.REFERENCE_WIDTH, Config.REFERENCE_HEIGHT, 24, RenderTextureFormat.Default);
            texture.Create();

            string dir = "Assets/RenderTextures/Tale";
            string file = "VideoRenderTexture.renderTexture";

            Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(texture, Path.NormalizeAssetPath(dir, file));

            GameObject video = new GameObject("Video Player");
            GameObjectUtility.SetParentAndAlign(video, group);

            VideoPlayer player = video.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = texture;
            player.audioOutputMode = VideoAudioOutputMode.AudioSource;

            tale.cinematicVideoPlayer = player;

            GameObject img = new GameObject("Raw Image");
            GameObjectUtility.SetParentAndAlign(img, group);
            StretchTransform(img.AddComponent<RectTransform>());

            RawImage image = img.AddComponent<RawImage>();
            image.texture = texture;

            GameObject src = CreateAudioSource("Audio Source");
            GameObjectUtility.SetParentAndAlign(src, group);

            tale.cinematicVideoAudioSource = src.GetComponent<AudioSource>();

            // Subtitles
            group = new GameObject("Subtitle Group");
            GameObjectUtility.SetParentAndAlign(group, canvas);
            StretchTransform(group.AddComponent<RectTransform>());
            group.SetActive(false);

            tale.cinematicSubtitlesGroup = group;

            GameObject subtitles = new GameObject("Subtitle Text");
            GameObjectUtility.SetParentAndAlign(subtitles, group);

            tale.cinematicSubtitles = subtitles;

            TextMeshProUGUI text = subtitles.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;

            RectTransform tform = subtitles.GetComponent<RectTransform>();
            tform.sizeDelta = new Vector2(512, 60);
            tform.anchoredPosition = new Vector2(0f, -300f);

            GameObject bg = new GameObject("Subtitle Background");
            GameObjectUtility.SetParentAndAlign(bg, group);

            tale.cinematicSubtitlesBackground = bg;

            Image i = bg.AddComponent<Image>();
            i.color = Color.black;

            tform = bg.GetComponent<RectTransform>();
            tform.sizeDelta = new Vector2(0f, 0f);
            tform.anchoredPosition = new Vector2(0f, -300f);
        }

        static void SetupTaleSplashScene()
        {
            CreateSplashScene("Tale", Resources.Load<Sprite>("Tale/Logo"), Resources.Load<AudioClip>("Tale/Splash"), 0);
        }
    }
}
#endif