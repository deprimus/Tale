using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace TaleUtil
{
    [CreateAssetMenu(fileName = "TaleConfig", menuName = "Tale/Config", order = 1)]
    public class Config : ScriptableObject {
#if UNITY_EDITOR
        [BoldFoldout]
#endif
        public CoreData Core;

#if UNITY_EDITOR
        [BoldFoldout]
#endif
        public DialogData Dialog;

#if UNITY_EDITOR
        [BoldFoldout]
#endif
        public TransitionsData Transitions;

#if UNITY_EDITOR
        [BoldFoldout]
#endif
        public CinematicData Cinematic;

#if UNITY_EDITOR
        [BoldFoldout]
#endif
        public AssetRootsData AssetRoots;

#if UNITY_EDITOR
        [BoldFoldout]
#endif
        public SceneSelectorData SceneSelector;

#if UNITY_EDITOR
        [BoldFoldout]
#endif
        public DebugData Debug;

        // Meta-config used for Editor stuff.
        // These are kept here since they must be accessible during the initial Tale setup,
        // and should not be changed after that.
        public static class Editor {
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
            public const int CHOICE_SORT_ORDER = 500;
            public const int CINEMATIC_SORT_ORDER = 100;

            public const string RESOURCE_MASTER_PREFAB = "Assets/Resources/Tale/Tale Master.prefab";
            public const string RESOURCE_SCENE_SELECTOR_ITEM_PREFAB = "Assets/Resources/Tale/TaleSceneSelectorItem.prefab";
            public const string RESOURCE_CONFIG = "Assets/Resources/Tale/TaleConfig.asset";
            public const string RESOURCE_DEBUG_INFO_MATERIAL = "Assets/Resources/Tale/TaleDebugInfo.mat";

            public const string ASSET_ROOT_SCENE = "Scenes/";
            public const string SPLASH_SCENE_DIR = "Splash";

            // Where to store scene thumbnails for the scene selector
            public const string RESOURCE_ROOT_SCENE_THUMBNAIL = "Assets/Resources/Sprites/SceneThumbnails";

            public const int SCENE_THUMBNAIL_WIDTH = REFERENCE_WIDTH / 10;
            public const int SCENE_THUMBNAIL_HEIGHT = REFERENCE_HEIGHT / 10;
        }

#if UNITY_EDITOR
        void OnValidate() {
            if (!Math.IsPowerOfTwo(Core.QUEUE_BASE_CAPACITY)) {
                Core.QUEUE_BASE_CAPACITY = Mathf.Max(2, Math.CeilPowerOfTwo(Core.QUEUE_BASE_CAPACITY));
            }

            if (!Math.IsPowerOfTwo(Core.QUEUE_VACUUM_CAPACITY)) {
                Core.QUEUE_VACUUM_CAPACITY = Mathf.Max(2, Math.CeilPowerOfTwo(Core.QUEUE_VACUUM_CAPACITY));
            }

            if (Core.QUEUE_VACUUM_FACTOR < 2) {
                Core.QUEUE_VACUUM_FACTOR = 2;
            }

            if (!Math.IsPowerOfTwo(Core.PARALLEL_BASE_CAPACITY)) {
                Core.PARALLEL_BASE_CAPACITY = Mathf.Max(2, Math.CeilPowerOfTwo(Core.PARALLEL_BASE_CAPACITY));
            }

            if (!Math.IsPowerOfTwo(Core.PARALLEL_VACUUM_CAPACITY)) {
                Core.PARALLEL_VACUUM_CAPACITY = Mathf.Max(2, Math.CeilPowerOfTwo(Core.PARALLEL_VACUUM_CAPACITY));
            }

            if (Core.PARALLEL_VACUUM_FACTOR < 2) {
                Core.PARALLEL_VACUUM_FACTOR = 2;
            }
        }
#endif

        [Conditional("UNITY_ASSERTIONS")]
        public void SanityCheck() {
            AssertPowOfTwo(Core.QUEUE_BASE_CAPACITY, "Queue Base Capacity");
            AssertPowOfTwo(Core.QUEUE_VACUUM_CAPACITY, "Queue Vacuum Capacity");
            AssertCond(Core.QUEUE_VACUUM_FACTOR > 1, "Queue Vacuum Factor must be at least 2");

            AssertPowOfTwo(Core.PARALLEL_BASE_CAPACITY, "Parallel Base Capacity");
            AssertPowOfTwo(Core.PARALLEL_VACUUM_CAPACITY, "Parallel Vacuum Capacity");
            AssertCond(Core.PARALLEL_VACUUM_FACTOR > 1, "Parallel Vacuum Factor must be at least 2");
        }

        [Conditional("UNITY_ASSERTIONS")]
        void AssertCond(bool cond, string what) {
            Assert.Condition(cond, string.Format("[CONFIG] {0}", what));
        }

        [Conditional("UNITY_ASSERTIONS")]
        void AssertPowOfTwo(int value, string what) {
            AssertCond(Math.IsPowerOfTwo(value), string.Format("{0} must be a power of 2", what));
        }

        [System.Serializable]
        public class CoreData {
#if UNITY_EDITOR
            [Rename("Run in Background")]
            [Tooltip("If true, will set Application.runInBackground to true.\n\nOtherwise, the value is left untouched.")]
#endif
            public bool APPLICATION_RUN_IN_BACKGROUND = true;

#if UNITY_EDITOR
            [Header("Action Pool")]
            [Rename("Max Capacity")]
            [Tooltip("Maximum capacity for Tale's action pool, per action type.\n\n-1 -> unlimited\n\n0 -> disable pooling")]
#endif
            public int ACTION_POOL_MAX_CAPACITY = 4096;

#if UNITY_EDITOR
            [Header("Queue")]
            [Rename("Base Capacity")]
            [Tooltip("Base capacity for Tale's action queue.\n\nMust be a power of 2.")]
#endif
            public int QUEUE_BASE_CAPACITY = 64;

#if UNITY_EDITOR
            [Rename("Vacuum")]
            [Tooltip("If enabled, Tale will attempt to shrink the action queue between scenes, so that it doesn't use too much memory.\n\nShould have a negligible impact on scene loading, but can be disabled.")]
#endif
            public bool QUEUE_VACUUM = true;

#if UNITY_EDITOR
            [Rename("Vacuum Capacity")]
            [Tooltip("If the action queue has grown to at least this capacity, Tale will attempt to shrink it between scenes.\n\nMust be a power of 2.")]
#endif
            public int QUEUE_VACUUM_CAPACITY = 8192;

#if UNITY_EDITOR
            [Rename("Vacuum Factor")]
            [Tooltip("If the above capacity is reached, and there are less than 1/factor actions in the queue, Tale will shrink the queue between scenes.\n\nMust be at least 2.")]
#endif
            public int QUEUE_VACUUM_FACTOR = 4;

#if UNITY_EDITOR
            [Header("Parallel List")]
            [Rename("Base Capacity")]
            [Tooltip("Base capacity for Tale's main parallel list.\n\nMust be a power of 2.")]
#endif
            public int PARALLEL_BASE_CAPACITY = 64;

#if UNITY_EDITOR
            [Rename("Vacuum")]
            [Tooltip("If enabled, Tale will attempt to shrink the main parallel list between scenes, so that it doesn't use too much memory.\n\nShould have a negligible impact on scene loading, but can be disabled.")]
#endif
            public bool PARALLEL_VACUUM = true;

#if UNITY_EDITOR
            [Rename("Vacuum Capacity")]
            [Tooltip("If the main parallel list has grown to at least this capacity, Tale will attempt to shrink it between scenes.\n\nMust be a power of 2.")]
#endif
            public int PARALLEL_VACUUM_CAPACITY = 8192;

#if UNITY_EDITOR
            [Rename("Vacuum Factor")]
            [Tooltip("If the above capacity is reached, and there are less than 1/factor actions in the queue, Tale will shrink the queue between scenes.\n\nMust be at least 2.")]
#endif
            public int PARALLEL_VACUUM_FACTOR = 4;
        }

        [System.Serializable]
        public class DialogData {
#if UNITY_EDITOR
            [Rename("Chars Per Second")]
            [Tooltip("Dialog characters per second.\n\nThe higher it is, the faster dialog text is typed.")]
#endif
            public uint CPS = 50;

#if UNITY_EDITOR
            [Rename("Character Fade Factor")]
            [Tooltip("How pronounced should the dialog text trailing character fade effect be.")]
#endif
            public uint FADE_FACTOR = 5;

#if UNITY_EDITOR
            [Rename("Additive Separator")]
            [Tooltip("When there is additive dialog, append this after the first string.\n\nBy default, this is a space (\" \").")]
#endif
            public string ADDITIVE_SEPARATOR = " ";

#if UNITY_EDITOR
            [Header("Keys")]
            [Tooltip("Keys for advancing dialog. Mouse left click works by default.")]
#endif
            public KeyCode[] KEY_NEXT = new KeyCode[] { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.RightArrow, KeyCode.Space };

#if UNITY_EDITOR
            [Rename("Skip Key")]
            [Tooltip("Key for skipping dialog.")]
#endif
            public KeyCode KEY_SKIP = KeyCode.LeftControl;

#if UNITY_EDITOR
            [Rename("Auto Key")]
            [Tooltip("Key for toggling dialog auto mode.")]
#endif
            public KeyCode KEY_AUTO = KeyCode.F2; // Press this key -> dialog auto-advances

#if UNITY_EDITOR
            [Rename("Auto Delay")]
            [Tooltip("Seconds until dialog auto-advances when auto mode is enabled.\n\nStarts counting when dialog text is fully shown, and voice has finished playing.")]
#endif
            public float AUTO_DELAY = 1f; // How many seconds until dialog auto-advances in auto mode

#if UNITY_EDITOR
            [Header("Click-to-Continue")]
            [Rename("CTC Alignment")]
            [Tooltip("How to align the normal dialog click-to-continue object relative to the text.")]
#endif
            public CTCAlignment CTC_OVERRIDE_ALIGNMENT = CTCAlignment.MIDDLE;
#if UNITY_EDITOR
            [Rename("Additive CTC Alignment")]
            [Tooltip("How to align the additive dialog click-to-continue object relative to the text.")]
#endif
            public CTCAlignment CTC_ADDITIVE_ALIGNMENT = CTCAlignment.MIDDLE;

#if UNITY_EDITOR
            [Rename("Override CTC Offset")]
            [Tooltip("Offset for normal dialog click-to-continue object, in canvas space.\n\nRelative to the last text character.")]
#endif
            public Vector2 CTC_OVERRIDE_OFFSET = new Vector2(48f, 0f);

#if UNITY_EDITOR
            [Rename("Additive CTC Offset")]
            [Tooltip("Offset for additive dialog click-to-continue object, in canvas space.\n\nRelative to the last text character.")]
#endif
            public Vector2 CTC_ADDITIVE_OFFSET = new Vector2(48f, 0f);

#if UNITY_EDITOR
            [Rename("Animation IN Order")]
            [Tooltip("The order in which to animate dialog elements when the dialog canvas appears.")]
#endif
            public DialogAnimationInMode ANIMATION_IN_MODE = DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT;

#if UNITY_EDITOR
            [Rename("Animation OUT Order")]
            [Tooltip("The order in which to animate dialog elements when the dialog canvas disappears.")]
#endif
            public DialogAnimationOutMode ANIMATION_OUT_MODE = DialogAnimationOutMode.CANVAS_AVATAR;
        }

        [System.Serializable]
        public class TransitionsData {
#if UNITY_EDITOR
            [Rename("Skip Speed")]
            [Tooltip("Animator speed used when a transition should be skipped (played faster).")]
#endif
            public float SKIP_SPEED = 100f;

#if UNITY_EDITOR
            [Rename("Instant Speed")]
            [Tooltip("Animator speed used when a transition should be instant.\n\n(e.g. using a transition duration of 0)")]
#endif
            public float INSTANT_SPEED = 1000f;
        }

        [System.Serializable]
        public class CinematicData {
#if UNITY_EDITOR
            [Rename("Subtitle Padding")]
            [Tooltip("Padding between cinematic subtitle text and background.")]
#endif
            public Vector2 SUBTITLE_BACKGROUND_PADDING = new Vector2(5f, 5f);

#if UNITY_EDITOR
            [Rename("Subtitle Chars Per Sec")]
            [Tooltip("Subtitle characters per second.\n\nThe higher it is, the faster subtitle text is typed.")]
#endif
            public uint SUBTITLE_CPS = 50;
    }

        [System.Serializable]
        public class AssetRootsData {
#if UNITY_EDITOR
            [Header("Audio")]
            [Rename("Sound")]
            [Tooltip("Root path containing sounds.\n\nPaths passed to Tale.Sound are relative to this path.")]
#endif
            public string AUDIO_SOUND = "Audio/Sound/";
#if UNITY_EDITOR
            [Rename("Music")]
            [Tooltip("Root path containing music.\n\nPaths passed to Tale.Music are relative to this path.")]
#endif
            public string AUDIO_MUSIC = "Audio/Music/";
#if UNITY_EDITOR
            [Rename("Voice")]
            [Tooltip("Root path containing voices.\n\nVoice paths passed to Tale.Dialog are relative to this path.")]
#endif
            public string AUDIO_VOICE = "Audio/Voice/";
#if UNITY_EDITOR
            [Header("Cinematic")]
            [Rename("Background")]
            [Tooltip("Root path containing cinematic backgrounds.\n\nPaths passed to Tale.Cinematic.Background are relative to this path.")]
#endif
            public string CINEMATIC_BACKGROUND = "Cinematic/Backgrounds";
#if UNITY_EDITOR
            [Rename("Video")]
            [Tooltip("Root path containing cinematic videos.\n\nPaths passed to Tale.Cinematic.Video are relative to this path.")]
#endif
            public string CINEMATIC_VIDEO = "Cinematic/Video";
        }

        [System.Serializable]
        public class SceneSelectorData {
#if UNITY_EDITOR
            [Rename("Enable")]
            [Tooltip("Enable the in-game scene selector.")]
#endif
            public bool ENABLE = true;

#if UNITY_EDITOR
            [Rename("Trigger Key")]
            [Tooltip("Key for showing the in-game scene selector.")]
#endif
            public KeyCode KEY = KeyCode.F12;

#if UNITY_EDITOR
            [Tooltip("List of scenes which shouldn't appear in the scene selector.\n\n(e.g. Assets/Scenes/MyScene)")]
#endif
            public List<string> BLACKLIST;
        }

        [System.Serializable]
        public class DebugData {
#if UNITY_EDITOR
            [Rename("Enable Debug Info")]
            [Tooltip("Enable showing and hiding in-game Tale debug info via the toggle key.")]
#endif
            public bool INFO_ENABLE = true;

#if UNITY_EDITOR
            [Rename("Toggle Key")]
            [Tooltip("Key used to toggle between showing and hiding in-game Tale debug info.")]
#endif
            public KeyCode INFO_KEY = KeyCode.F3;

#if UNITY_EDITOR
            [Rename("Show by Default")]
            [Tooltip("Show in-game Tale debug info by default.")]
#endif
            public bool SHOW_INFO_BY_DEFAULT = false;

#if UNITY_EDITOR
            [Rename("Text Color 1")]
            [Tooltip("Primary text color for debug info.")]
#endif
            public Color INFO_TEXT_COLOR_PRIMARY = new Color(1f, 1f, 1f);

#if UNITY_EDITOR
            [Rename("Text Color 2")]
            [Tooltip("Secondary text color for debug info.\nUsed for displaying running actions.")]
#endif
            public Color INFO_TEXT_COLOR_SECONDARY = new Color(0.59f, 1f, 0.59f);

#if UNITY_EDITOR
            [Rename("Accent Color 1")]
            [Tooltip("Primary accent color for debug info.\nUsed for displaying action states.")]
#endif
            public Color INFO_ACCENT_COLOR_PRIMARY = new Color(1f, 1f, 0f);

#if UNITY_EDITOR
            [Rename("Accent Color 2")]
            [Tooltip("Secondary accent color for debug info.\nUsed for displaying action arguments.")]
#endif
            public Color INFO_ACCENT_COLOR_SECONDARY = new Color(0f, 1f, 1f);
        }

        // In which order to animate the avatar and dialog canvas before the dialog text is shown
        public enum DialogAnimationInMode {
            CANVAS_THEN_AVATAR_THEN_TEXT, // canvas -> avatar -> text
            CANVAS_THEN_AVATAR_TEXT,      // canvas -> avatar + text

            AVATAR_THEN_CANVAS_THEN_TEXT, // avatar -> canvas -> text
            AVATAR_THEN_CANVAS_TEXT,      // avatar -> canvas + text

            CANVAS_AVATAR_THEN_TEXT,      // canvas + avatar -> text
            CANVAS_AVATAR_TEXT            // canvas + avatar + text (all at the same time)
        }

        // In which order to animate the avatar and dialog canvas after the dialog text is hidden
        public enum DialogAnimationOutMode {
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