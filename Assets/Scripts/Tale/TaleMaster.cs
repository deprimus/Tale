using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

// TODO: execution order is no longer required since the object is lazy init'ed
[DefaultExecutionOrder(-1000)]
public class TaleMaster : MonoBehaviour {
    #region Fields
    public TaleUtil.Queue Queue { get; private set; }
    public TaleUtil.Parallel Parallel { get; private set; }
    public TaleUtil.Props Props { get; private set; }
    public TaleUtil.Config Config { get { return config; } } // TODO: CopyOnWrite
    public TaleUtil.Input Input { get; private set; }
    public TaleUtil.Triggers Triggers { get; private set; }
    public TaleUtil.Hooks Hooks { get; private set; }
    public TaleUtil.Flags Flags { get; private set; }

    Dictionary<System.Type, Stack<TaleUtil.Action>> actionPool;

    ulong actionCounter;
    #endregion

    #region Behavior
    // Hello Tale!
    void Awake() {
        if (Tale.HasMaster()) {
            Destroy(gameObject);
            return;
        }

        Config.SanityCheck();

        if (Config.Core.APPLICATION_RUN_IN_BACKGROUND) {
            Application.runInBackground = true;
        }

        if (Config.Debug.SHOW_INFO_BY_DEFAULT) {
            if (TaleUtil.Debug.SoftAssert.Condition(props.debugMaster != null, "Debug info is enabled by default, but there is no DebugMaster object")) {
                props.debugMaster.ShowDebugInfo();
            }
        }

        actionPool = new Dictionary<System.Type, Stack<TaleUtil.Action>>();

        Queue = new TaleUtil.Queue(Config.Core.QUEUE_BASE_CAPACITY);
        Parallel = new TaleUtil.Parallel(Config.Core.PARALLEL_BASE_CAPACITY);
        Props = new TaleUtil.Props(props);
        Input = new TaleUtil.Input(this);
        Triggers = new TaleUtil.Triggers(this);
        Hooks = new TaleUtil.Hooks();
        Flags = new TaleUtil.Flags();

        actionCounter = 0;

        DontDestroyOnLoad(gameObject);
    }

    // The heart of Tale
    void Update() {
        if (Config.SceneSelector.ENABLE && TaleUtil.Input.GetKeyDown(Config.SceneSelector.KEY)) {
            TriggerSceneSelector();
            return;
        }

        // That's it.
        Queue.Run();
        Parallel.Run();
    }

    void LateUpdate() {
        Triggers.Update();
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Props.ReinitCamera();

        var config = Config.Core;

        if (config.QUEUE_VACUUM) {
            if (Queue.Count < (Queue.Capacity / config.QUEUE_VACUUM_FACTOR) && Queue.Capacity >= config.QUEUE_VACUUM_CAPACITY) {
                Queue.Vacuum();
            }
        }

        if (config.PARALLEL_VACUUM) {
            if (Parallel.Count < (Parallel.Capacity / config.PARALLEL_VACUUM_FACTOR) && Parallel.Capacity >= config.PARALLEL_VACUUM_CAPACITY) {
                Parallel.Vacuum();
            }
        }
    }

    void TriggerSceneSelector() {
        string path = "SceneSelector";

        if (SceneManager.GetSceneByPath(TaleUtil.Path.NormalizeResourcePath(TaleUtil.Config.Editor.ASSET_ROOT_SCENE, path)) == null) {
            return;
        }

        Queue.ForceClear();
        Parallel.ForceClear();

        Tale.Scene(path);
        Props.Reset();

        // Reset() places some transition cleaning actions on the parallel queue; execute all of them
        Parallel.Run();
    }
    #endregion

    #region Public Stuff
    public T CreateAction<T>() where T : TaleUtil.Action, new() {
        var pool = GetActionPool(typeof(T));

        if (!pool.TryPop(out var act)) {
            act = new T();
        }

        act.Inject(this, actionCounter++);

        return act as T;
    }
    public void ReturnAction(System.Type type, TaleUtil.Action action) {
        var pool = GetActionPool(type);
        var max = config.Core.ACTION_POOL_MAX_CAPACITY;

        if (max >= 0 && pool.Count >= max) {
            return;
        }

        GetActionPool(type).Push(action);
    }
    public ulong GetTotalActionCount() {
        return actionCounter;
    }
    #endregion

    Stack<TaleUtil.Action> GetActionPool(System.Type type) {
        if (!actionPool.TryGetValue(type, out var stack)) {
            stack = new Stack<TaleUtil.Action>();
            actionPool[type] = stack;
        }

        return stack;
    }

    #region Inspector
#if UNITY_EDITOR
    [Space(10)]
#endif
    [SerializeField]
    internal TaleUtil.Config config;

    [SerializeField]
    internal InspectorProps props;

    [System.Serializable]
    public class InspectorProps {
#if UNITY_EDITOR
        [Header("Dialog")]
        [Rename("Canvas")]
#endif
        public Canvas dialogCanvas;

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
        [Header("Choice")]
#endif
        public TaleUtil.Props.ChoiceStyle[] choiceStyles;

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
    #endregion
}
