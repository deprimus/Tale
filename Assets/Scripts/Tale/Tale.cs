using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using TaleUtil;
using UnityEditor;
using UnityEngine.SceneManagement;

public static partial class Tale
{
    static TaleMaster master;

    public static TaleMaster Master {
        get {
            if (master == null) {
                InitTaleMaster();
            }

            return master;
        }
    }

    internal static bool HasMaster() =>
        master != null;

    static void InitTaleMaster() {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnExitPlayMode;
#endif

        // Part of the scene
        var obj = GameObject.FindFirstObjectByType<TaleMaster>();

        if (obj != null) {
            master = obj;
        } else {
            // Missing from scene; instantiate it now
            master = GameObject.Instantiate(Resources.Load<GameObject>(TaleUtil.Path.NormalizeResourcePath(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB))).GetComponent<TaleMaster>();
        }

        TaleUtil.Debug.Assert.Condition(master != null, "Master object Instantiate() returned false; something is seriously wrong");

        SceneManager.sceneLoaded += master.OnSceneLoaded;
    }

#if UNITY_EDITOR
    // This ensures that Tale works even without domain reloads
    static void OnExitPlayMode(PlayModeStateChange state) {
        if (state == PlayModeStateChange.ExitingPlayMode) {
            EditorApplication.playModeStateChanged -= OnExitPlayMode;

            if (master != null) {
                SceneManager.sceneLoaded -= master.OnSceneLoaded;
                master = null;
            }
        }
    }
#endif

    // Preserve the same order as in TransitionAction (the cast will silently fail otherwise).
    public enum TransitionType
    {
        IN,
        OUT
    }

    // TODO: replace float.MinValue with Tale.Default.FLOAT everywhere else.
    // TODO: make use of UNITY_POST_PROCESSING_STACK_V2 to check if PostProcessing is installed.

    public static class Default
    {
        public const float FLOAT = float.MinValue;
    }

    public static class Interpolation
    {
        public static readonly TaleUtil.Delegates.InterpolationDelegate LINEAR      = TaleUtil.Math.Identity;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_IN     = TaleUtil.Math.QuadraticIn;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_OUT    = TaleUtil.Math.QuadraticOut;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_IN_OUT = TaleUtil.Math.ParametricBlend;
    }

    // Starts executing an action and returns a task that can be awaited, bypassing the Tale action queue.
    public static Task Async(TaleUtil.Action action)
    {
        // Async actions are immediately placed on the parallel list
        Master.Queue.TakeLast(action);

        var task = new TaskCompletionSource<bool>();

        action.task = task;

        Master.Parallel.InsertMany(action);

        return task.Task;
    }

    public static TaleUtil.Action Multiplex(params TaleUtil.Action[] actions) {
        Master.Queue.TakeLast(actions);
        return Master.Queue.Enqueue(Master.CreateAction<MultiplexAction>().Init(actions));
    }

    public static TaleUtil.Action Any(params TaleUtil.Action[] actions) {
        Master.Queue.TakeLast(actions);
        return Master.Queue.Enqueue(Master.CreateAction<AnyAction>().Init(actions));
    }

    public static TaleUtil.Action Queue(params TaleUtil.Action[] actions) {
        Master.Queue.TakeLast(actions);
        return Master.Queue.Enqueue(Master.CreateAction<QueueAction>().Init(actions));
    }

    public static TaleUtil.Action Parallel(params TaleUtil.Action[] actions) {
        Master.Queue.TakeLast(actions);
        return Master.Queue.Enqueue(Master.CreateAction<ParallelAction>().Init(actions));
    }

    public static TaleUtil.Action Bind(TaleUtil.Action primary, TaleUtil.Action secondary) {
        Master.Queue.TakeLast(secondary);
        Master.Queue.TakeLast(primary);

        return Master.Queue.Enqueue(Master.CreateAction<BindAction>().Init(primary, secondary));
    }

    public static TaleUtil.Action Repeat(ulong count, TaleUtil.Delegates.ActionDelegate actionCallback) =>
        Master.Queue.Enqueue(Master.CreateAction<RepeatAction>().Init(count, () => {
            var act = actionCallback();
            Master.Queue.TakeLast(act);
            return act;
        }));

    public static TaleUtil.Action Trigger(string name) =>
        Master.Queue.Enqueue(Master.CreateAction<ExecAction>().Init(() => Master.Triggers.Set(name)));

    public static TaleUtil.Action Interruptible(string trigger, TaleUtil.Action action) {
        Master.Queue.TakeLast(action);
        return Master.Queue.Enqueue(Master.CreateAction<InterruptibleAction>().Init(trigger, action));
    }

    public static TaleUtil.Action Unscaled(TaleUtil.Action action)
    {
        action.SetDeltaCallback(() => Time.unscaledDeltaTime);
        return action;
    }

    public static TaleUtil.Action[] Map<T>(IReadOnlyList<T> arr, TaleUtil.Delegates.MapDelegate<T, TaleUtil.Action> callback)
    {
        var actions = new TaleUtil.Action[arr.Count];
        int i = 0;

        foreach (var obj in arr)
        {
            actions[i] = callback(obj, i);
            ++i;
        }

        return actions;
    }

    public static TaleUtil.Action Scene(int index = 1) =>
        Master.Queue.Enqueue(Master.CreateAction<SceneAction>().Init(index));
    public static TaleUtil.Action Scene(string path) =>
        Master.Queue.Enqueue(Master.CreateAction<SceneAction>().Init(TaleUtil.Path.NormalizeResourcePath(Config.Editor.ASSET_ROOT_SCENE, path)));

    public static TaleUtil.Action Dialog(string actor, string content, string avatar = null, string voice = null, bool loopVoice = false, bool additive = false, bool reverb = false, bool keepOpen = false, TaleUtil.Action action = null) {
        if (action != null) {
            Master.Queue.TakeLast(action);
        }

        return Master.Queue.Enqueue(Master.CreateAction<DialogAction>().Init(actor, content, avatar, voice != null ? TaleUtil.Path.NormalizeResourcePath(Master.Config.AssetRoots.AUDIO_VOICE, voice) : null, loopVoice, additive, reverb, keepOpen, action));
    }

    public static TaleUtil.Action TransitionIn(float duration = Tale.Default.FLOAT) =>
        Master.Queue.Enqueue(Master.CreateAction<TransitionAction>().Init(null, TransitionAction.Type.IN, duration));

    public static TaleUtil.Action TransitionOut(string name, float duration = 1f) =>
        Master.Queue.Enqueue(Master.CreateAction<TransitionAction>().Init(name, TransitionAction.Type.OUT, duration));

    public static  TaleUtil.Action Interpolate(float value, float target, TaleUtil.Delegates.CallbackDelegate<float> callback, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
        Master.Queue.Enqueue(Master.CreateAction<InterpolationAction<float>>().Init(value, target, callback, duration, interpolation));

    public static TaleUtil.Action Interpolate(UnityEngine.Color value, UnityEngine.Color target, TaleUtil.Delegates.CallbackDelegate<UnityEngine.Color> callback, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
        Master.Queue.Enqueue(Master.CreateAction<InterpolationAction<UnityEngine.Color>>().Init(value, target, callback, duration, interpolation));

    public static TaleUtil.Action Interpolate(Vector3 value, Vector3 target, TaleUtil.Delegates.CallbackDelegate<Vector3> callback, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
        Master.Queue.Enqueue(Master.CreateAction<InterpolationAction<Vector3>>().Init(value, target, callback, duration, interpolation));

    public static TaleUtil.Action Wait(float amount = 1f) =>
        Master.Queue.Enqueue(Master.CreateAction<WaitAction>().Init(amount));

    public static TaleUtil.Action WaitFor(string trigger) =>
        Master.Queue.Enqueue(Master.CreateAction<WaitForAction>().Init(trigger));

    public static TaleUtil.Action Delayed(float amount, TaleUtil.Action action) {
        Master.Queue.TakeLast(action);
        return Master.Queue.Enqueue(Master.CreateAction<DelayedAction>().Init(amount, action));
    }

    public static TaleUtil.Action DelayedBy(string trigger, TaleUtil.Action action) {
        Master.Queue.TakeLast(action);
        return Master.Queue.Enqueue(Master.CreateAction<DelayedByAction>().Init(trigger, action));
    }

    public static TaleUtil.Action Advance() =>
        Master.Queue.Enqueue(Master.CreateAction<AdvanceAction>().Init());

    public static TaleUtil.Action Exec(TaleUtil.Delegates.ShallowDelegate action) =>
        Master.Queue.Enqueue(Master.CreateAction<ExecAction>().Init(action));

    public static TaleUtil.Action Branch(string flag, TaleUtil.Delegates.BranchDelegate<ulong> action) =>
        Master.Queue.Enqueue(Master.CreateAction<BranchAction>().Init(flag, (value) => {
            var act = action(value);
            Master.Queue.TakeLast(act);

            return act;
        }));

    public static TaleUtil.Action Cinematic() =>
        Master.Queue.Enqueue(Master.CreateAction<CinematicToggleAction>().Init());

    public static TaleUtil.Action Animation(Animator animator, string trigger) =>
        Master.Queue.Enqueue(Master.CreateAction<ExecAction>().Init(() => animator.SetTrigger(trigger)));

    public static TaleUtil.Action Particles(ParticleSystem particles) =>
        Master.Queue.Enqueue(Master.CreateAction<ExecAction>().Init(() => particles.Play()));

    public static TaleUtil.Action SetActive(GameObject obj, bool value) =>
        Tale.Exec(() => obj.SetActive(value));

    public static class Sound
    {
        public static TaleUtil.Action Play(string path, float volume = 1f, float pitch = 1f) =>
            Parallel(Master.CreateAction<SoundAction>().Init(0, TaleUtil.Path.NormalizeResourcePath(Master.Config.AssetRoots.AUDIO_SOUND, path), volume, pitch));

        public static TaleUtil.Action Play(int channel, string path, float volume = 1f, float pitch = 1f) =>
            Parallel(Master.CreateAction<SoundAction>().Init(channel, TaleUtil.Path.NormalizeResourcePath(Master.Config.AssetRoots.AUDIO_SOUND, path), volume, pitch));

        public static TaleUtil.Action PlaySync(string path, float volume = 1f, float pitch = 1f) =>
            Queue(Play(path, volume, pitch), Sync());

        public static TaleUtil.Action PlaySync(int channel, string path, float volume = 1f, float pitch = 1f) =>
            Queue(Play(channel, path, volume, pitch), Sync(channel));

        public static TaleUtil.Action Stop(int channel = 0) =>
            Master.Queue.Enqueue(Master.CreateAction<SoundAction>().Init(channel, null, 1f, 1f));

        public static TaleUtil.Action Sync(int channel = 0, float syncTimestamp = Default.FLOAT) =>
            Master.Queue.Enqueue(Master.CreateAction<SoundAction>().Init(channel, syncTimestamp));
    }

    public static class Music
    {
        // Preserve the same order as in MusicAction (the cast will silently fail otherwise).
        public enum PlayMode
        {
            ONCE,
            LOOP,
            SHUFFLE,
            SHUFFLE_LOOP
        }

        // TODO: Change the asset root to MUSIC.
        public static TaleUtil.Action Play(string path, PlayMode mode = PlayMode.ONCE, float volume = 1f, float pitch = 1f) =>
            Parallel(Master.CreateAction<MusicAction>().Init(new List<string>(1) { TaleUtil.Path.NormalizeResourcePath(Master.Config.AssetRoots.AUDIO_MUSIC, path) }, (TaleUtil.MusicAction.Mode) (int) mode, volume, pitch));

        public static TaleUtil.Action Play(string[] paths, PlayMode mode = PlayMode.ONCE, float volume = 1f, float pitch = 1f) =>
            Parallel(Master.CreateAction<MusicAction>().Init(TaleUtil.Path.NormalizeAssetPath(Master.Config.AssetRoots.AUDIO_MUSIC, new List<string>(paths)), (TaleUtil.MusicAction.Mode) (int) mode, volume, pitch));

        public static TaleUtil.Action Play(List<string> paths, PlayMode mode = PlayMode.ONCE, float volume = 1f, float pitch = 1f) =>
            Parallel(Master.CreateAction<MusicAction>().Init(TaleUtil.Path.NormalizeAssetPath(Master.Config.AssetRoots.AUDIO_MUSIC, paths), (TaleUtil.MusicAction.Mode) (int) mode, volume, pitch));

        public static TaleUtil.Action Stop(float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<MusicAction>().Init(duration, interpolation));

        public static TaleUtil.Action Sync(float syncTimestamp = Default.FLOAT) =>
            Master.Queue.Enqueue(Master.CreateAction<MusicAction>().Init(syncTimestamp));

        // Async actions

        public static Task PlayAsync(string path, PlayMode mode = PlayMode.ONCE, float volume = 1f, float pitch = 1f) =>
            Async(Master.CreateAction<MusicAction>().Init(new List<string>(1) { TaleUtil.Path.NormalizeResourcePath(Master.Config.AssetRoots.AUDIO_MUSIC, path) }, (TaleUtil.MusicAction.Mode)(int)mode, volume, pitch));

        public static Task PlayAsync(string[] paths, PlayMode mode = PlayMode.ONCE, float volume = 1f, float pitch = 1f) =>
            Async(Master.CreateAction<MusicAction>().Init(TaleUtil.Path.NormalizeAssetPath(Master.Config.AssetRoots.AUDIO_MUSIC, new List<string>(paths)), (TaleUtil.MusicAction.Mode)(int)mode, volume, pitch));

        public static Task PlayAsync(List<string> paths, PlayMode mode = PlayMode.ONCE, float volume = 1f, float pitch = 1f) =>
            Async(Master.CreateAction<MusicAction>().Init(TaleUtil.Path.NormalizeAssetPath(Master.Config.AssetRoots.AUDIO_MUSIC, paths), (TaleUtil.MusicAction.Mode)(int)mode, volume, pitch));

        public static Task StopAsync(float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Async(Stop(duration, interpolation));

        public static Task SyncAsync(float syncTimestamp = Default.FLOAT) =>
            Async(Sync(syncTimestamp));
    }

    public static class Cam
    {
        public static TaleUtil.Action Position(Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformPositionAction>().Init(Master.Props.camera, pos, transitionDuration, interpolation, false));

        public static TaleUtil.Action Position(float x = Default.FLOAT, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformPositionAction>().Init(Master.Props.camera, new Vector2(x, y), transitionDuration, interpolation, false));

        public static TaleUtil.Action Move(Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformPositionAction>().Init(Master.Props.camera, pos, transitionDuration, interpolation, true));

        public static TaleUtil.Action Move(float x = Default.FLOAT, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformPositionAction>().Init(Master.Props.camera, new Vector2(x, y), transitionDuration, interpolation, true));

        public static TaleUtil.Action Zoom(float factor, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<CameraZoomAction>().Init(factor, transitionDuration, interpolation));

        public static TaleUtil.Action Rotation(float degreesX = Default.FLOAT, float degreesY = Default.FLOAT, float degreesZ = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(Master.Props.camera, new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationX(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(Master.Props.camera, new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationY(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(Master.Props.camera, new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationZ(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(Master.Props.camera, new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation, false));

        public static TaleUtil.Action Rotate(float degreesX = Default.FLOAT, float degreesY = Default.FLOAT, float degreesZ = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(Master.Props.camera, new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateX(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(Master.Props.camera, new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateY(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(Master.Props.camera, new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateZ(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(Master.Props.camera, new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation, true));

        public static TaleUtil.Action Shake(Vector2 magnitude, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformShakeAction>().Init(Master.Props.camera, magnitude, duration, interpolation));

        public static TaleUtil.Action Shake(float magnitude, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformShakeAction>().Init(Master.Props.camera, new Vector2(magnitude, magnitude), duration, interpolation));

        public static TaleUtil.Action ShakeX(float magnitude, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformShakeAction>().Init(Master.Props.camera, new Vector2(magnitude, Default.FLOAT), duration, interpolation));

        public static TaleUtil.Action ShakeY(float magnitude, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformShakeAction>().Init(Master.Props.camera, new Vector2(Default.FLOAT, magnitude), duration, interpolation));

#if UNITY_POST_PROCESSING_STACK_V2
        public static TaleUtil.Action Effect(string name = null, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(new TaleUtil.CameraEffectAction(name, transitionDuration, interpolation));

        public static TaleUtil.Action Vignette(float intensity, float transitionDuration = 1f, UnityEngine.Color? color = null, float smoothness = Default.FLOAT, float roundness = Default.FLOAT, bool? rounded = null, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(new TaleUtil.VignetteAction(intensity, transitionDuration, color, smoothness, roundness, rounded, interpolation));

        public static TaleUtil.Action Bloom(float intensity, float transitionDuration = 1f, UnityEngine.Color? color = null, float threshold = Default.FLOAT, float diffusion = Default.FLOAT, float anamorphicRatio = Default.FLOAT, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(new TaleUtil.BloomAction(intensity, transitionDuration, color, threshold, diffusion, anamorphicRatio, interpolation));
#endif
    }

    public static class Transform
    {
        public static TaleUtil.Action Position(UnityEngine.Transform transform, Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformPositionAction>().Init(transform, pos, transitionDuration, interpolation, false));

        public static TaleUtil.Action Position(UnityEngine.Transform transform, float x = Default.FLOAT, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformPositionAction>().Init(transform, new Vector2(x, y), transitionDuration, interpolation, false));

        public static TaleUtil.Action Move(UnityEngine.Transform transform, Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformPositionAction>().Init(transform, pos, transitionDuration, interpolation, true));

        public static TaleUtil.Action Move(UnityEngine.Transform transform, float x = Default.FLOAT, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformPositionAction>().Init(transform, new Vector2(x, y), transitionDuration, interpolation, true));

        public static TaleUtil.Action Rotation(UnityEngine.Transform transform, float degreesX = Default.FLOAT, float degreesY = Default.FLOAT, float degreesZ = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(transform, new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationX(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(transform, new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationY(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(transform, new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationZ(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(transform, new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation, false));

        public static TaleUtil.Action Rotate(UnityEngine.Transform transform, float degreesX = Default.FLOAT, float degreesY = Default.FLOAT, float degreesZ = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(transform, new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateX(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(transform, new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateY(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(transform, new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateZ(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformRotateAction>().Init(transform, new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation, true));

        public static TaleUtil.Action Shake(UnityEngine.Transform transform, Vector2 magnitude, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformShakeAction>().Init(transform, magnitude, duration, interpolation));

        public static TaleUtil.Action Shake(UnityEngine.Transform transform, float magnitude, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformShakeAction>().Init(transform, new Vector2(magnitude, magnitude), duration, interpolation));

        public static TaleUtil.Action ShakeX(UnityEngine.Transform transform, float magnitude, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformShakeAction>().Init(transform, new Vector2(magnitude, Default.FLOAT), duration, interpolation));

        public static TaleUtil.Action ShakeY(UnityEngine.Transform transform, float magnitude, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Master.Queue.Enqueue(Master.CreateAction<TransformShakeAction>().Init(transform, new Vector2(Default.FLOAT, magnitude), duration, interpolation));
    }

    public static partial class Choice
    {   
        // Generic types must match the style's master script generic types
        public static TaleUtil.Action Style<TArgs, TChoice>(string style, TArgs title, params TChoice[] choices) =>
            Master.Queue.Enqueue(Master.CreateAction<ChoiceAction<TArgs, TChoice>>().Init(style, title, choices));

        // Wrapper functions are declared by the scripts located at Scripts/Choice
    }

    public static class Cinema
    {
        // Preserve the same order as in CinematicBackgroundAction (the cast will silently fail otherwise).
        public enum BackgroundTransitionType
        {
            INSTANT,
            CUSTOM,
            CROSSFADE
        }

        public static TaleUtil.Action Subtitles(string content, float ttl = 3f, bool showBackground = true) =>
            Master.Queue.Enqueue(Master.CreateAction<CinematicSubtitleAction>().Init(content, ttl, showBackground));

        public static TaleUtil.Action Background(string path, BackgroundTransitionType type = BackgroundTransitionType.INSTANT, float speed = 1f) =>
            Master.Queue.Enqueue(Master.CreateAction<CinematicBackgroundAction>().Init(TaleUtil.Path.NormalizeResourcePath(Master.Config.AssetRoots.CINEMATIC_BACKGROUND, path), (TaleUtil.CinematicBackgroundAction.Type) (int) type, speed));

        public static class Video {
            public static TaleUtil.Action Play(string path = null, float speed = 1f) =>
                Master.Queue.Enqueue(Master.CreateAction<CinematicVideoAction>().Init(path != null ? TaleUtil.Path.NormalizeResourcePath(Master.Config.AssetRoots.CINEMATIC_VIDEO, path) : null, speed));

            public static TaleUtil.Action Sync(float syncTimestamp = Tale.Default.FLOAT) =>
                Master.Queue.Enqueue(Master.CreateAction<CinematicVideoAction>().Init(syncTimestamp));

            public static TaleUtil.Action Pause() =>
                Master.Queue.Enqueue(Master.CreateAction<CinematicVideoPauseAction>().Init());
        }
    }

    public static class Image
    {
        public static TaleUtil.Action Set(UnityEngine.UI.Image img, string path) =>
            Tale.Exec(() => img.sprite = Resources.Load<Sprite>(TaleUtil.Path.NormalizeResourcePath(path)));

        public static TaleUtil.Action Set(SpriteRenderer img, string path) =>
            Tale.Exec(() => img.sprite = Resources.Load<Sprite>(TaleUtil.Path.NormalizeResourcePath(path)));

        public static TaleUtil.Action Fade(UnityEngine.UI.Image img, float from, float to, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Tale.Interpolate(from, to, (value) => img.color = new UnityEngine.Color(img.color.r, img.color.g, img.color.b, value), transitionDuration, interpolation);

        public static TaleUtil.Action Fade(SpriteRenderer img, float from, float to, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Tale.Interpolate(from, to, (value) => img.color = new UnityEngine.Color(img.color.r, img.color.g, img.color.b, value), transitionDuration, interpolation);

        public static TaleUtil.Action FadeIn(UnityEngine.UI.Image img, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Fade(img, 0f, 1f, transitionDuration, interpolation);

        public static TaleUtil.Action FadeIn(SpriteRenderer img, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Fade(img, 0f, 1f, transitionDuration, interpolation);

        public static TaleUtil.Action FadeOut(UnityEngine.UI.Image img, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Fade(img, 1f, 0f, transitionDuration, interpolation);

        public static TaleUtil.Action FadeOut(SpriteRenderer img, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Fade(img, 1f, 0f, transitionDuration, interpolation);
    }

    public static class Text
    {
        public static TaleUtil.Action Set(TextMeshProUGUI text, string content) =>
            Tale.Exec(() => text.text = content);

        public static TaleUtil.Action Fade(TextMeshProUGUI text, float from, float to, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Tale.Interpolate(from, to, (value) => text.color = new UnityEngine.Color(text.color.r, text.color.g, text.color.b, value), transitionDuration, interpolation);

        public static TaleUtil.Action FadeIn(TextMeshProUGUI text, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Fade(text, 0f, 1f, transitionDuration, interpolation);

        public static TaleUtil.Action FadeOut(TextMeshProUGUI text, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            Fade(text, 1f, 0f, transitionDuration, interpolation);
    }
}

namespace System.Runtime.CompilerServices
{
    // Required for records to work in Unity
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}