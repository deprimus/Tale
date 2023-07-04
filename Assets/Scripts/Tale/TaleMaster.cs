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

        if (TaleUtil.Config.SHOW_DEBUG_INFO_BY_DEFAULT)
        {
            if (TaleUtil.SoftAssert.Condition(debugMaster != null, "Debug info is enabled by default, but there is no DebugMaster object"))
            {
                debugMaster.ShowDebugInfo();
            }
        }

        TaleUtil.Queue.Init();
        TaleUtil.Parallel.Init();
        TaleUtil.Triggers.Init();

        // Yes, this looks messy, but it's easier for the user to assign the raw objects and components in the inspector, and for
        // Tale to process them and make all necessary initializations and checks in Init(), than to force the user to do everything by themselves.
        // If I wanted to do everything by myself, I wouldn't have used Tale.
        //
        // Sure, in the future it would be nice to pass a single object containing all args to this Init() function, but it's fine for now.
        //
        // Also, everything is assigned to static variables so they are ready to be used by actions. Sure, a singleton is better practice,
        // but it's simpler with static variables.
        //
        // TaleUtil.Props.anything is cleaner than TaleUtil.Props.GetInstance().anything
        //
        // Keep it simple, stupid
        TaleUtil.Props.Init(dialogCanvas, dialogActor, dialogContent, dialogAvatar, dialogAnimator, dialogCtc, dialogActc,
                            audioGroup, audioSoundGroup, audioSound, audioMusic, audioVoice,
                            cinematicCanvas, cinematicSubtitles, cinematicSubtitlesBackground, cinematicSubtitlesGroup,
                            cinematicBackgroundGroupAnimator, cinematicBackground, cinematicBackgroundAlt,
                            cinematicVideoGroup, cinematicVideoPlayer, cinematicVideoAudioSource,
                            transitions, cameraEffects);

        // Events
        SceneManager.sceneLoaded += TaleUtil.Events.OnSceneLoaded; // This is used to re-assign the camera when the scene changes

        DontDestroyOnLoad(gameObject);
        Tale.alive = true;
    }

    // The heart of Tale ;)
    void Update()
    {
        // That's it. That's literally it. No fancy or complex stuff. Just Run().
        TaleUtil.Queue.Run();
        TaleUtil.Parallel.Run();
    }

    void LateUpdate()
    {
        TaleUtil.Triggers.Update();
    }

    // TODO: Add brackets to all switch cases in the project.
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
