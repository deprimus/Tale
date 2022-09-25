using UnityEngine;

namespace TaleUtil
{
    public static class Config
    {
        public const uint DIALOG_CPS = 50; // Characters per second, aka how fast to type the text.
        public const uint CINEMATIC_SUBTITLE_CPS = 50;

        // Mouse left click or one of these keys -> advance the dialog
        public static readonly KeyCode[] DIALOG_KEY_NEXT = new KeyCode[] { KeyCode.Return, KeyCode.KeypadEnter };
        public const KeyCode DIALOG_KEY_SKIP = KeyCode.LeftControl; // Hold this key -> skip through the dialog

        public const string DIALOG_ADDITIVE_SEPARATOR = " "; // When there is additive dialog, append this after the first string.

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

        public const DialogAnimationInMode  DIALOG_ANIMATION_IN_MODE  = DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT;
        public const DialogAnimationOutMode DIALOG_ANIMATION_OUT_MODE = DialogAnimationOutMode.CANVAS_AVATAR;

        public enum CTCAlignment // Alignment modes for the Click-To-Continue (CTC) animation.
        {
            MIDDLE,
            BASELINE
        }

        public const CTCAlignment DIALOG_CTC_OVERRIDE_ALIGNMENT = CTCAlignment.MIDDLE; // CTC.
        public const CTCAlignment DIALOG_CTC_ADDITIVE_ALIGNMENT = CTCAlignment.MIDDLE; // Additive CTC.

        public const float DIALOG_CTC_OVERRIDE_OFFSET_X = 48f; // CTC offset on the X axis.
        public const float DIALOG_CTC_OVERRIDE_OFFSET_Y = 0f;  // CTC offset on the Y axis.
        public const float DIALOG_CTC_ADDITIVE_OFFSET_X = 48f; // Additive CTC offset on the X axis.
        public const float DIALOG_CTC_ADDITIVE_OFFSET_Y = 0f;  // Additive CTC offset on the Y axis.

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

        public const float  TRANSITION_SKIP_SPEED = 100f;     // When you hold the skip button
        public const float  TRANSITION_INSTANT_SPEED = 1000f; // When you pass 0f as the transition duration
        public const string TRANSITION_ANIMATOR_STATE_FORMAT = "Transition{0}";
        public const string TRANSITION_ANIMATOR_TRIGGER_FORMAT = "Transition{0}";
        public const string TRANSITION_ANIMATOR_TRIGGER_NEUTRAL = "Neutral";

        public const string ASSET_ROOT_AUDIO_SOUND = "Audio/Sound/";
        public const string ASSET_ROOT_AUDIO_MUSIC = "Audio/Music/";
        public const string ASSET_ROOT_CINEMATIC_BACKGROUND = "Cinematic/Backgrounds";
        public const string ASSET_ROOT_CINEMATIC_VIDEO = "Cinematic/Video";

        public const float CINEMATIC_SUBTITLE_BACKGROUND_PADDING_X = 5f;
        public const float CINEMATIC_SUBTITLE_BACKGROUND_PADDING_Y = 5f;

        public const string CINEMATIC_BACKGROUND_ANIMATOR_STATE_FORMAT = "CinematicBackground{0}";
        public const string CINEMATIC_BACKGROUND_ANIMATOR_TRIGGER = "Transition";
    }
}