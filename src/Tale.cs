using System.Collections.Generic;
using UnityEngine;

public static class Tale
{
    // Whether or not the TaleMaster object is alive (initially false).
    public static bool alive = false;

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

    public static UnityEngine.Color Color(byte value) =>
        Color(value, value, value);

    public static UnityEngine.Color Color(byte value, byte alpha) =>
        Color(value, value, value, alpha);

    public static UnityEngine.Color Color(byte red, byte green, byte blue, byte alpha = 255) =>
        new Color32(red, green, blue, alpha);

    public static class Interpolation
    {
        public static readonly TaleUtil.Delegates.InterpolationDelegate LINEAR      = TaleUtil.Math.Identity;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_IN     = TaleUtil.Math.QuadraticIn;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_OUT    = TaleUtil.Math.QuadraticOut;
        public static readonly TaleUtil.Delegates.InterpolationDelegate EASE_IN_OUT = TaleUtil.Math.ParametricBlend;
    }
    
    public static TaleUtil.Action MagicFix() =>
        TaleUtil.Queue.Enqueue(new TaleUtil.WaitAction(0.001f));

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

    public static TaleUtil.Action Transition(string name, TransitionType type, float duration = 1f) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.TransitionAction(name, (TaleUtil.TransitionAction.Type) (int) type, duration));

    public static  TaleUtil.Action Interpolate(float value, float target, TaleUtil.Delegates.CallbackDelegate<float> callback, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.InterpolationAction<float>(value, target, callback, duration, interpolation));

    public static TaleUtil.Action Interpolate(UnityEngine.Color value, UnityEngine.Color target, TaleUtil.Delegates.CallbackDelegate<UnityEngine.Color> callback, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.InterpolationAction<UnityEngine.Color>(value, target, callback, duration, interpolation));

    public static TaleUtil.Action Interpolate(Vector3 value, Vector3 target, TaleUtil.Delegates.CallbackDelegate<Vector3> callback, float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.InterpolationAction<Vector3>(value, target, callback, duration, interpolation));

    public static TaleUtil.Action Wait(float amount = 1f) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.WaitAction(amount));

    public static TaleUtil.Action Exec(TaleUtil.Delegates.ShallowDelegate action) =>
        TaleUtil.Queue.Enqueue(new TaleUtil.ExecAction(action));

    public static TaleUtil.Action Cinematic() =>
        TaleUtil.Queue.Enqueue(new TaleUtil.CinematicToggleAction());

    public static class Sound
    {
        public static TaleUtil.Action Play(string path, float volume = 1f, float pitch = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.SoundAction(0, TaleUtil.Path.Enroot(TaleUtil.Config.ASSET_ROOT_AUDIO_SOUND, path), volume, pitch));

        public static TaleUtil.Action Play(int channel, string path, float volume = 1f, float pitch = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.SoundAction(channel, TaleUtil.Path.Enroot(TaleUtil.Config.ASSET_ROOT_AUDIO_SOUND, path), volume, pitch));

        public static TaleUtil.Action Stop(int channel = 0) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.SoundAction(channel, null, 1f, 1f));
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
            TaleUtil.Queue.Enqueue(new TaleUtil.MusicAction(new List<string>(1) { TaleUtil.Path.Enroot(TaleUtil.Config.ASSET_ROOT_AUDIO_MUSIC, path) }, (TaleUtil.MusicAction.Mode) (int) mode, volume, pitch));

        public static TaleUtil.Action Play(string[] paths, PlayMode mode = PlayMode.ONCE, float volume = 1f, float pitch = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.MusicAction(TaleUtil.Path.Enroot(TaleUtil.Config.ASSET_ROOT_AUDIO_MUSIC, new List<string>(paths)), (TaleUtil.MusicAction.Mode) (int) mode, volume, pitch));

        public static TaleUtil.Action Play(List<string> paths, PlayMode mode = PlayMode.ONCE, float volume = 1f, float pitch = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.MusicAction(TaleUtil.Path.Enroot(TaleUtil.Config.ASSET_ROOT_AUDIO_MUSIC, paths), (TaleUtil.MusicAction.Mode) (int) mode, volume, pitch));

        public static TaleUtil.Action Stop(float duration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.MusicAction(duration, interpolation));
    }

    public static class Cam
    {
        public static TaleUtil.Action Position(Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformPositionAction(TaleUtil.Props.camera, pos, transitionDuration, interpolation, false));

        public static TaleUtil.Action Position(float x, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformPositionAction(TaleUtil.Props.camera, new Vector2(x, y), transitionDuration, interpolation, false));

        public static TaleUtil.Action Move(Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformPositionAction(TaleUtil.Props.camera, pos, transitionDuration, interpolation, true));

        public static TaleUtil.Action Move(float x, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformPositionAction(TaleUtil.Props.camera, new Vector2(x, y), transitionDuration, interpolation, true));

        public static TaleUtil.Action Zoom(float factor, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraZoomAction(factor, transitionDuration, interpolation));

        public static TaleUtil.Action Rotation(float degreesX, float degreesY, float degreesZ, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(TaleUtil.Props.camera, new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationX(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(TaleUtil.Props.camera, new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationY(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(TaleUtil.Props.camera, new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationZ(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(TaleUtil.Props.camera, new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation, false));

        public static TaleUtil.Action Rotate(float degreesX, float degreesY, float degreesZ, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(TaleUtil.Props.camera, new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateX(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(TaleUtil.Props.camera, new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateY(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(TaleUtil.Props.camera, new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateZ(float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(TaleUtil.Props.camera, new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation, true));

        public static TaleUtil.Action Effect(string name = null, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CameraEffectAction(name, transitionDuration, interpolation));

        public static TaleUtil.Action Vignette(float intensity, float transitionDuration = 1f, UnityEngine.Color? color = null, float smoothness = Default.FLOAT, float roundness = Default.FLOAT, bool? rounded = null, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.VignetteAction(intensity, transitionDuration, color, smoothness, roundness, rounded, interpolation));

        public static TaleUtil.Action Bloom(float intensity, float transitionDuration = 1f, UnityEngine.Color? color = null, float threshold = Default.FLOAT, float diffusion = Default.FLOAT, float anamorphicRatio = Default.FLOAT, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.BloomAction(intensity, transitionDuration, color, threshold, diffusion, anamorphicRatio, interpolation));
    }

    public static class Transform
    {
        public static TaleUtil.Action Position(UnityEngine.Transform transform, Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformPositionAction(transform, pos, transitionDuration, interpolation, false));

        public static TaleUtil.Action Position(UnityEngine.Transform transform, float x, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformPositionAction(transform, new Vector2(x, y), transitionDuration, interpolation, false));

        public static TaleUtil.Action Move(UnityEngine.Transform transform, Vector2 pos, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformPositionAction(transform, pos, transitionDuration, interpolation, true));

        public static TaleUtil.Action Move(UnityEngine.Transform transform, float x, float y = Default.FLOAT, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformPositionAction(transform, new Vector2(x, y), transitionDuration, interpolation, true));

        public static TaleUtil.Action Rotation(UnityEngine.Transform transform, float degreesX, float degreesY, float degreesZ, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(transform, new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationX(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(transform, new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationY(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(transform, new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation, false));

        public static TaleUtil.Action RotationZ(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(transform, new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation, false));

        public static TaleUtil.Action Rotate(UnityEngine.Transform transform, float degreesX, float degreesY, float degreesZ, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(transform, new Vector3(degreesX, degreesY, degreesZ), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateX(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(transform, new Vector3(degrees, Default.FLOAT, Default.FLOAT), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateY(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(transform, new Vector3(Default.FLOAT, degrees, Default.FLOAT), transitionDuration, interpolation, true));

        public static TaleUtil.Action RotateZ(UnityEngine.Transform transform, float degrees, float transitionDuration = 1f, TaleUtil.Delegates.InterpolationDelegate interpolation = null) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.TransformRotateAction(transform, new Vector3(Default.FLOAT, Default.FLOAT, degrees), transitionDuration, interpolation, true));
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
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicBackgroundAction(TaleUtil.Path.Enroot(TaleUtil.Config.ASSET_ROOT_CINEMATIC_BACKGROUND, path), (TaleUtil.CinematicBackgroundAction.Type) (int) type, speed));

        public static TaleUtil.Action Video(string path, float detatchValue = 0f, VideoDetatchType detatchType = VideoDetatchType.BEFORE, float speed = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicVideoAction(TaleUtil.Path.Enroot(TaleUtil.Config.ASSET_ROOT_CINEMATIC_VIDEO, path), detatchValue, (TaleUtil.CinematicVideoAction.DetatchType)(int) detatchType, speed));

        public static TaleUtil.Action VideoPause() =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicVideoPauseAction());

        public static TaleUtil.Action VideoResume(float detatchValue = 0f, VideoDetatchType detatchType = VideoDetatchType.BEFORE, float speed = 1f) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.CinematicVideoAction(null, detatchValue, (TaleUtil.CinematicVideoAction.DetatchType) (int) detatchType, speed));
    }
}