using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public static class Config
    {
        public const uint DIALOG_CPS = 50;             // Characters per second.
        public const uint CINEMATIC_SUBTITLE_CPS = 50;

        public static readonly KeyCode[] DIALOG_KEY_NEXT = new KeyCode[] { KeyCode.Return, KeyCode.KeypadEnter };
        public const KeyCode DIALOG_KEY_SKIP = KeyCode.LeftControl;

        public const string DIALOG_ADDITIVE_SEPARATOR = " "; // When there is additive dialog, append this after the first string.

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

        public const float  TRANSITION_SKIP_SPEED = 100f;
        public const float  TRANSITION_INSTANT_SPEED = 1000f;
        public const string TRANSITION_ANIMATOR_STATE_FORMAT = "Transition{0}";
        public const string TRANSITION_ANIMATOR_TRIGGER_FORMAT = "Transition{0}";
        public const string TRANSITION_ANIMATOR_TRIGGER_NEUTRAL = "Neutral";

        public const string ASSET_ROOT_AUDIO_SOUND = "Audio/Sound/";
        public const string ASSET_ROOT_AUDIO_MUSIC = "Audio/Music/";
        public const string ASSET_ROOT_CINEMATIC_BACKGROUND = "Cinematic/";
        public const string ASSET_ROOT_CINEMATIC_VIDEO = "Cinematic/";

        public const float CINEMATIC_SUBTITLE_BACKGROUND_PADDING_X = 5f;
        public const float CINEMATIC_SUBTITLE_BACKGROUND_PADDING_Y = 5f;

        public const string CINEMATIC_BACKGROUND_ANIMATOR_STATE_FORMAT = "CinematicBackground{0}";
        public const string CINEMATIC_BACKGROUND_ANIMATOR_TRIGGER = "Transition";
    }
}