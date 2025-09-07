using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    [CreateAssetMenu(fileName = "TaleConfig", menuName = "Tale/Config", order = 1)]
    public class Config : ScriptableObject
    {
        // Used for Editor stuff, like setting up Tale props
        public static class Editor
        {
            // Default width and height used for creating canvases, render textures, etc
            public const int REFERENCE_WIDTH = 1920;
            public const int REFERENCE_HEIGHT = 1080;

            public const string DIALOG_CANVAS_ANIMATOR_STATE_IN = "DialogIn";
            public const string DIALOG_CANVAS_ANIMATOR_STATE_OUT = "DialogOut";
            public const string DIALOG_CANVAS_ANIMATOR_TRIGGER_IN = "TransitionIn";
            public const string DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT = "TransitionOut";
            public const string DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL = "Neutral";

            public const string DIALOG_AVATAR_ANIMATOR_STATE_IN = "DialogAvatarIn";
            public const string DIALOG_AVATAR_ANIMATOR_STATE_OUT = "DialogAvatarOut";
            public const string DIALOG_AVATAR_ANIMATOR_TRIGGER_IN = "TransitionIn";
            public const string DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT = "TransitionOut";
            public const string DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL = "Neutral";

            public const string TRANSITION_ANIMATOR_STATE_FORMAT = "Transition{0}";
            public const string TRANSITION_ANIMATOR_TRIGGER_FORMAT = "Transition{0}";
            public const string TRANSITION_ANIMATOR_TRIGGER_NEUTRAL = "Neutral";

            public const string CINEMATIC_BACKGROUND_ANIMATOR_STATE_FORMAT = "CinematicBackground{0}";
            public const string CINEMATIC_BACKGROUND_ANIMATOR_TRIGGER = "Transition";

            public const int DEBUG_SORT_ORDER = 1000;
            public const int ADVANCE_SORT_ORDER = 400;
            public const int DIALOG_SORT_ORDER = 300;
            public const int TRANSITION_SORT_ORDER = 200;
            public const int CINEMATIC_SORT_ORDER = 100;

            public const string ASSET_ROOT_SCENE = "Scenes/";

            // Where to store scene thumbnails for the scene selector
            public const string ASSET_ROOT_SCENE_THUMBNAIL = "Sprites/SceneThumbnails";

            public const int SCENE_THUMBNAIL_WIDTH = REFERENCE_WIDTH / 10;
            public const int SCENE_THUMBNAIL_HEIGHT = REFERENCE_HEIGHT / 10;
        }

#if UNITY_EDITOR
        [Rename("Run Game in Background")]
        [Tooltip("If true, will set Application.runInBackground to true.\n\nOtherwise, the value is left untouched.")]
#endif
        public bool APPLICATION_RUN_IN_BACKGROUND = true;

#if UNITY_EDITOR
        [Header("Debug")]
        [Rename("Enable Debug Info")]
        [Tooltip("Enable showing and hiding in-game Tale debug info via the toggle key.")]
#endif
        public bool DEBUG_INFO_ENABLE = true;

#if UNITY_EDITOR
        [Rename("Toggle Key")]
        [Tooltip("Key used to toggle between showing and hiding in-game Tale debug info.")]
#endif
        public KeyCode DEBUG_INFO_KEY = KeyCode.F3;

#if UNITY_EDITOR
        [Rename("Show by Default")]
        [Tooltip("Show in-game Tale debug info by default.")]
#endif
        public bool SHOW_DEBUG_INFO_BY_DEFAULT = false;

#if UNITY_EDITOR
        [Header("Dialog")]
        [Rename("Chars Per Second")]
        [Tooltip("Dialog characters per second.\n\nThe higher it is, the faster dialog text is typed.")]
#endif
        public uint DIALOG_CPS = 50;

#if UNITY_EDITOR
        [Rename("Character Fade Factor")]
        [Tooltip("How pronounced should the dialog text trailing character fade effect be.")]
#endif
        public uint DIALOG_FADE_FACTOR = 5;

#if UNITY_EDITOR
        [Tooltip("Keys for advancing dialog. Mouse left click works by default.")]
#endif
        public KeyCode[] DIALOG_KEY_NEXT = new KeyCode[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.RightArrow, KeyCode.Space };

#if UNITY_EDITOR
        [Rename("Skip Key")]
        [Tooltip("Key for skipping dialog.")]
#endif
        public KeyCode DIALOG_KEY_SKIP = KeyCode.LeftControl;

#if UNITY_EDITOR
        [Rename("Auto Key")]
        [Tooltip("Key for toggling dialog auto mode.")]
#endif
        public KeyCode DIALOG_KEY_AUTO = KeyCode.F2; // Press this key -> dialog auto-advances

#if UNITY_EDITOR
        [Rename("Auto Delay")]
        [Tooltip("Seconds until dialog auto-advances when auto mode is enabled.\n\nStarts counting when dialog text is fully shown, and voice has finished playing.")]
#endif
        public float DIALOG_AUTO_DELAY = 1f; // How many seconds until dialog auto-advances in auto mode

#if UNITY_EDITOR
        [Rename("Additive Separator")]
        [Tooltip("When there is additive dialog, append this after the first string.\n\nBy default, this is a space (\" \").")]
#endif
        public string DIALOG_ADDITIVE_SEPARATOR = " ";

#if UNITY_EDITOR
        [Rename("CTC Alignment")]
        [Tooltip("How to align the normal dialog click-to-continue object relative to the text.")]
#endif
        public CTCAlignment DIALOG_CTC_OVERRIDE_ALIGNMENT = CTCAlignment.MIDDLE;
#if UNITY_EDITOR
        [Rename("Additive CTC Alignment")]
        [Tooltip("How to align the additive dialog click-to-continue object relative to the text.")]
#endif
        public CTCAlignment DIALOG_CTC_ADDITIVE_ALIGNMENT = CTCAlignment.MIDDLE;

#if UNITY_EDITOR
        [Rename("Override CTC Offset")]
        [Tooltip("Offset for normal dialog click-to-continue object, in canvas space.\n\nRelative to the last text character.")]
#endif
        public Vector2 DIALOG_CTC_OVERRIDE_OFFSET = new Vector2(48f, 0f);

#if UNITY_EDITOR
        [Rename("Additive CTC Offset")]
        [Tooltip("Offset for additive dialog click-to-continue object, in canvas space.\n\nRelative to the last text character.")]
#endif
        public Vector2 DIALOG_CTC_ADDITIVE_OFFSET = new Vector2(48f, 0f);

#if UNITY_EDITOR
        [Rename("Animation IN Order")]
        [Tooltip("The order in which to animate dialog elements when the dialog canvas appears.")]
#endif
        public DialogAnimationInMode DIALOG_ANIMATION_IN_MODE = DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT;

#if UNITY_EDITOR
        [Rename("Animation OUT Order")]
        [Tooltip("The order in which to animate dialog elements when the dialog canvas disappears.")]
#endif
        public DialogAnimationOutMode DIALOG_ANIMATION_OUT_MODE = DialogAnimationOutMode.CANVAS_AVATAR;

#if UNITY_EDITOR
        [Header("Transitions")]
        [Rename("Skip Speed")]
        [Tooltip("Animator speed used when a transition should be skipped (played faster).")]
#endif
        public float TRANSITION_SKIP_SPEED = 100f;

#if UNITY_EDITOR
        [Rename("Instant Speed")]
        [Tooltip("Animator speed used when a transition should be instant.\n\n(e.g. using a transition duration of 0)")]
#endif
        public float TRANSITION_INSTANT_SPEED = 1000f;

#if UNITY_EDITOR
        [Header("Cinematic")]
        [Rename("Subtitle Padding")]
        [Tooltip("Padding between cinematic subtitle text and background.")]
#endif
        public Vector2 CINEMATIC_SUBTITLE_BACKGROUND_PADDING = new Vector2(5f, 5f);

#if UNITY_EDITOR
        [Rename("Subtitle Chars Per Sec")]
        [Tooltip("Subtitle characters per second.\n\nThe higher it is, the faster subtitle text is typed.")]
#endif
        public uint CINEMATIC_SUBTITLE_CPS = 50;

#if UNITY_EDITOR
        [Header("Asset Roots")]
        [Rename("Audio Sound Root")]
        [Tooltip("Root path containing sounds.\n\nPaths passed to Tale.Sound are relative to this path.")]
#endif
        public string ASSET_ROOT_AUDIO_SOUND = "Audio/Sound/";
#if UNITY_EDITOR
        [Rename("Audio Music Root")]
        [Tooltip("Root path containing music.\n\nPaths passed to Tale.Music are relative to this path.")]
#endif
        public string ASSET_ROOT_AUDIO_MUSIC = "Audio/Music/";
#if UNITY_EDITOR
        [Rename("Audio Voice Root")]
        [Tooltip("Root path containing voices.\n\nVoice paths passed to Tale.Dialog are relative to this path.")]
#endif
        public string ASSET_ROOT_AUDIO_VOICE = "Audio/Voice/";
#if UNITY_EDITOR
        [Rename("Cinematic Background Root")]
        [Tooltip("Root path containing cinematic backgrounds.\n\nPaths passed to Tale.Cinematic.Background are relative to this path.")]
#endif
        public string ASSET_ROOT_CINEMATIC_BACKGROUND = "Cinematic/Backgrounds";
#if UNITY_EDITOR
        [Rename("Cinematic Video Root")]
        [Tooltip("Root path containing cinematic videos.\n\nPaths passed to Tale.Cinematic.Video are relative to this path.")]
#endif
        public string ASSET_ROOT_CINEMATIC_VIDEO = "Cinematic/Video";

#if UNITY_EDITOR
        [Header("Scene Selector")]
        [Rename("Enable Scene Selector")]
        [Tooltip("Enable the in-game scene selector.")]
#endif
        public bool SCENE_SELECTOR_ENABLE = true;

#if UNITY_EDITOR
        [Rename("Trigger Key")]
        [Tooltip("Key for showing the in-game scene selector.")]
#endif
        public KeyCode SCENE_SELECTOR_KEY = KeyCode.F12;

#if UNITY_EDITOR
        [Tooltip("List of scenes which shouldn't appear in the scene selector.\n\n(e.g. Assets/Scenes/MyScene)")]
#endif
        public List<string> SCENE_SELECTOR_BLACKLIST;

        // In which order to animate the avatar and dialog canvas before the dialog text is shown
        public enum DialogAnimationInMode
        {
            CANVAS_THEN_AVATAR_THEN_TEXT, // canvas -> avatar -> text
            CANVAS_THEN_AVATAR_TEXT,      // canvas -> avatar + text

            AVATAR_THEN_CANVAS_THEN_TEXT, // avatar -> canvas -> text
            AVATAR_THEN_CANVAS_TEXT,      // avatar -> canvas + text

            CANVAS_AVATAR_THEN_TEXT,      // canvas + avatar -> text
            CANVAS_AVATAR_TEXT            // canvas + avatar + text (all at the same time)
        }

        // In which order to animate the avatar and dialog canvas after the dialog text is hidden
        public enum DialogAnimationOutMode
        {
            CANVAS_THEN_AVATAR, // canvas -> avatar
            AVATAR_THEN_CANVAS, // avatar -> canvas
            CANVAS_AVATAR       // canvas + avatar (both at the same time)
        }

        public enum CTCAlignment // Alignment modes for the Click-To-Continue (CTC) animation.
        {
            MIDDLE,
            BASELINE
        }
    }
}