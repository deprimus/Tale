using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace TaleUtil
{
    public static class Props
    {
        public static Props.Camera camera;
        public static Props.Dialog dialog;
        public static Props.Audio audio;
        public static Props.Cinematic cinematic;

#if UNITY_POST_PROCESSING_STACK_V2
        public static Props.PostProcessing postProcessing;
#endif

        public static GameObject advanceCanvas;

        public static Dictionary<string, TransitionData> transitions;
        public static Dictionary<string, Texture> cameraEffects;

        public static void Init(TaleMaster.InspectorProps props)
        {
            ReinitCamera();

            dialog               = new Dialog(props.dialogCanvas, props.dialogActor, props.dialogContent, props.dialogAvatar, props.dialogAnimator, props.dialogCtc, props.dialogActc);
            audio                = new Audio(props.audioGroup, props.audioSoundGroup, props.audioSound, props.audioMusic, props.audioVoice);
            cinematic            = new Cinematic(props.cinematicCanvas, props.cinematicSubtitles, props.cinematicSubtitlesBackground, props.cinematicSubtitlesGroup);
            cinematic.background = new CinematicBackground(props.cinematicBackgroundGroupAnimator, props.cinematicBackground, props.cinematicBackgroundAlt);
            cinematic.video      = new CinematicVideo(props.cinematicVideoGroup, props.cinematicVideoPlayer, props.cinematicVideoAudioSource);

            advanceCanvas = props.advanceCanvas;

            transitions = new Dictionary<string, TransitionData>();

            for (int i = 0; i < props.transitions.Length; ++i)
            {
                if (props.transitions[i].data.canvas == null || props.transitions[i].data.animator == null)
                    Warning(string.Format("Canvas or animator is null for transition '{0}'; the entry will be ignored", props.transitions[i].name));
                else transitions[props.transitions[i].name.ToLowerInvariant()] = new TransitionData(props.transitions[i].data.canvas, props.transitions[i].data.animator);
            }

            if (props.transitions.Length != transitions.Count)
                Warning("Two or more transitions with same name exist; in these cases, the last one is kept");

#if UNITY_POST_PROCESSING_STACK_V2
            cameraEffects = new Dictionary<string, Texture>();

            for(int i = 0; i < props.cameraEffects.Length; ++i)
            {
                if(props.cameraEffects[i].texture == null)
                    Warning(string.Format("Texture is null for camera effect '{0}'; the entry will be ignored", props.cameraEffects[i].name));
                else cameraEffects[props.cameraEffects[i].name.ToLower()] = props.cameraEffects[i].texture;
            }

            if(props.cameraEffects.Length != cameraEffects.Count)
                Warning("Two or more camera effects with same name exist (case insensitive); in these cases, the last one is kept");
#else
            if (props.cameraEffects.Length > 0)
            {
                Warning("Camera effects require PostProcessing V2 to be installed; ignoring effects registered in TaleMaster");
            }
#endif
        }

        public static void Reset()
        {
            if (dialog != null)
            {
                dialog.Reset();
            }

            if (audio != null)
            {
                audio.Reset();
            }

            if (cinematic != null)
            {
                cinematic.Reset();
            }

            foreach (var t in transitions)
            {
                if (t.Value.canvas.activeSelf)
                {
                    Tale.Parallel(
                        Tale.Transition(t.Key, Tale.TransitionType.IN, 0f)
                    );
                }
            }
        }

        public static void Warning(string msg) =>
            TaleUtil.Log.Warning("PROPS", msg);

        public static void Error(string msg) =>
            TaleUtil.Log.Error("PROPS", msg);

        public static void ReinitCamera()
        {
            // It's important to keep the reference, because the camera actions which use Transformable need a reliable reference,
            // which doesn't change with the scene.
            if (camera == null)
                camera = new Props.Camera(UnityEngine.Camera.main);
            else camera.Reinit(UnityEngine.Camera.main);

#if UNITY_POST_PROCESSING_STACK_V2
            postProcessing = new Props.PostProcessing(camera.obj);
#endif

            if (camera == null)
                Error("Could not retrieve main camera");
        }

        // A constant reference to a changing Transform reference.
        // For example, when changing scenes, the Camera transform reference changes as well.
        // This is problematic, because some Tale actions need to have a valid Transform reference at all times.
        // Transformable solves this problem by simply wrapping around a Transform.
        // Even if the Transform reference changes, the Transformable address always stays the same.
        public class Transformable
        {
            public Transform transform;
            
            public Transformable() { }

            public Transformable(Transform transform) =>
                this.transform = transform;
        }

        public class Camera : Transformable
        {
            public UnityEngine.Camera obj;
            public float baseOrthographicSize;

            public Camera(UnityEngine.Camera camera)
            {
                Reinit(camera);
            }

            public void Reinit(UnityEngine.Camera camera)
            {
                obj = camera;
                baseOrthographicSize = obj.orthographicSize;
                transform = obj.GetComponent<Transform>();
            }
        }

        public class Dialog
        {
            public Canvas canvas;

            public TextMeshProUGUI actor;
            public TextMeshProUGUI content;

            public Image avatar;
            public Animator avatarAnimator;

            public Animator animator;

            public GameObject ctc;
            public RectTransform ctcTransform;

            public GameObject actc;
            public RectTransform actcTransform;

            public Dialog(Canvas canvas, GameObject actor, GameObject content, GameObject avatar, Animator animator, GameObject ctc, GameObject actc)
            {
                if(canvas != null)
                {
                    this.canvas = canvas;

                    if (actor != null)
                    {
                        this.actor = actor.GetComponent<TextMeshProUGUI>();
                    }

                    if (content != null)
                    {
                        this.content = content.GetComponent<TextMeshProUGUI>();

                    }

                    if (this.actor == null)
                    {
                        Warning("Dialog actor object does not have a TextMeshProUGUI component");
                    }

                    if (this.content == null)
                    {
                        Warning("Dialog content object does not have a TextMeshProUGUI component");
                    }

                    if (avatar != null)
                    {
                        this.avatar = avatar.GetComponent<Image>();
                        // TODO: if we get the animator component directly from the avatar object,
                        // why not do the same for the dialog canvas?
                        this.avatarAnimator = avatar.GetComponent<Animator>();

                        if(this.avatar == null)
                        {
                            Warning("Dialog avatar object does not have an Image component");
                        }
                    }

                    this.animator = animator;

                    this.ctc = ctc;

                    if(this.ctc != null)
                    {
                        ctcTransform = this.ctc.GetComponent<RectTransform>();

                        if (ctcTransform == null)
                        {
                            Warning("Dialog CTC object does not have a RectTransform component");
                        }
                    }

                    this.actc = actc;

                    if(this.actc != null)
                    {
                        actcTransform = this.actc.GetComponent<RectTransform>();

                        if (actcTransform == null)
                        {
                            Warning("Dialog CTC (Additive) object does not have a RectTransform component");
                        }
                    }
                }
            }

            public void Reset()
            {
                if (dialog.canvas != null)
                {
                    if (actor != null)
                    {
                        actor.text = "";
                    }

                    if (content != null)
                    {
                        content.text = "";
                    }

                    if (ctc != null)
                    {
                        ctc.SetActive(false);
                    }

                    if (actc != null)
                    {
                        actc.SetActive(false);
                    }

                    dialog.canvas.enabled = false;
                }
            }
        }

        public class Audio
        {
            public GameObject group;
            public GameObject soundGroup;
            public AudioSource[] sound;
            public AudioSource music;
            public AudioSource voice;
            public AudioReverbFilter voiceReverb;

            public Audio(GameObject group, GameObject soundGroup, AudioSource[] sound, AudioSource music, AudioSource voice)
            {
                this.group = group;
                this.soundGroup = soundGroup;
                this.sound = sound;
                this.music = music;
                this.voice = voice;

                if (soundGroup == null && sound != null && sound.Length > 0)
                {
                    Warning("The audio sound channel list is not empty, but the audio sound group is null");
                }

                if (voice != null)
                {
                    voiceReverb = voice.gameObject.GetComponent<AudioReverbFilter>();
                }
            }

            public void Reset()
            {
                if (music != null)
                {
                    music.Stop();
                }

                if (sound != null)
                {
                    foreach (var s in sound)
                    {
                        s.Stop();
                    }

                    if (soundGroup != null)
                    {
                        soundGroup.SetActive(false);
                    }
                }

                if (voice != null)
                {
                    voice.Stop();
                }
            }
        }

        public class Cinematic
        {
            public GameObject canvas;
            public TextMeshProUGUI subtitles;
            public RectTransform subtitlesBackground;
            public GameObject subtitlesGroup;

            public CinematicBackground background;
            public CinematicVideo video;

            public Cinematic(GameObject canvas, GameObject subtitles, GameObject subtitlesBackground, GameObject subtitlesGroup)
            {
                if(canvas != null)
                {
                    this.canvas = canvas;

                    if (subtitlesGroup != null)
                    {
                        this.subtitlesGroup = subtitlesGroup;
                    }

                    if(subtitles != null)
                    {
                        this.subtitles = subtitles.GetComponent<TextMeshProUGUI>();

                        if (this.subtitles == null)
                        {
                            Warning("Cinematic subtitles object does not have a TextMeshProUGUI component");
                        }
                        else if (this.subtitlesGroup == null)
                        {
                            Warning("Cinematic subtitles object is not null, but the cinematic subtitles group is null");
                        }
                    }

                    if(subtitlesBackground != null)
                    {
                        this.subtitlesBackground = subtitlesBackground.GetComponent<RectTransform>();

                        if (this.subtitlesBackground == null)
                        {
                            Warning("Cinematic subtitles background object does not have a RectTransform component");
                        }
                    }
                }
            }

            public void Reset()
            {
                if (canvas != null)
                {
                    if (subtitlesGroup != null)
                    {
                        subtitlesGroup.SetActive(false);
                    }

                    if (background != null)
                    {
                        background.Reset();
                    }

                    if (video != null)
                    {
                        video.Reset();
                    }

                    canvas.SetActive(false);
                }
            }
        }

        public class CinematicBackground
        {
            public Animator groupAnimator;
            public Image image;
            public Image imageAlt;
            public RectTransform transform;
            public RectTransform transformAlt;

            private bool isOriginalActive;

            public CinematicBackground(Animator groupAnimator, GameObject background, GameObject backgroundAlt)
            {
                this.groupAnimator = groupAnimator;
                
                if(background != null)
                {
                    this.image = background.GetComponent<Image>();
                    this.transform = background.GetComponent<RectTransform>();

                    if (this.image == null)
                    {
                        Warning("Cinematic background object does not have an Image component");
                    }

                    if (this.transform == null)
                    {
                        Warning("Cinematic background object does not have a RectTransform component");
                    }

                    if(backgroundAlt != null)
                    {
                        this.imageAlt = backgroundAlt.GetComponent<Image>();
                        this.transformAlt = backgroundAlt.GetComponent<RectTransform>();

                        if (this.imageAlt == null)
                        {
                            Warning("Cinematic background (alternative) object does not have an Image component");
                        }

                        if (this.transformAlt == null)
                        {
                            Warning("Cinematic background (alternative) object does not have a RectTransform component");
                        }
                    }
                }

                this.isOriginalActive = true;
            }

            public void Reset()
            {
                if (image != null)
                {
                    image.sprite = null;
                }

                if (imageAlt != null)
                {
                    imageAlt.sprite = null;
                }
            }

            public void Swap()
            {
                isOriginalActive = !isOriginalActive;
                GetActiveTransform().SetAsLastSibling();
            }

            public Image GetActiveImage()
            {
                return (isOriginalActive ? image : imageAlt);
            }

            public Image GetPassiveImage()
            {
                return (isOriginalActive ? imageAlt : image);
            }

            public RectTransform GetActiveTransform()
            {
                return (isOriginalActive ? transform : transformAlt);
            }

            public RectTransform GetPassiveTransform()
            {
                return (isOriginalActive ? transformAlt : transform);
            }
        }

        public class CinematicVideo
        {
            public GameObject group;
            public VideoPlayer player;
            public AudioSource audio;

            public CinematicVideo(GameObject group, VideoPlayer player, AudioSource audio)
            {
                this.group = group;
                this.player = player;
                this.audio = audio;

                if(this.player != null)
                {
                    this.player.loopPointReached += TaleUtil.Events.OnCinematicVideoEnd;
                }
            }

            public void Reset()
            {
                if (player != null)
                {
                    player.Stop();
                    
                    if (audio)
                    {
                        audio.Stop();
                    }

                    group.SetActive(false);
                }
            }
        }

#if UNITY_POST_PROCESSING_STACK_V2
        public class PostProcessing
        {
            public Vignette vignette;
            public ColorGrading colorGrading;
            public Bloom bloom;

            public PostProcessing(UnityEngine.Camera camera)
            {
                PostProcessVolume volume = camera.GetComponent<PostProcessVolume>();

                if(volume != null)
                {
                    PostProcessProfile profile = volume.profile;

                    profile.TryGetSettings(out vignette);
                    profile.TryGetSettings(out colorGrading);
                    profile.TryGetSettings(out bloom);
                }
            }
        }
#endif

        [System.Serializable]
        public class Transition
        {
            public string name;
            public TransitionData data;
        }

        [System.Serializable]
        public class TransitionData
        {
            public GameObject canvas;
            public Animator animator;

            public TransitionData(GameObject canvas, Animator animator)
            {
                this.canvas = canvas;
                this.animator = animator;
            }
        }

        [System.Serializable]
        public class CameraEffect
        {
            public string name;
            public Texture texture;
        }
    }
}