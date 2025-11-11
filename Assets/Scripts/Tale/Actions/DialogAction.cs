#pragma warning disable 0162 // Disable the 'unreachable code' warning caused by config constants.

using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace TaleUtil {
    public class DialogAction : Action {
        public enum Type {
            OVERRIDE,
            ADDITIVE
        }

        public enum State {
            SETUP,
            TRANSITION_IN,
            AVATAR_TRANSITION_IN,

            BEGIN_WRITE,
            WRITE,
            WAIT_FOR_INPUT_OVERRIDE,
            WAIT_FOR_INPUT_ADDITIVE,
            WAIT_FOR_ACTION,
            END_WRITE,

            AVATAR_TRANSITION_OUT,
            TRANSITION_OUT,

            END
        }

        public static bool autoMode = false;

        public string actor;
        public string content;
        public string avatar;
        public string voice;

        bool loopVoice;
        bool reverb;

        bool keepOpen;

        TaleUtil.Action action;

        public Type type;
        public State state;
        int index;
        int fadeStartIndex;
        int fadeEndIndex;

        TMP_TextInfo contentInfo;

        float timePerChar;
        float clock;
        float screenToWorldUnitX;
        float screenToWorldUnitY;

        bool hasAnimation;
        bool hasAvatarAnimation;

        public DialogAction Init(string actor, string content, string avatar, string voice, bool loopVoice, bool additive, bool reverb, bool keepOpen, TaleUtil.Action action) {
            if (content != null) {
                Debug.SoftAssert.Condition(master.Props.dialog.content != null,
                    "DialogAction requires a content object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");
            } else {
                Log.Warning("DIALOG", "DialogAction has null content; did you mean to do this?");
            }

            if (actor != null) {
                Debug.SoftAssert.Condition(master.Props.dialog.actor != null,
                    "DialogAction requires an actor object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");
            }

            if (voice != null) {
                if (!Debug.SoftAssert.Condition(master.Props.audio.group != null,
                        "A voice clip was passed to the dialog action, but no audio group prop is available; voice will be disabled")
                ||
                    !Debug.SoftAssert.Condition(master.Props.audio.voice != null,
                        "A voice clip was passed to the dialog action, but no audio voice prop is available; voice will be disabled")) {
                    voice = null;
                }
            } else {
                if (!Debug.SoftAssert.Condition(!reverb,
                    "DialogAction has no voice, but Reverb is set to true; ignoring")) {
                    reverb = false;
                }
            }

            if (reverb) {
                if (!Debug.SoftAssert.Condition(master.Props.audio.voiceReverb != null,
                    "Dialog action has Reverb set to true, but there is no AudioReverbFilter component on the Audio Voice object; reverb will be disabled")) {
                    reverb = false;
                }
            }

            this.actor = actor;
            this.content = content;
            this.avatar = avatar;
            this.voice = voice;
            this.loopVoice = loopVoice;
            this.reverb = reverb;
            this.keepOpen = keepOpen;
            this.action = action;

            type = additive ? Type.ADDITIVE : Type.OVERRIDE;
            ChangeState(State.SETUP);

            if (master.Config.Dialog.CPS > 0)
                timePerChar = 1f / master.Config.Dialog.CPS;
            else timePerChar = 0f;

            clock = 0f;

            index = (timePerChar == 0) ? content.Length : 0;

            hasAnimation = HasAnimation();
            hasAvatarAnimation = HasAnimateableAvatar();

            return this;
        }

        // After the dialog ends, we want to see if another dialog action
        // will be executed immediately afterwards. If so, keep the dialog canvas active.
        // This happens if the next action is dialog (standalone or inside Multiplex/Bind)
        DialogAction GetNextDialogAction(Action next) {
            if (next is DialogAction) {
                return (DialogAction)next;
            } else if (next is MultiplexAction) {
                var multi = (MultiplexAction)next;

                for (int i = 0; i < multi.actions.Count; ++i) {
                    var dialog = GetNextDialogAction(multi.actions[i]);

                    if (dialog != null) {
                        return dialog;
                    }
                }
            } else if (next is BindAction) {
                var bind = (BindAction)next;

                var dialog = GetNextDialogAction(bind.primary);

                if (dialog == null) {
                    dialog = GetNextDialogAction(bind.secondary);
                }

                if (dialog != null) {
                    return dialog;
                }
            }

            return null;
        }

        // Moves the CTC object to the end of the text.
        void RepositionCTC(TMP_TextInfo textInfo, RectTransform ctcTransform, float xOffset, float yOffset, TaleUtil.Config.CTCAlignment alignment) {
            // Note: This way of repositioning the CTC only works when the TextMeshProUGUI object has the horizontal alignment set to "Left" and the vertical alignment set to "Top".
            TMP_CharacterInfo lastCharInfo = textInfo.characterInfo[contentInfo.characterCount - 1];
            RectTransform contentTransform = master.Props.dialog.content.rectTransform;

            Vector3 bottomRight = contentTransform.TransformPoint(lastCharInfo.bottomRight);

            float x = bottomRight.x + xOffset * screenToWorldUnitX;
            float y = yOffset * screenToWorldUnitY;

            if (alignment == TaleUtil.Config.CTCAlignment.MIDDLE) {
                y += contentTransform.TransformPoint(
                    new Vector2(0,
                        contentInfo.lineInfo[contentInfo.lineCount - 1].descender + contentInfo.lineInfo[contentInfo.lineCount - 1].lineHeight / 2)).y;
            } else {
                y += contentTransform.TransformPoint(new Vector2(0, lastCharInfo.baseLine)).y;
            }

            ctcTransform.position = new Vector3(x, y, bottomRight.z);
        }

        bool HasAnimation() {
            if (master.Props.dialog.animator == null) {
                return false;
            }

            if (!master.Props.dialog.animator.HasStates(
                "Dialog",
                "Canvas animator doesn't have a state named {0} (this is needed for Tale)",
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_STATE_IN,
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_STATE_OUT
            )) {
                return false;
            }

            if (!master.Props.dialog.animator.HasTriggers(
                "Dialog",
                "Canvas animator doesn't have a trigger named {0} (this is needed for Tale)",
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN,
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT,
                TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL
            )) {
                return false;
            }

            return true;
        }

        bool HasAnimateableAvatar() {
            if (avatar == null || master.Props.dialog.avatar == null || master.Props.dialog.avatarAnimator == null) {
                return false;
            }

            if (!master.Props.dialog.avatarAnimator.HasStates(
                "Dialog",
                "Avatar animator doesn't have a state named {0} (this is needed for Tale)",
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_STATE_IN,
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_STATE_OUT
            )) {
                return false;
            }

            if (!master.Props.dialog.avatarAnimator.HasTriggers(
                "Dialog",
                "Avatar animator doesn't have a trigger named {0} (this is needed for Tale)",
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN,
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT,
                TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL
            )) {
                return false;
            }

            return true;
        }

        bool ActivateCanvasAnimation(string state) {
            if (hasAnimation) {
                master.Props.dialog.animator.SetTrigger(state);
                return true;
            }

            return false;
        }

        bool ActivateAvatarAnimation(string state) {
            if (hasAvatarAnimation) {
                master.Props.dialog.avatarAnimator.SetTrigger(state);
                return true;
            }

            return false;
        }

        bool ActivateCanvasAnimationIn() =>
            ActivateCanvasAnimation(TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN);
        bool ActivateCanvasAnimationOut() =>
            ActivateCanvasAnimation(TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT);

        bool ActivateAvatarAnimationIn() =>
            ActivateAvatarAnimation(TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN);
        bool ActivateAvatarAnimationOut() =>
            ActivateAvatarAnimation(TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT);

        void ActivateCanvasAnimationNeutral() {
            if (hasAnimation) {
                switch (master.Config.Dialog.ANIMATION_IN_MODE) {
                    case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                    case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
                        master.Props.dialog.animator.SetTrigger(TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);
                        break;
                    default:
                        break;
                }
            }
        }

        void ActivateAvatarAnimationNeutral() {
            if (master.Props.dialog.avatarAnimator != null) {
                switch (master.Config.Dialog.ANIMATION_IN_MODE) {
                    case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                    case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
                        master.Props.dialog.avatarAnimator.SetTrigger(TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);
                        break;
                    default:
                        break;
                }
            }
        }

        void OnPreRenderContentAlpha(TMP_TextInfo textInfo) {
            // TODO: instead of using a hook, do this:
            // - ForceMeshUpdate() in BEGIN_WRITE
            // - get rid of maxVisibleCharacters
            // - after the magic below: call master.Props.dialog.content.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32)
            // That should be way more efficient, since it doesn't force a mesh update for every single character

            for (int i = System.Math.Max(master.Props.dialog.content.maxVisibleCharacters - (int)master.Config.Dialog.FADE_FACTOR, fadeStartIndex); i < master.Props.dialog.content.maxVisibleCharacters; ++i) {
                if (!textInfo.characterInfo[i].isVisible) {
                    // This check is very important; without it, characters will flicker randomly
                    continue;
                }

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Color color = textInfo.meshInfo[materialIndex].colors32[vertexIndex];
                color.a = Mathf.Clamp01((1f / master.Config.Dialog.FADE_FACTOR) * (fadeEndIndex - i));

                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 0] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 1] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 2] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 3] = color;

                // TODO: add tint color support
            }
        }

        protected override bool Run() {
            if (TaleUtil.Input.GetKeyDown(master.Config.Dialog.KEY_AUTO)) {
                autoMode = !autoMode;

                // Edge case; reset auto mode clock
                if (state == State.WAIT_FOR_INPUT_ADDITIVE || state == State.WAIT_FOR_INPUT_OVERRIDE) {
                    clock = 0f;
                }

                if (master.Hooks.OnDialogAutoModeToggle != null) {
                    master.Hooks.OnDialogAutoModeToggle(autoMode);
                }
            }

            switch (state) {
                case State.SETUP: {
                    if (voice != null) {
                        if (master.Props.audio.group != null && !master.Props.audio.group.activeSelf) {
                            master.Props.audio.group.SetActive(true);
                        }

                        if (master.Props.audio.voice != null && !master.Props.audio.voice.gameObject.activeSelf) {
                            master.Props.audio.voice.gameObject.SetActive(true);
                        }

                        if (master.Props.audio.voiceReverb != null) {
                            master.Props.audio.voiceReverb.enabled = reverb;
                        }

                        master.Props.audio.voice.clip = Resources.Load<AudioClip>(voice);

                        Check(master.Props.audio.voice != null, string.Format("The voice audio clip '{0}' is missing", voice));

                        master.Props.audio.voice.loop = loopVoice;
                    }

                    // Dialog canvas is active; previous action was a Dialog action
                    if (master.Props.dialog.canvas.enabled) {
                        if (type == Type.ADDITIVE) {
                            content = master.Props.dialog.content.text + master.Config.Dialog.ADDITIVE_SEPARATOR + content;
                            index += master.Props.dialog.content.text.Length + master.Config.Dialog.ADDITIVE_SEPARATOR.Length;
                        }

                        ChangeState(State.BEGIN_WRITE);
                    } else {
                        if (!Debug.SoftAssert.Condition(type == Type.OVERRIDE, "Additive dialog must be preceded by a dialog action; setting type to Override")) {
                            type = Type.OVERRIDE;
                        }

                        if (actor != null) {
                            master.Props.dialog.actor.text = "";
                        }

                        master.Props.dialog.content.text = "";
                        master.Props.dialog.canvas.enabled = true;

                        // Activate the animations, in the order specified in the master.Config.
                        switch (master.Config.Dialog.ANIMATION_IN_MODE) {
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                                if (ActivateCanvasAnimationIn()) {
                                    ChangeState(State.TRANSITION_IN);
                                } else {
                                    if (ActivateAvatarAnimationIn()) {
                                        ChangeState(State.AVATAR_TRANSITION_IN);
                                    } else {
                                        ChangeState(State.BEGIN_WRITE);
                                    }
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                                if (ActivateCanvasAnimationIn()) {
                                    ChangeState(State.TRANSITION_IN);
                                } else {
                                    ActivateAvatarAnimationIn();
                                    ChangeState(State.BEGIN_WRITE);
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                                if (ActivateAvatarAnimationIn()) {
                                    ChangeState(State.AVATAR_TRANSITION_IN);
                                } else {
                                    if (ActivateCanvasAnimationIn()) {
                                        ChangeState(State.TRANSITION_IN);
                                    } else {
                                        ChangeState(State.BEGIN_WRITE);
                                    }
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                                if (ActivateAvatarAnimationIn()) {
                                    ChangeState(State.AVATAR_TRANSITION_IN);
                                } else {
                                    ActivateCanvasAnimationIn();
                                    ChangeState(State.BEGIN_WRITE);
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT: {
                                if (hasAnimation) {
                                    // The canvas is animated;
                                    // The avatar may or may not be animated, so try to activate its animation too
                                    ActivateCanvasAnimationIn();
                                    ActivateAvatarAnimationIn();
                                    ChangeState(State.TRANSITION_IN);
                                } else if (hasAvatarAnimation) {
                                    ActivateAvatarAnimationIn();
                                    ChangeState(State.AVATAR_TRANSITION_IN);
                                } else {
                                    ChangeState(State.BEGIN_WRITE);
                                }
                            }
                            break;
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
                                ActivateCanvasAnimationIn();
                                ActivateAvatarAnimationIn();
                                ChangeState(State.BEGIN_WRITE);
                                break;
                            default:
                                Log.Warning("Dialog", "Unreachable block reached: default case in SETUP (report this to the devs)");
                                ChangeState(State.BEGIN_WRITE);
                                break;
                        }
                    }

                    screenToWorldUnitX = ((float)Screen.width) / TaleUtil.Config.Editor.REFERENCE_WIDTH;
                    screenToWorldUnitY = ((float)Screen.height) / TaleUtil.Config.Editor.REFERENCE_HEIGHT;

                    if (avatar != null) {
                        Debug.SoftAssert.Condition(master.Props.dialog.avatar != null,
                            "An avatar was passed to the dialog action, but no avatar prop is available");

                        master.Props.dialog.avatar.sprite = (Sprite)Resources.Load<Sprite>(avatar);

                        Check(master.Props.dialog.avatar.sprite != null, string.Format("The avatar '{0}' is missing", avatar));
                    }

                    if (master.Config.Dialog.FADE_FACTOR > 0) {
                        master.Props.dialog.content.OnPreRenderText += OnPreRenderContentAlpha;
                    }

                    break;
                }
                case State.TRANSITION_IN: {
                    if (master.Input.Advance()) {
                        master.Props.dialog.animator.speed = master.Config.Transitions.SKIP_SPEED;
                    }

                    if (!master.Props.dialog.animator.StateFinished(TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_STATE_IN)) {
                        break;
                    }

                    master.Props.dialog.animator.speed = 1f;
                    master.Props.dialog.animator.SetTrigger(TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);

                    if (master.Props.dialog.avatarAnimator == null) {
                        ChangeState(State.BEGIN_WRITE);
                        break;
                    }

                    switch (master.Config.Dialog.ANIMATION_IN_MODE) {
                        case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                            ChangeState(State.BEGIN_WRITE);
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                            if (ActivateAvatarAnimationIn()) {
                                ChangeState(State.AVATAR_TRANSITION_IN);
                            } else {
                                ChangeState(State.BEGIN_WRITE);
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                            ActivateAvatarAnimationIn();
                            ChangeState(State.BEGIN_WRITE);
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT:
                            // Canvas animation done, now wait for the avatar animation
                            if (hasAvatarAnimation) {
                                ChangeState(State.AVATAR_TRANSITION_IN);
                            } else {
                                ChangeState(State.BEGIN_WRITE);
                            }
                            break;
                        default:
                            Log.Warning("Dialog", "Unreachable block reached: default case in TRANSITION_IN (report this to the devs)");
                            ChangeState(State.BEGIN_WRITE);
                            break;
                    }

                    break;
                }
                case State.AVATAR_TRANSITION_IN: {
                    if (master.Input.Advance()) {
                        master.Props.dialog.avatarAnimator.speed = master.Config.Transitions.SKIP_SPEED;
                    }

                    if (!master.Props.dialog.avatarAnimator.StateFinished(TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_STATE_IN))
                        break;

                    master.Props.dialog.avatarAnimator.speed = 1f;
                    master.Props.dialog.avatarAnimator.SetTrigger(TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);

                    if (master.Props.dialog.animator == null) {
                        ChangeState(State.BEGIN_WRITE);
                        break;
                    }

                    switch (master.Config.Dialog.ANIMATION_IN_MODE) {
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                            ChangeState(State.BEGIN_WRITE);
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                            if (ActivateCanvasAnimationIn()) {
                                ChangeState(State.TRANSITION_IN);
                            } else {
                                ChangeState(State.BEGIN_WRITE);
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                            ActivateCanvasAnimationIn();
                            ChangeState(State.BEGIN_WRITE);
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT:
                            ChangeState(State.BEGIN_WRITE);
                            break;
                        default:
                            Log.Warning("Dialog", "Unreachable block reached: default case in AVATAR_TRANSITION_IN (report this to the devs)");
                            ChangeState(State.BEGIN_WRITE);
                            break;
                    }

                    break;
                }
                case State.BEGIN_WRITE: {
                    if (actor != null) {
                        master.Props.dialog.actor.text = actor;
                    }

                    master.Props.dialog.content.text = content;

                    contentInfo = master.Props.dialog.content.textInfo;

                    master.Props.dialog.content.maxVisibleCharacters = index;
                    fadeStartIndex = index;
                    fadeEndIndex = index;

                    if (voice != null) {
                        master.Props.audio.voice.Play();
                    }

                    ChangeState(State.WRITE);

                    break;
                }
                case State.WRITE: {
                    if (fadeEndIndex < contentInfo.characterCount + master.Config.Dialog.FADE_FACTOR) {
                        clock += delta();

                        int numChars;

                        if (master.Input.Advance()) {
                            numChars = content.Length - index + (int)master.Config.Dialog.FADE_FACTOR;
                        } else {
                            numChars = (int)Mathf.Floor(clock / timePerChar);
                        }

                        if (numChars > 0) {
                            clock = clock % timePerChar;
                            index = Mathf.Min(index + numChars, contentInfo.characterCount);

                            fadeEndIndex = System.Math.Min(fadeEndIndex + numChars, contentInfo.characterCount + (int)master.Config.Dialog.FADE_FACTOR);

                            if (master.Props.dialog.content.maxVisibleCharacters == contentInfo.characterCount) {
                                // Done writing; wait for the fade effect to finish
                                master.Props.dialog.content.ForceMeshUpdate();
                            } else {
                                master.Props.dialog.content.maxVisibleCharacters = index;
                            }
                        }
                    } else {
                        if (loopVoice && master.Props.audio.voice != null) {
                            master.Props.audio.voice.loop = false;
                        }

                        clock = 0f;

                        if (action != null) {
                            // Run the custom action; don't wait for user input and don't show CTC
                            ChangeState(State.WAIT_FOR_ACTION);
                            break;
                        }

                        // If an additive dialog action follows this one,
                        // use the additive CTC
                        var nextDialog = GetNextDialogAction(Tale.Master.Queue.FetchNext());

                        if (nextDialog != null && nextDialog.type == Type.ADDITIVE) {
                            if (master.Props.dialog.actc != null) {
                                RepositionCTC(
                                    contentInfo,
                                    master.Props.dialog.actcTransform,
                                    master.Config.Dialog.CTC_ADDITIVE_OFFSET.x,
                                    master.Config.Dialog.CTC_ADDITIVE_OFFSET.y,
                                    master.Config.Dialog.CTC_ADDITIVE_ALIGNMENT);

                                master.Props.dialog.actc.SetActive(true);
                            }

                            ChangeState(State.WAIT_FOR_INPUT_ADDITIVE);
                        } else {
                            if (master.Props.dialog.ctc != null) {
                                RepositionCTC(
                                    contentInfo,
                                    master.Props.dialog.ctcTransform,
                                    master.Config.Dialog.CTC_OVERRIDE_OFFSET.x,
                                    master.Config.Dialog.CTC_OVERRIDE_OFFSET.y,
                                    master.Config.Dialog.CTC_OVERRIDE_ALIGNMENT);

                                master.Props.dialog.ctc.SetActive(true);
                            }

                            ChangeState(State.WAIT_FOR_INPUT_OVERRIDE);
                        }
                    }

                    break;
                }
                case State.WAIT_FOR_INPUT_OVERRIDE: {
                    if (autoMode) {
                        clock += delta();
                    }

                    if (master.Input.Advance() || autoMode && clock >= master.Config.Dialog.AUTO_DELAY) {
                        if (master.Props.dialog.ctc != null) {
                            master.Props.dialog.ctc.SetActive(false);
                        }
                        ChangeState(State.END_WRITE);

                        // If the animations were playing while the text was written,
                        // set the neutral trigger so that the animators return to the idle state.
                        ActivateCanvasAnimationNeutral();
                        ActivateAvatarAnimationNeutral();
                    }

                    break;
                }
                case State.WAIT_FOR_INPUT_ADDITIVE: {
                    if (autoMode) {
                        clock += delta();
                    }

                    if (master.Input.Advance() || autoMode && clock >= master.Config.Dialog.AUTO_DELAY) {
                        if (master.Props.dialog.actc != null) {
                            master.Props.dialog.actc.SetActive(false);
                        }
                        ChangeState(State.END_WRITE);

                        // If the animations were playing while the text was written,
                        // set the neutral trigger so that the animators return to the idle state.
                        ActivateCanvasAnimationNeutral();
                        ActivateAvatarAnimationNeutral();
                    }

                    break;
                }
                case State.WAIT_FOR_ACTION: {
                    if (action.Execute()) {
                        state = State.END_WRITE;
                    }

                    break;
                }
                case State.END_WRITE: {
                    // Disable the voice object and audio group
                    if (voice != null && master.Props.audio.voice != null && master.Props.audio.group != null) {
                        master.Props.audio.voice.Stop();
                        master.Props.audio.voice.gameObject.SetActive(false);

                        bool areSoundChannelsInactive = true;

                        if (master.Props.audio.sound != null) {
                            for (int i = 0; i < master.Props.audio.sound.Length; ++i) {
                                if (master.Props.audio.sound[i] != null && master.Props.audio.sound[i].gameObject.activeSelf) {
                                    areSoundChannelsInactive = false;
                                    break;
                                }
                            }
                        }

                        if (areSoundChannelsInactive) {
                            if (master.Props.audio.soundGroup != null) {
                                // Deactivate the sound group.
                                master.Props.audio.soundGroup.SetActive(false);
                            }

                            // Deactivate the audio group.
                            if (master.Props.audio.music == null || !master.Props.audio.music.gameObject.activeSelf) {
                                master.Props.audio.group.SetActive(false);
                            }
                        }
                    }

                    // Keep the dialog open
                    if (keepOpen) {
                        if (actor != null) {
                            master.Props.dialog.actor.text = "";
                        }
                        master.Props.dialog.content.text = "";

                        ChangeState(State.END);
                        return true;
                    }

                    // If the next action is a dialog, also keep open
                    var nextDialog = GetNextDialogAction(Tale.Master.Queue.FetchNext());

                    if (nextDialog != null) {
                        if (nextDialog.type == Type.OVERRIDE) {
                            if (actor != null) {
                                master.Props.dialog.actor.text = "";
                            }
                            master.Props.dialog.content.text = "";
                        }

                        ChangeState(State.END);
                        return true;
                    }

                    if (actor != null) {
                        master.Props.dialog.actor.text = "";
                    }
                    master.Props.dialog.content.text = "";

                    // Activate the OUT animations in the order specified in the master.Config.
                    switch (master.Config.Dialog.ANIMATION_OUT_MODE) {
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                            if (ActivateCanvasAnimationOut()) {
                                ChangeState(State.TRANSITION_OUT);
                            } else if (ActivateAvatarAnimationOut()) {
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            } else {
                                goto default; // No animations
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            if (ActivateAvatarAnimationOut()) {
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            } else if (ActivateCanvasAnimationOut()) {
                                ChangeState(State.TRANSITION_OUT);
                            } else {
                                goto default; // No animations
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_AVATAR: {
                            //bool hasAnimation = ActivateCanvasAnimationOut();
                            //hasAnimation = ActivateAvatarAnimationOut() || hasAnimation;

                            if (hasAnimation) {
                                // The canvas is animated;
                                // The avatar may or may not be animated, so try to activate its animation too
                                ActivateCanvasAnimationOut();
                                ActivateAvatarAnimationOut();
                                ChangeState(State.TRANSITION_OUT);
                            } else if (hasAvatarAnimation) {
                                ActivateAvatarAnimationOut();
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            } else {
                                ChangeState(State.BEGIN_WRITE);
                            }
                            break;
                        }
                        default:
                            master.Props.dialog.canvas.enabled = false;
                            ChangeState(State.END);
                            return true;
                    }

                    break;
                }
                case State.TRANSITION_OUT: {
                    if (master.Input.Advance()) {
                        master.Props.dialog.animator.speed = master.Config.Transitions.SKIP_SPEED;
                    }

                    if (!master.Props.dialog.animator.StateFinished(TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_STATE_OUT)) {
                        break;
                    }

                    master.Props.dialog.animator.speed = 1f;
                    master.Props.dialog.animator.SetTrigger(TaleUtil.Config.Editor.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);

                    if (master.Props.dialog.avatarAnimator == null) {
                        master.Props.dialog.canvas.enabled = false;
                        ChangeState(State.END);
                        return true;
                    }

                    switch (master.Config.Dialog.ANIMATION_OUT_MODE) {
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                            if (ActivateAvatarAnimationOut()) {
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            } else {
                                goto default;
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_AVATAR:
                            // Canvas animation finished, now wait for the avatar animation
                            if (hasAvatarAnimation) {
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            } else {
                                goto default; // There's no avatar, done
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                        // Fallthrough
                        default:
                            master.Props.dialog.canvas.enabled = false;
                            ChangeState(State.END);
                            return true;
                    }

                    break;
                }
                case State.AVATAR_TRANSITION_OUT: {
                    if (master.Input.Advance()) {
                        master.Props.dialog.avatarAnimator.speed = master.Config.Transitions.SKIP_SPEED;
                    }

                    if (!master.Props.dialog.avatarAnimator.StateFinished(TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_STATE_OUT)) {
                        break;
                    }

                    master.Props.dialog.avatarAnimator.speed = 1f;
                    master.Props.dialog.avatarAnimator.SetTrigger(TaleUtil.Config.Editor.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);

                    switch (master.Config.Dialog.ANIMATION_OUT_MODE) {
                        case TaleUtil.Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            if (ActivateCanvasAnimationOut()) {
                                ChangeState(State.TRANSITION_OUT);
                            } else {
                                goto default;
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_AVATAR:
                        // Canvas and avatar animations finished.
                        // Fallthrough
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                        // Fallthrough
                        default:
                            master.Props.dialog.canvas.enabled = false;
                            ChangeState(State.END);
                            return true;
                    }

                    break;
                }
                case State.END: {
                    Debug.Assert.Impossible("DialogAction.Run() called when action was already done");
                    return true;
                }
            }

            return false;
        }

        public override IEnumerable<Action> GetSubactions() {
            if (action == null) {
                yield break;
            }

            yield return action;
        }

        public override string ToString() =>
            string.Format("DialogAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), state.ToString());

        void ChangeState(State state) {
            this.state = state;

            if (state == State.END) {
                if (master.Config.Dialog.FADE_FACTOR > 0) {
                    master.Props.dialog.content.OnPreRenderText -= OnPreRenderContentAlpha;
                }
            }

            if (master.Hooks.OnDialogUpdate != null) {
                master.Hooks.OnDialogUpdate(this);
            }
        }
    }
}