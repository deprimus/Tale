using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

namespace TaleUtil
{
    public static class Props
    {
        public static Props.Camera camera;
        public static Props.PostProcessing postProcessing;
        public static Props.Dialog dialog;
        public static Props.Audio audio;
        public static Props.Cinematic cinematic;

        public static Dictionary<string, TransitionData> transitions;
        public static Dictionary<string, Texture> cameraEffects;

        public static void Init(GameObject dialogCanvas, GameObject dialogActor, GameObject dialogContent, Animator dialogAnimator, GameObject dialogCtc, GameObject dialogActc,
                                GameObject audioGroup, GameObject audioSoundGroup, AudioSource[] audioSound, AudioSource audioMusic, AudioSource audioVoice,
                                GameObject cinematicCanvas, GameObject cinematicSubtitles, GameObject cinematicSubtitlesBackground, GameObject cinematicSubtitlesGroup,
                                Animator cinematicBackgroundGroupAnimator, GameObject cinematicBackground, GameObject cinematicBackgroundAlt,
                                GameObject cinematicVideoGroup, VideoPlayer cinematicVideoPlayer, AudioSource cinematicVideoAudioSource,
                                Transition[] transitionArray, CameraEffect[] cameraEffectArray)
        {
            ReinitCamera();

            dialog               = new Dialog(dialogCanvas, dialogActor, dialogContent, dialogAnimator, dialogCtc, dialogActc);
            audio                = new Audio(audioGroup, audioSoundGroup, audioSound, audioMusic, audioVoice);
            cinematic            = new Cinematic(cinematicCanvas, cinematicSubtitles, cinematicSubtitlesBackground, cinematicSubtitlesGroup);
            cinematic.background = new CinematicBackground(cinematicBackgroundGroupAnimator, cinematicBackground, cinematicBackgroundAlt);
            cinematic.video      = new CinematicVideo(cinematicVideoGroup, cinematicVideoPlayer, cinematicVideoAudioSource);

            transitions = new Dictionary<string, TransitionData>();

            for(int i = 0; i < transitionArray.Length; ++i)
            {
                if(transitionArray[i].data.canvas == null || transitionArray[i].data.animator == null)
                    Warning(string.Format("Canvas or animator is null for transition '{0}'; the entry will be ignored", transitionArray[i].name));
                else transitions[transitionArray[i].name.ToLowerInvariant()] = new TransitionData(transitionArray[i].data.canvas, transitionArray[i].data.animator);
            }

            if (transitionArray.Length != transitions.Count)
                Warning("Two or more transitions with same name exist; in these cases, the last one is kept");

            cameraEffects = new Dictionary<string, Texture>();

            for(int i = 0; i < cameraEffectArray.Length; ++i)
            {
                if(cameraEffectArray[i].texture == null)
                    Warning(string.Format("Texture is null for camera effect '{0}'; the entry will be ignored", cameraEffectArray[i].name));
                else cameraEffects[cameraEffectArray[i].name.ToLower()] = cameraEffectArray[i].texture;
            }

            if(cameraEffectArray.Length != cameraEffects.Count)
                Warning("Two or more camera effects with same name exist (case insensitive); in these cases, the last one is kept");
        }

        public static void Warning(string msg) =>
            TaleUtil.Log.Warning("PROPS", msg);

        public static void Error(string msg) =>
            TaleUtil.Log.Error("PROPS", msg);

        public static void ReinitCamera()
        {
            // It's important to keep the reference, because the camera actions which use Transformable need a reliable reference,
            // which doesn't change with the scene.
            if(camera == null)
                camera = new Props.Camera(UnityEngine.Camera.main);
            else camera.Reinit(UnityEngine.Camera.main);

            postProcessing = new Props.PostProcessing(camera.obj);

            if(camera == null)
                Error("Could not retrieve main camera");
        }

        // A constant reference to a changing Transform reference.
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
            //public Transform transform;

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
            public GameObject canvas;

            public TextMeshProUGUI actor;
            public TextMeshProUGUI content;

            public Animator animator;

            public GameObject ctc;
            public RectTransform ctcTransform;

            public GameObject actc;
            public RectTransform actcTransform;

            public Dialog(GameObject canvas, GameObject actor, GameObject content, Animator animator, GameObject ctc, GameObject actc)
            {
                if(canvas != null)
                {
                    this.canvas = canvas;

                    if(actor != null)
                        this.actor = actor.GetComponent<TextMeshProUGUI>();

                    if(content != null)
                        this.content = content.GetComponent<TextMeshProUGUI>();

                    this.animator = animator;

                    if(this.actor == null)
                        Warning("Dialog actor object does not have a TextMeshProUGUI component");

                    if(this.content == null)
                        Warning("Dialog content object does not have a TextMeshProUGUI component");

                    this.ctc = ctc;

                    if(this.ctc != null)
                    {
                        ctcTransform = this.ctc.GetComponent<RectTransform>();
                        
                        if(ctcTransform == null)
                            Warning("Dialog CTC object does not have a RectTransform component");
                    }

                    this.actc = actc;

                    if(this.actc != null)
                    {
                        actcTransform = this.actc.GetComponent<RectTransform>();

                        if(actcTransform == null)
                            Warning("Dialog CTC (Additive) object does not have a RectTransform component");
                    }
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

            public Audio(GameObject group, GameObject soundGroup, AudioSource[] sound, AudioSource music, AudioSource voice)
            {
                this.group = group;
                this.soundGroup = soundGroup;
                this.sound = sound;
                this.music = music;
                this.voice = voice;

                if(soundGroup == null && sound != null && sound.Length > 0)
                    Warning("The audio sound channel list is not empty, but the audio sound group is null");
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

                    if(subtitlesGroup != null)
                        this.subtitlesGroup = subtitlesGroup;

                    if(subtitles != null)
                    {
                        this.subtitles = subtitles.GetComponent<TextMeshProUGUI>();

                        if (this.subtitles == null)
                            Warning("Cinematic subtitles object does not have a TextMeshProUGUI component");
                        else if (this.subtitlesGroup == null)
                            Warning("Cinematic subtitles object is not null, but the cinematic subtitles group is null");
                    }

                    if(subtitlesBackground != null)
                    {
                        this.subtitlesBackground = subtitlesBackground.GetComponent<RectTransform>();

                        if(this.subtitlesBackground == null)
                            Warning("Cinematic subtitles background object does not have a RectTransform component");
                    }
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

                    if(this.image == null)
                        Warning("Cinematic background object does not have an Image component");

                    if (this.transform == null)
                        Warning("Cinematic background object does not have a RectTransform component");

                    if(backgroundAlt != null)
                    {
                        this.imageAlt = backgroundAlt.GetComponent<Image>();
                        this.transformAlt = backgroundAlt.GetComponent<RectTransform>();

                        if (this.imageAlt == null)
                            Warning("Cinematic background (alternative) object does not have an Image component");

                        if (this.transformAlt == null)
                            Warning("Cinematic background (alternative) object does not have a RectTransform component");
                    }
                }

                this.isOriginalActive = true;
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

                if(this.player != null) {
                    this.player.loopPointReached += TaleUtil.Events.OnCinematicVideoEnd;
                }
            }
        }

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