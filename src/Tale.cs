using UnityEngine;

public static class Tale
{
    // Do not change this.
    public static bool alive = false;

    public enum TransitionType
    {
        IN,
        OUT
    }

    public static class Default
    {
        public const float FLOAT = float.MinValue;
    }

    public static UnityEngine.Color Color(byte red, byte green, byte blue, byte alpha = 255) =>
        new Color32(red, green, blue, alpha);

    public static class Interpolation
    {
        public static readonly TaleUtil.Delegates.InterpolationDelegate LINEAR      = TaleUtil.Math.Identity;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_IN     = TaleUtil.Math.QuadraticIn;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_OUT    = TaleUtil.Math.QuadraticOut;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_IN_OUT = TaleUtil.Math.ParametricBlend;
    }

    public static TaleUtil.Action Multiplex(params TaleUtil.Action[] actions) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.MultiplexAction(actions));

    public static TaleUtil.Action Parallel(params TaleUtil.Action[] actions) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.ParallelAction(actions));

    public static TaleUtil.Action ParallelQueue(params TaleUtil.Action[] actions) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.ParallelAction(new TaleUtil.Action[] { new TaleUtil.ParallelQueueAction(actions) }));

    public static TaleUtil.Action Repeat(ulong count, params TaleUtil.Action[] actions) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.RepeatAction(count, actions));

    public static TaleUtil.Action Scene(int index = 1) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.SceneAction(index));
    public static TaleUtil.Action Scene(string path) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.SceneAction(path));

    public static TaleUtil.Action Dialog(string actor, string content, bool additive = false) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.DialogAction(actor, content, additive));

    // TODO: Replace the bool type with enum, and use a cast just like in Background().
    public static TaleUtil.Action Transition(string name, TransitionType type, float duration = 1f) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.TransitionAction(name, type == TransitionType.IN, duration));

    public static TaleUtil.Action Wait(float amount = 1f) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.WaitAction(amount));

    public static TaleUtil.Action Exec(TaleUtil.Delegates.ShallowDelegate action) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.ExecAction(action));

    public static TaleUtil.Action Cinematic() =>
        TaleUtil.Queue.Enqueue(new TaleUtil.CinematicToggleAction());

    public static class Sound
    {
        public static TaleUtil.Action Play(string path, float volume = 1f, float pitch = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.SoundAction(0, TaleUtil.Config.ASSET_ROOT_AUDIO_SOUND + path, volume, pitch));

        public static TaleUtil.Action Play(int channel, string path, float volume = 1f, float pitch = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.SoundAction(channel, TaleUtil.Config.ASSET_ROOT_AUDIO_SOUND + path, volume, pitch));

        public static TaleUtil.Action Stop(int channel = 0) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.SoundAction(channel, null, 1f, 1f));
    }

    public static class Cam
    {
        public static TaleUtil.Action Position(Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraPositionAction(pos, transitionDuration, interpolation));

        public static TaleUtil.Action Position(float x, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraPositionAction(new Vector2(x, y), transitionDuration, interpolation));

        public static TaleUtil.Action Move(Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraMoveAction(pos, transitionDuration, interpolation));

        public static TaleUtil.Action Move(float x, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraMoveAction(new Vector2(x, y), transitionDuration, interpolation));

        public static TaleUtil.Action Zoom(float factor, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraZoomAction(factor, transitionDuration, interpolation));

        public static TaleUtil.Action Rotate(float degreesX, float degreesY, float degreesZ, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraRotateAction(new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation));

        public static TaleUtil.Action RotateX(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraRotateAction(new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation));

        public static TaleUtil.Action RotateY(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraRotateAction(new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation));

        public static TaleUtil.Action RotateZ(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraRotateAction(new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation));

        public static TaleUtil.Action Effect(string name = null, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraEffectAction(name, transitionDuration, interpolation));

        public static TaleUtil.Action Vignette(float intensity, float transitionDuration = 1f, UnityEngine.Color? color = null, float smoothness = Default.FLOAT, float roundness = Default.FLOAT, bool? rounded = null, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.VignetteAction(intensity, transitionDuration, color, smoothness, roundness, rounded, interpolation));

        public static TaleUtil.Action Bloom(float intensity, float transitionDuration = 1f, UnityEngine.Color? color = null, float threshold = Default.FLOAT, float diffusion = Default.FLOAT, float anamorphicRatio = Default.FLOAT, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.BloomAction(intensity, transitionDuration, color, threshold, diffusion, anamorphicRatio, interpolation));
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

        // Preserve the same order as in CinematicVideoAction (the cast will silently fail otherwise).
        public enum VideoDetatchType
        {
            BEFORE,
            AFTER,
            FIXED
        }

        public static TaleUtil.Action Subtitles(string content, float ttl = 3f, bool showBackground = true) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicSubtitleAction(content, ttl, showBackground));

        public static TaleUtil.Action Background(string path, BackgroundTransitionType type = BackgroundTransitionType.INSTANT, float speed = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicBackgroundAction(TaleUtil.Config.ASSET_ROOT_CINEMATIC_BACKGROUND + path, (TaleUtil.CinematicBackgroundAction.Type) (int) type, speed));

        public static TaleUtil.Action Video(string path, float detatchValue = 0f, VideoDetatchType detatchType = VideoDetatchType.BEFORE, float speed = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicVideoAction(TaleUtil.Config.ASSET_ROOT_CINEMATIC_VIDEO + path, detatchValue, (TaleUtil.CinematicVideoAction.DetatchType)(int)detatchType, speed));

        public static TaleUtil.Action VideoPause() =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicVideoPauseAction());

        public static TaleUtil.Action VideoResume(float detatchValue = 0f, VideoDetatchType detatchType = VideoDetatchType.BEFORE, float speed = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicVideoAction(null, detatchValue, (TaleUtil.CinematicVideoAction.DetatchType)(int)detatchType, speed));
    }
}