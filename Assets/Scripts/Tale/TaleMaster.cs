#pragma warning disable 0162 // Disable the 'unreachable code' warning caused by config constants.

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class TaleMaster : MonoBehaviour
{
    // Hello Tale!
    void Awake()
    {
        if(Tale.alive)
        {
            Destroy(gameObject); // Prevent multiple master objects from existing at the same time.
            return;
        }

        if (TaleUtil.Config.APPLICATION_RUN_IN_BACKGROUND)
        {
            Application.runInBackground = true;
        }

        if (TaleUtil.Config.SHOW_DEBUG_INFO_BY_DEFAULT)
        {
            if (TaleUtil.SoftAssert.Condition(props.debugMaster != null, "Debug info is enabled by default, but there is no DebugMaster object"))
            {
                props.debugMaster.ShowDebugInfo();
            }
        }

        TaleUtil.Queue.Init();
        TaleUtil.Parallel.Init();
        TaleUtil.Triggers.Init();
        TaleUtil.Props.Init(props);

        // Events
        SceneManager.sceneLoaded += TaleUtil.Events.OnSceneLoaded; // This is used to re-assign the camera when the scene changes

        DontDestroyOnLoad(gameObject);
        Tale.alive = true;
    }

    // The heart of Tale
    void Update()
    {
        // That's it.
        TaleUtil.Queue.Run();
        TaleUtil.Parallel.Run();
    }

    void LateUpdate()
    {
        TaleUtil.Triggers.Update();
    }

    public InspectorProps props;

    [System.Serializable]
    public class InspectorProps
    {
#if UNITY_EDITOR
        [Header("Dialog")]
        [Rename("Canvas")]
#endif
        public GameObject dialogCanvas;

#if UNITY_EDITOR
        [Rename("Actor")]
#endif
        public GameObject dialogActor;

#if UNITY_EDITOR
        [Rename("Content")]
#endif
        public GameObject dialogContent;

#if UNITY_EDITOR
        [Rename("Avatar")]
#endif
        public GameObject dialogAvatar;

#if UNITY_EDITOR
        [Rename("Animator")]
#endif
        public Animator dialogAnimator;

#if UNITY_EDITOR
        [Space(10)]
        [Rename("CTC")]
#endif
        public GameObject dialogCtc;

#if UNITY_EDITOR
        [Rename("CTC (Additive)")]
#endif
        public GameObject dialogActc;


#if UNITY_EDITOR
        [Header("Audio")]
        [Rename("Group")]
#endif
        public GameObject audioGroup;

#if UNITY_EDITOR
        [Rename("Music")]
#endif
        public AudioSource audioMusic;

#if UNITY_EDITOR
        [Rename("Voice")]
#endif
        public AudioSource audioVoice;

#if UNITY_EDITOR
        [Rename("Sound Group")]
#endif
        public GameObject audioSoundGroup;

#if UNITY_EDITOR
        [Rename("Sound")]
#endif
        public AudioSource[] audioSound;


#if UNITY_EDITOR
        [Header("Cinematic")]
        [Rename("Canvas")]
#endif
        public GameObject cinematicCanvas;

#if UNITY_EDITOR
        [Rename("Subtitle Group")]
#endif
        public GameObject cinematicSubtitlesGroup;

#if UNITY_EDITOR
        [Rename("Subtitles")]
#endif
        public GameObject cinematicSubtitles;

#if UNITY_EDITOR
        [Rename("Subtitle Background")]
#endif
        public GameObject cinematicSubtitlesBackground;


#if UNITY_EDITOR
        [Header("Cinematic Background")]
        [Rename("Group Animator")]
#endif
        public Animator cinematicBackgroundGroupAnimator;


#if UNITY_EDITOR
        [Space(10)]
        [Rename("Background")]
#endif
        public GameObject cinematicBackground;

#if UNITY_EDITOR
        [Rename("Background (alternative)")]
#endif
        public GameObject cinematicBackgroundAlt;


#if UNITY_EDITOR
        [Header("Cinematic Video")]
        [Rename("Video Group")]
#endif
        public GameObject cinematicVideoGroup;


#if UNITY_EDITOR
        [Space(10)]
        [Rename("Video Player")]
#endif
        public VideoPlayer cinematicVideoPlayer;

#if UNITY_EDITOR
        [Rename("Audio Source")]
#endif
        public AudioSource cinematicVideoAudioSource;

#if UNITY_EDITOR
        [Space(20)]
#endif
        public GameObject advanceCanvas;


#if UNITY_EDITOR
        [Space(20)]
#endif
        public TaleUtil.Props.Transition[] transitions;


#if UNITY_EDITOR
        [Space(20)]
#endif
        public TaleUtil.Props.CameraEffect[] cameraEffects;

#if UNITY_EDITOR
        [Space(20)]
#endif
        public DebugMaster debugMaster;
    }
}
