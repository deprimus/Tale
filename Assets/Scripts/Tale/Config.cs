using UnityEngine;

namespace TaleUtil
{
    [CreateAssetMenu(fileName = "TaleConfig", menuName = "Tale/Config", order = 1)]
    public class Config : ScriptableObject
    {
        // Used for setting up Tale props
        public static class Setup
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
        }

#if UNITY_EDITOR
        [Rename("Run Game in Background")]
#endif
        public bool APPLICATION_RUN_IN_BACKGROUND = true; // If true, will set Application.runInBackground to true. Otherwise, the field is untouched.

#if UNITY_EDITOR
        [Header("Debug")]
        [Rename("Toggle Key")]
        public KeyCode DEBUG_INFO_KEY = KeyCode.F3; // Press this to show/hide Tale debug info
#endif

#if UNITY_EDITOR
        [Rename("Show by Default")]
#endif
        public bool SHOW_DEBUG_INFO_BY_DEFAULT = false; // Show/hide by default, but can still toggle by using DEBUG_INFO_KEY

#if UNITY_EDITOR
        [Header("Dialog")]
        [Rename("Chars Per Second")]
#endif
        public uint DIALOG_CPS = 50; // Characters per second, aka how fast to type the text.

#if UNITY_EDITOR
        [Header("Dialog")]
        [Rename("Character Fade Factor")]
#endif
        public uint DIALOG_FADE_FACTOR = 5; // How pronounced should the trailing character fade effect be.

        // Mouse left click or one of these keys -> advance the dialog
#if UNITY_EDITOR
        [Rename("Next Keys")]
#endif
        public KeyCode[] DIALOG_KEY_NEXT = new KeyCode[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.RightArrow, KeyCode.Space };

#if UNITY_EDITOR
        [Rename("Skip Key")]
#endif
        public KeyCode DIALOG_KEY_SKIP = KeyCode.LeftControl; // Hold this key -> skip through the dialog

#if UNITY_EDITOR
        [Rename("Additive Separator")]
#endif
        public string DIALOG_ADDITIVE_SEPARATOR = " "; // When there is additive dialog, append this after the first string.

#if UNITY_EDITOR
        [Rename("Override CTC Alignment")]
#endif
        public CTCAlignment DIALOG_CTC_OVERRIDE_ALIGNMENT = CTCAlignment.MIDDLE; // CTC
#if UNITY_EDITOR
        [Rename("Additive CTC Alignment")]
#endif
        public CTCAlignment DIALOG_CTC_ADDITIVE_ALIGNMENT = CTCAlignment.MIDDLE; // Additive CTC

#if UNITY_EDITOR
        [Rename("Override CTC Offset")]
#endif
        public Vector2 DIALOG_CTC_OVERRIDE_OFFSET = new Vector2(48f, 0f); // CTC offset

#if UNITY_EDITOR
        [Rename("Additive CTC Offset")]
#endif
        public Vector2 DIALOG_CTC_ADDITIVE_OFFSET = new Vector2(48f, 0f); // Additive CTC offset

#if UNITY_EDITOR
        [Rename("Animation IN Order")]
#endif
        public DialogAnimationInMode DIALOG_ANIMATION_IN_MODE = DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT;

#if UNITY_EDITOR
        [Rename("Animation OUT Order")]
#endif
        public DialogAnimationOutMode DIALOG_ANIMATION_OUT_MODE = DialogAnimationOutMode.CANVAS_AVATAR;

#if UNITY_EDITOR
        [Header("Transitions")]
        [Rename("Skip Speed")]
#endif
        public float TRANSITION_SKIP_SPEED = 100f;     // When you hold the skip button

#if UNITY_EDITOR
        [Rename("Instant Speed")]
#endif
        public float TRANSITION_INSTANT_SPEED = 1000f; // When you pass 0f as the transition duration

#if UNITY_EDITOR
        [Header("Cinematic")]
        [Rename("Subtitle Background Padding")]
#endif
        public Vector2 CINEMATIC_SUBTITLE_BACKGROUND_PADDING = new Vector2(5f, 5f);

#if UNITY_EDITOR
        [Rename("Subtitle Chars Per Second")]
#endif
        public uint CINEMATIC_SUBTITLE_CPS = 50;

#if UNITY_EDITOR
        [Header("Asset Roots")]
        [Rename("Scene Root")]
#endif
        public string ASSET_ROOT_SCENE = "Scenes/";
#if UNITY_EDITOR
        [Rename("Audio Sound Root")]
#endif
        public string ASSET_ROOT_AUDIO_SOUND = "Audio/Sound/";
#if UNITY_EDITOR
        [Rename("Audio Music Root")]
#endif
        public string ASSET_ROOT_AUDIO_MUSIC = "Audio/Music/";
#if UNITY_EDITOR
        [Rename("Audio Voice Root")]
#endif
        public string ASSET_ROOT_AUDIO_VOICE = "Audio/Voice/";
#if UNITY_EDITOR
        [Rename("Cinematic Background Root")]
#endif
        public string ASSET_ROOT_CINEMATIC_BACKGROUND = "Cinematic/Backgrounds";
#if UNITY_EDITOR
        [Rename("Cinematic Video Root")]
#endif
        public string ASSET_ROOT_CINEMATIC_VIDEO = "Cinematic/Video";

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