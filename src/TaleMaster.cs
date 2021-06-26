using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class TaleMaster : MonoBehaviour
{
    // TODO: Add brackets to all switch cases in the project.
    [Header("Dialog")]
    [Rename("Canvas")]
    public GameObject dialogCanvas;
    [Rename("Actor")]
    public GameObject dialogActor;
    [Rename("Content")]
    public GameObject dialogContent;
    [Rename("Animator")]
    public Animator dialogAnimator;

    [Space(10)]
    [Rename("CTC")]
    public GameObject dialogCtc;
    [Rename("CTC (Additive)")]
    public GameObject dialogActc;

    [Header("Audio")]
    [Rename("Group")]
    public GameObject audioGroup;
    [Rename("Music")]
    public AudioSource audioMusic;
    [Rename("Voice")]
    public AudioSource audioVoice;
    [Rename("Sound Group")]
    public GameObject audioSoundGroup;
    [Rename("Sound")]
    public AudioSource[] audioSound;

    [Header("Cinematic")]
    [Rename("Canvas")]
    public GameObject cinematicCanvas;
    [Rename("Subtitle Group")]
    public GameObject cinematicSubtitlesGroup;
    [Rename("Subtitles")]
    public GameObject cinematicSubtitles;
    [Rename("Subtitle Background")]
    public GameObject cinematicSubtitlesBackground;

    [Header("Cinematic Background")]
    [Rename("Group Animator")]
    public Animator cinematicBackgroundGroupAnimator;

    [Space(10)]
    [Rename("Background")]
    public GameObject cinematicBackground;
    [Rename("Background (alternative)")]
    public GameObject cinematicBackgroundAlt;

    [Header("Cinematic Video")]
    [Rename("Video Group")]
    public GameObject cinematicVideoGroup;

    [Space(10)]
    [Rename("Video Player")]
    public VideoPlayer cinematicVideoPlayer;
    [Rename("Audio Source")]
    public AudioSource cinematicVideoAudioSource;

    [Space(20)]
    public TaleUtil.Props.Transition[] transitions;

    [Space(20)]
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
}
