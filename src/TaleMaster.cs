using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class TaleMaster : MonoBehaviour
{
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

    void Awake()
    {
        if(Tale.alive)
        {
            Destroy(gameObject);
            return;
        }

        TaleUtil.Queue.Init();
        TaleUtil.Parallel.Init();
        TaleUtil.Triggers.Init();

        TaleUtil.Props.Init(dialogCanvas, dialogActor, dialogContent, dialogAnimator, dialogCtc, dialogActc,
                            audioGroup, audioSoundGroup, audioSound, audioMusic, audioVoice,
                            cinematicCanvas, cinematicSubtitles, cinematicSubtitlesBackground, cinematicSubtitlesGroup,
                            cinematicBackgroundGroupAnimator, cinematicBackground, cinematicBackgroundAlt,
                            cinematicVideoGroup, cinematicVideoPlayer, cinematicVideoAudioSource,
                            transitions, cameraEffects);

        // Events.
        SceneManager.sceneLoaded += TaleUtil.Events.OnSceneLoaded;

        DontDestroyOnLoad(gameObject);
        Tale.alive = true;
    }

    void Update()
    {
        TaleUtil.Queue.Run();
        TaleUtil.Parallel.Run();
    }

    void LateUpdate()
    {
        TaleUtil.Triggers.Update();
    }
}
