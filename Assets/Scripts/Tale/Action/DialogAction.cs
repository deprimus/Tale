#pragma warning disable 0162 // Disable the 'unreachable code' warning caused by config constants.

using UnityEngine;
using TMPro;
using UnityEditor;

namespace TaleUtil
{
    public class DialogAction : Action
    {
        public enum Type
        {
            OVERRIDE,
            ADDITIVE
        }

        public enum State
        {
            SETUP,
            TRANSITION_IN,
            AVATAR_TRANSITION_IN,

            BEGIN_WRITE,
            WRITE,
            WAIT_FOR_INPUT_OVERRIDE,
            WAIT_FOR_INPUT_ADDITIVE,
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

        DialogAction() { }

        public DialogAction(string actor, string content, string avatar, string voice, bool loopVoice, bool additive, bool reverb)
        {
            if (content != null)
            {
                SoftAssert.Condition(Props.dialog.content != null,
                    "DialogAction requires a content object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");
            }
            else
            {
                Log.Warning("DIALOG", "DialogAction has null content; did you mean to do this?");
            }

            if (actor != null)
            {
                SoftAssert.Condition(Props.dialog.actor != null,
                    "DialogAction requires an actor object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");
            }

            if (reverb)
            {
                if (!SoftAssert.Condition(Props.audio.voiceReverb != null,
                    "DialogAction has Reverb set to true, but there is no AudioReverbFilter component on the Audio Voice prop; reverb will be disabled"))
                {
                    reverb = false;
                }
            }

            if (voice != null)
            {
                if (!SoftAssert.Condition(Props.audio.group != null,
                        "A voice clip was passed to the dialog action, but no audio group prop is available; voice will be disabled")
                ||
                    !SoftAssert.Condition(Props.audio.voice != null,
                        "A voice clip was passed to the dialog action, but no audio voice prop is available; voice will be disabled"))
                {
                    voice = null;
                }
            }
            else
            {
                if (!SoftAssert.Condition(!reverb,
                    "DialogAction has no voice, but Reverb is set to true; ignoring"))
                {
                    reverb = false;
                }
            }

            if (reverb)
            {
                if (!SoftAssert.Condition(Props.audio.voiceReverb != null,
                    "Dialog action has Reverb set to true, but there is no AudioReverbFilter component on the Audio Voice object; reverb will be disabled"))
                {
                    reverb = false;
                }
            }

            this.actor = actor;
            this.content = content;
            this.avatar = avatar;
            this.voice = voice;
            this.loopVoice = loopVoice;
            this.reverb = reverb;

            type = additive ? Type.ADDITIVE : Type.OVERRIDE;
            ChangeState(State.SETUP);

            if(Tale.config.DIALOG_CPS > 0)
                timePerChar = 1f / Tale.config.DIALOG_CPS;
            else timePerChar = 0f;

            clock = 0f;

            index = (timePerChar == 0) ? content.Length : 0;

            hasAnimation = HasAnimation();
            hasAvatarAnimation = HasAnimateableAvatar();
        }

        // After the dialog ends, we want to see if another dialog action
        // will be executed immediately afterwards. If so, keep the dialog canvas active.
        // This happens if the next action is dialog (standalone or inside Multiplex/Bind)
        DialogAction GetNextDialogAction(Action next)
        {
            if (next is DialogAction)
            {
                return (DialogAction)next;
            }
            else if (next is MultiplexAction)
            {
                var multi = (MultiplexAction)next;

                foreach (Action act in multi.actions)
                {
                    var dialog = GetNextDialogAction(act);

                    if (dialog != null)
                    {
                        return dialog;
                    }
                }
            }
            else if (next is BindAction)
            {
                var bind = (BindAction)next;

                var dialog = GetNextDialogAction(bind.primary);

                if (dialog == null)
                {
                    dialog = GetNextDialogAction(bind.secondary);
                }

                if (dialog != null)
                {
                    return dialog;
                }
            }

            return null;
        }

        // Moves the CTC object to the end of the text.
        void RepositionCTC(TMP_TextInfo textInfo, RectTransform ctcTransform, float xOffset, float yOffset, TaleUtil.Config.CTCAlignment alignment)
        {
            // Note: This way of repositioning the CTC only works when the TextMeshProUGUI object has the horizontal alignment set to "Left" and the vertical alignment set to "Top".
            TMP_CharacterInfo lastCharInfo = textInfo.characterInfo[contentInfo.characterCount - 1];
            RectTransform contentTransform = Props.dialog.content.rectTransform;

            Vector3 bottomRight = contentTransform.TransformPoint(lastCharInfo.bottomRight);

            float x = bottomRight.x + xOffset * screenToWorldUnitX;
            float y = yOffset * screenToWorldUnitY;

            if (alignment == TaleUtil.Config.CTCAlignment.MIDDLE)
            {
                y += contentTransform.TransformPoint(
                    new Vector2(0,
                        contentInfo.lineInfo[contentInfo.lineCount - 1].descender + contentInfo.lineInfo[contentInfo.lineCount - 1].lineHeight / 2)).y;
            }
            else
            {
                y += contentTransform.TransformPoint(new Vector2(0, lastCharInfo.baseLine)).y;
            }

            ctcTransform.position = new Vector3(x, y, bottomRight.z);
        }

        bool HasAnimation()
        {
            if (Props.dialog.animator == null)
            {
                return false;
            }

            if (!Props.dialog.animator.HasStates(
                "Dialog",
                "Canvas animator doesn't have a state named {0} (this is needed for Tale)",
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_STATE_IN,
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_STATE_OUT
            ))
            {
                return false;
            }

            if (!Props.dialog.animator.HasTriggers(
                "Dialog",
                "Canvas animator doesn't have a trigger named {0} (this is needed for Tale)",
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN,
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT,
                TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL
            ))
            {
                return false;
            }

            return true;
        }

        bool HasAnimateableAvatar()
        {
            if (avatar == null || Props.dialog.avatar == null || Props.dialog.avatarAnimator == null)
            {
                return false;
            }

            if (!Props.dialog.avatarAnimator.HasStates(
                "Dialog",
                "Avatar animator doesn't have a state named {0} (this is needed for Tale)",
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_STATE_IN,
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_STATE_OUT
            ))
            {
                return false;
            }

            if (!Props.dialog.avatarAnimator.HasTriggers(
                "Dialog",
                "Avatar animator doesn't have a trigger named {0} (this is needed for Tale)",
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN,
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT,
                TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL
            ))
            {
                return false;
            }

            return true;
        }

        bool ActivateCanvasAnimation(string state)
        {
            if (hasAnimation)
            {
                Props.dialog.animator.SetTrigger(state);
                return true;
            }

            return false;
        }

        bool ActivateAvatarAnimation(string state)
        {
            if (hasAvatarAnimation)
            {
                Props.dialog.avatarAnimator.SetTrigger(state);
                return true;
            }

            return false;
        }

        bool ActivateCanvasAnimationIn() =>
            ActivateCanvasAnimation(TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN);
        bool ActivateCanvasAnimationOut() =>
            ActivateCanvasAnimation(TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT);

        bool ActivateAvatarAnimationIn() =>
            ActivateAvatarAnimation(TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN);
        bool ActivateAvatarAnimationOut() =>
            ActivateAvatarAnimation(TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT);

        void ActivateCanvasAnimationNeutral()
        {
            if (hasAnimation)
            {
                switch (Tale.config.DIALOG_ANIMATION_IN_MODE)
                {
                    case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                    case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
                        Props.dialog.animator.SetTrigger(TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);
                        break;
                    default:
                        break;
                }
            }
        }

        void ActivateAvatarAnimationNeutral()
        {
            if (Props.dialog.avatarAnimator != null)
            {
                switch (Tale.config.DIALOG_ANIMATION_IN_MODE)
                {
                    case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                    case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
                        Props.dialog.avatarAnimator.SetTrigger(TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);
                        break;
                    default:
                        break;
                }
            }
        }

        void OnPreRenderContentAlpha(TMP_TextInfo textInfo)
        {
            // TODO: instead of using a hook, do this:
            // - ForceMeshUpdate() in BEGIN_WRITE
            // - get rid of maxVisibleCharacters
            // - after the magic below: call Props.dialog.content.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32)
            // That should be way more efficient, since it doesn't force a mesh update for every single character

            for (int i = System.Math.Max(Props.dialog.content.maxVisibleCharacters - (int) Tale.config.DIALOG_FADE_FACTOR, fadeStartIndex); i < Props.dialog.content.maxVisibleCharacters; ++i)
            {
                if (!textInfo.characterInfo[i].isVisible)
                {
                    // This check is very important; without it, characters will flicker randomly
                    continue;
                }

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Color color = textInfo.meshInfo[materialIndex].colors32[vertexIndex];
                color.a = Mathf.Clamp01((1f / Tale.config.DIALOG_FADE_FACTOR) * (fadeEndIndex - i));

                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 0] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 1] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 2] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 3] = color;

                // TODO: add tint color support
            }
        }

        public override Action Clone()
        {
            DialogAction clone = new DialogAction();
            clone.delta = delta;
            clone.actor = actor;
            clone.content = content;
            clone.avatar = avatar;
            clone.voice = voice;
            clone.loopVoice = loopVoice;
            clone.type = type;
            clone.state = state;
            clone.index = index;
            clone.timePerChar = timePerChar;
            clone.clock = clock;

            return clone;
        }

        public override bool Run()
        {
            if (UnityEngine.Input.GetKeyDown(Tale.config.DIALOG_KEY_AUTO))
            {
                autoMode = !autoMode;

                // Edge case; reset auto mode clock
                if (state == State.WAIT_FOR_INPUT_ADDITIVE || state == State.WAIT_FOR_INPUT_OVERRIDE)
                {
                    clock = 0f;
                }

                if (Hooks.OnDialogAutoModeToggle != null)
                {
                    Hooks.OnDialogAutoModeToggle(autoMode);
                }
            }

            switch(state)
            {
                case State.SETUP:
                {
                    if (voice != null)
                    {
                        if (Props.audio.group != null && !Props.audio.group.activeSelf)
                        {
                            Props.audio.group.SetActive(true);
                        }

                        if (Props.audio.voice != null && !Props.audio.voice.gameObject.activeSelf)
                        {
                            Props.audio.voice.gameObject.SetActive(true);
                        }

                        if (Props.audio.voiceReverb != null)
                        {
                            Props.audio.voiceReverb.enabled = reverb;
                        }

                        Props.audio.voice.clip = Resources.Load<AudioClip>(voice);

                        SoftAssert.Condition(Props.audio.voice != null, "The voice audio clip '" + voice + "' is missing");

                        Props.audio.voice.loop = loopVoice;
                    }

                    // Dialog canvas is active; previous action was a Dialog action
                    if (Props.dialog.canvas.enabled)
                    {
                        if(type == Type.ADDITIVE)
                        {
                            content = Props.dialog.content.text + Tale.config.DIALOG_ADDITIVE_SEPARATOR + content;
                            index += Props.dialog.content.text.Length + Tale.config.DIALOG_ADDITIVE_SEPARATOR.Length;
                        }

                        ChangeState(State.BEGIN_WRITE);
                    }
                    else
                    {
                        // TODO: this may break things such as Clone(), so watch out if that happens (because type is modified in Run())
                        if (!SoftAssert.Condition(type == Type.OVERRIDE, "Additive dialog must be preceded by a dialog action; setting type to Override"))
                        {
                            type = Type.OVERRIDE;
                        }

                        if(actor != null)
                        {
                            Props.dialog.actor.text = "";
                        }

                        Props.dialog.content.text = "";
                        Props.dialog.canvas.enabled = true;

                        // Activate the animations, in the order specified in the Tale.config.
                        switch(Tale.config.DIALOG_ANIMATION_IN_MODE)
                        {
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                                if(ActivateCanvasAnimationIn())
                                {
                                    ChangeState(State.TRANSITION_IN);
                                }
                                else
                                {
                                    if(ActivateAvatarAnimationIn())
                                    {
                                        ChangeState(State.AVATAR_TRANSITION_IN);
                                    }
                                    else
                                    {
                                        ChangeState(State.BEGIN_WRITE);
                                    }
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                                if (ActivateCanvasAnimationIn())
                                {
                                    ChangeState(State.TRANSITION_IN);
                                }
                                else
                                {
                                    ActivateAvatarAnimationIn();
                                    ChangeState(State.BEGIN_WRITE);
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                                if (ActivateAvatarAnimationIn())
                                {
                                    ChangeState(State.AVATAR_TRANSITION_IN);
                                }
                                else
                                {
                                    if (ActivateCanvasAnimationIn())
                                    {
                                        ChangeState(State.TRANSITION_IN);
                                    }
                                    else
                                    {
                                        ChangeState(State.BEGIN_WRITE);
                                    }
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                                if (ActivateAvatarAnimationIn())
                                {
                                    ChangeState(State.AVATAR_TRANSITION_IN);
                                }
                                else
                                {
                                    ActivateCanvasAnimationIn();
                                    ChangeState(State.BEGIN_WRITE);
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT:
                            {
                                if(hasAnimation)
                                {
                                    // The canvas is animated;
                                    // The avatar may or may not be animated, so try to activate its animation too
                                    ActivateCanvasAnimationIn();
                                    ActivateAvatarAnimationIn();
                                    ChangeState(State.TRANSITION_IN);
                                }
                                else if (hasAvatarAnimation)
                                {
                                    ActivateAvatarAnimationIn();
                                    ChangeState(State.AVATAR_TRANSITION_IN);
                                }
                                else
                                {
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

                    screenToWorldUnitX = ((float) Screen.width) / TaleUtil.Config.Setup.REFERENCE_WIDTH;
                    screenToWorldUnitY = ((float) Screen.height) / TaleUtil.Config.Setup.REFERENCE_HEIGHT;

                    if (avatar != null)
                    {
                        SoftAssert.Condition(Props.dialog.avatar != null,
                            "An avatar was passed to the dialog action, but no avatar prop is available?");

                        Props.dialog.avatar.sprite = (Sprite) Resources.Load<Sprite>(avatar);

                        SoftAssert.Condition(Props.dialog.avatar.sprite != null, "The avatar '" + avatar + "' is missing");
                    }

                    if (Tale.config.DIALOG_FADE_FACTOR > 0)
                    {
                        Props.dialog.content.OnPreRenderText += OnPreRenderContentAlpha;
                    }

                    break;
                }
                case State.TRANSITION_IN:
                {
                    if (Input.Advance())
                    {
                        Props.dialog.animator.speed = Tale.config.TRANSITION_SKIP_SPEED;
                    }

                    if(!Props.dialog.animator.StateFinished(TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_STATE_IN))
                    {
                        break;
                    }

                    Props.dialog.animator.speed = 1f;
                    Props.dialog.animator.SetTrigger(TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);

                    if (Props.dialog.avatarAnimator == null)
                    {
                        ChangeState(State.BEGIN_WRITE);
                        break;
                    }

                    switch (Tale.config.DIALOG_ANIMATION_IN_MODE)
                    {
                        case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                            ChangeState(State.BEGIN_WRITE);
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                            if (ActivateAvatarAnimationIn())
                            {
                                ChangeState(State.AVATAR_TRANSITION_IN);
                            }
                            else
                            {
                                ChangeState(State.BEGIN_WRITE);
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                            ActivateAvatarAnimationIn();
                            ChangeState(State.BEGIN_WRITE);
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT:
                            // Canvas animation done, now wait for the avatar animation
                            if (hasAvatarAnimation)
                            {
                                ChangeState(State.AVATAR_TRANSITION_IN);
                            }
                            else
                            {
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
                case State.AVATAR_TRANSITION_IN:
                {
                    if (Input.Advance())
                    {
                        Props.dialog.avatarAnimator.speed = Tale.config.TRANSITION_SKIP_SPEED;
                    }

                    if (!Props.dialog.avatarAnimator.StateFinished(TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_STATE_IN))
                        break;

                    Props.dialog.avatarAnimator.speed = 1f;
                    Props.dialog.avatarAnimator.SetTrigger(TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);

                    if(Props.dialog.animator == null)
                    {
                        ChangeState(State.BEGIN_WRITE);
                        break;
                    }

                    switch(Tale.config.DIALOG_ANIMATION_IN_MODE)
                    {
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                            ChangeState(State.BEGIN_WRITE);
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                            if (ActivateCanvasAnimationIn())
                            {
                                ChangeState(State.TRANSITION_IN);
                            }
                            else
                            {
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
                case State.BEGIN_WRITE:
                {
                    if (actor != null)
                    {
                        Props.dialog.actor.text = actor;
                    }

                    Props.dialog.content.text = content;

                    contentInfo = Props.dialog.content.textInfo;

                    Props.dialog.content.maxVisibleCharacters = index;
                    fadeStartIndex = index;
                    fadeEndIndex = index;

                    if (voice != null)
                    {
                        Props.audio.voice.Play();
                    }

                    ChangeState(State.WRITE);

                    break;
                }
                case State.WRITE:
                {
                    if(fadeEndIndex < contentInfo.characterCount + Tale.config.DIALOG_FADE_FACTOR)
                    {
                        clock += delta();

                        int numChars;

                        if (Input.Advance())
                        {
                            numChars = content.Length - index + (int) Tale.config.DIALOG_FADE_FACTOR;
                        }
                        else
                        {
                            numChars = (int)Mathf.Floor(clock / timePerChar);
                        }

                        if(numChars > 0)
                        {
                            clock = clock % timePerChar;
                            index = Mathf.Min(index + numChars, contentInfo.characterCount);

                            fadeEndIndex = System.Math.Min(fadeEndIndex + numChars, contentInfo.characterCount + (int) Tale.config.DIALOG_FADE_FACTOR);

                            if (Props.dialog.content.maxVisibleCharacters == contentInfo.characterCount)
                            {
                                // Done writing; wait for the fade effect to finish
                                Props.dialog.content.ForceMeshUpdate();
                            }
                            else
                            {
                                Props.dialog.content.maxVisibleCharacters = index;
                            }
                        }
                    }
                    else
                    {
                        if(loopVoice && Props.audio.voice != null)
                        {
                            Props.audio.voice.loop = false;
                        }

                        // If an additive dialog action follows this one,
                        // use the additive CTC
                        var nextDialog = GetNextDialogAction(Queue.FetchNext());

                        if(nextDialog != null && nextDialog.type == Type.ADDITIVE)
                        {
                            if (Props.dialog.actc != null)
                            {
                                RepositionCTC(
                                    contentInfo,
                                    Props.dialog.actcTransform,
                                    Tale.config.DIALOG_CTC_ADDITIVE_OFFSET.x,
                                    Tale.config.DIALOG_CTC_ADDITIVE_OFFSET.y,
                                    Tale.config.DIALOG_CTC_ADDITIVE_ALIGNMENT);

                                Props.dialog.actc.SetActive(true);
                            }

                            ChangeState(State.WAIT_FOR_INPUT_ADDITIVE);
                        }
                        else
                        {
                            if(Props.dialog.ctc != null)
                            {
                                RepositionCTC(
                                    contentInfo,
                                    Props.dialog.ctcTransform,
                                    Tale.config.DIALOG_CTC_OVERRIDE_OFFSET.x,
                                    Tale.config.DIALOG_CTC_OVERRIDE_OFFSET.y,
                                    Tale.config.DIALOG_CTC_OVERRIDE_ALIGNMENT);
                                
                                Props.dialog.ctc.SetActive(true);
                            }

                            ChangeState(State.WAIT_FOR_INPUT_OVERRIDE);
                        }

                        clock = 0f;
                    }

                    break;
                }
                case State.WAIT_FOR_INPUT_OVERRIDE:
                {
                    if (autoMode)
                    {
                        clock += delta();
                    }

                    if(Input.Advance() || autoMode && clock >= Tale.config.DIALOG_AUTO_DELAY)
                    {
                        if (Props.dialog.ctc != null)
                        {
                            Props.dialog.ctc.SetActive(false);
                        }
                        ChangeState(State.END_WRITE);

                        // If the animations were playing while the text was written,
                        // set the neutral trigger so that the animators return to the idle state.
                        ActivateCanvasAnimationNeutral();
                        ActivateAvatarAnimationNeutral();
                    }

                    break;
                }
                case State.WAIT_FOR_INPUT_ADDITIVE:
                {
                    if (autoMode)
                    {
                        clock += delta();
                    }

                    if (Input.Advance() || autoMode && clock >= Tale.config.DIALOG_AUTO_DELAY)
                    {
                        if (Props.dialog.actc != null)
                        {
                            Props.dialog.actc.SetActive(false);
                        }
                        ChangeState(State.END_WRITE);

                        // If the animations were playing while the text was written,
                        // set the neutral trigger so that the animators return to the idle state.
                        ActivateCanvasAnimationNeutral();
                        ActivateAvatarAnimationNeutral();
                    }

                    break;
                }
                case State.END_WRITE:
                {
                    // Disable the voice object and audio group
                    if (voice != null && Props.audio.voice != null && Props.audio.group != null)
                    {
                        Props.audio.voice.Stop();
                        Props.audio.voice.gameObject.SetActive(false);

                        bool areSoundChannelsInactive = true;

                        if (Props.audio.sound != null)
                        {
                            for (int i = 0; i < Props.audio.sound.Length; ++i)
                            {
                                if (Props.audio.sound[i] != null && Props.audio.sound[i].gameObject.activeSelf)
                                {
                                    areSoundChannelsInactive = false;
                                    break;
                                }
                            }
                        }

                        if (areSoundChannelsInactive)
                        {
                            if (Props.audio.soundGroup != null)
                            {
                                // Deactivate the sound group.
                                Props.audio.soundGroup.SetActive(false);
                            }

                            // Deactivate the audio group.
                            if (Props.audio.music == null || !Props.audio.music.gameObject.activeSelf)
                            {
                                Props.audio.group.SetActive(false);
                            }
                        }
                    }

                    // If the next action is a dialog, don't play Transition Out.
                    var nextDialog = GetNextDialogAction(Queue.FetchNext());

                    if (nextDialog != null)
                    {
                        if(nextDialog.type == Type.OVERRIDE)
                        {
                            if (actor != null)
                            {
                                Props.dialog.actor.text = "";
                            }
                            Props.dialog.content.text = "";
                        }

                        ChangeState(State.END);
                        return true;
                    }

                    if (actor != null)
                    {
                        Props.dialog.actor.text = "";
                    }
                    Props.dialog.content.text = "";

                    // Activate the OUT animations in the order specified in the Tale.config.
                    switch(Tale.config.DIALOG_ANIMATION_OUT_MODE)
                    {
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                            if(ActivateCanvasAnimationOut())
                            {
                                ChangeState(State.TRANSITION_OUT);
                            }
                            else if(ActivateAvatarAnimationOut())
                            {
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            }
                            else
                            {
                                goto default; // No animations
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            if (ActivateAvatarAnimationOut())
                            {
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            }
                            else if (ActivateCanvasAnimationOut())
                            {
                                ChangeState(State.TRANSITION_OUT);
                            }
                            else
                            {
                                goto default; // No animations
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_AVATAR:
                        {
                            //bool hasAnimation = ActivateCanvasAnimationOut();
                            //hasAnimation = ActivateAvatarAnimationOut() || hasAnimation;

                            if (hasAnimation)
                            {
                                // The canvas is animated;
                                // The avatar may or may not be animated, so try to activate its animation too
                                ActivateCanvasAnimationOut();
                                ActivateAvatarAnimationOut();
                                ChangeState(State.TRANSITION_OUT);
                            }
                            else if(hasAvatarAnimation)
                            {
                                ActivateAvatarAnimationOut();
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            }
                            else
                            {
                                ChangeState(State.BEGIN_WRITE);
                            }
                            break;
                        }
                        default:
                            Props.dialog.canvas.enabled = false;
                            ChangeState(State.END);
                            return true;
                    }

                    break;
                }
                case State.TRANSITION_OUT:
                {
                    if (Input.Advance())
                    {
                        Props.dialog.animator.speed = Tale.config.TRANSITION_SKIP_SPEED;
                    }

                    if(!Props.dialog.animator.StateFinished(TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_STATE_OUT))
                    {
                        break;
                    }

                    Props.dialog.animator.speed = 1f;
                    Props.dialog.animator.SetTrigger(TaleUtil.Config.Setup.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);

                    if(Props.dialog.avatarAnimator == null)
                    {
                        Props.dialog.canvas.enabled = false;
                        ChangeState(State.END);
                        return true;
                    }

                    switch (Tale.config.DIALOG_ANIMATION_OUT_MODE)
                    {
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                            if (ActivateAvatarAnimationOut())
                            {
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            }
                            else
                            {
                                goto default;
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_AVATAR:
                            // Canvas animation finished, now wait for the avatar animation
                            if(hasAvatarAnimation)
                            {
                                ChangeState(State.AVATAR_TRANSITION_OUT);
                            }
                            else
                            {
                                goto default; // There's no avatar, done
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            // Fallthrough
                        default:
                            Props.dialog.canvas.enabled = false;
                            ChangeState(State.END);
                            return true;
                    }

                    break;
                }
                case State.AVATAR_TRANSITION_OUT:
                {
                    if (Input.Advance())
                    {
                        Props.dialog.avatarAnimator.speed = Tale.config.TRANSITION_SKIP_SPEED;
                    }

                    if (!Props.dialog.avatarAnimator.StateFinished(TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_STATE_OUT))
                    {
                        break;
                    }

                    Props.dialog.avatarAnimator.speed = 1f;
                    Props.dialog.avatarAnimator.SetTrigger(TaleUtil.Config.Setup.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);

                    switch (Tale.config.DIALOG_ANIMATION_OUT_MODE)
                    {
                        case TaleUtil.Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            if (ActivateCanvasAnimationOut())
                            {
                                ChangeState(State.TRANSITION_OUT);
                            }
                            else
                            {
                                goto default;
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_AVATAR:
                            // Canvas and avatar animations finished.
                            // Fallthrough
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                            // Fallthrough
                        default:
                            Props.dialog.canvas.enabled = false;
                            ChangeState(State.END);
                            return true;
                    }

                    break;
                }
                case State.END:
                {
                    TaleUtil.Log.Warning("DialogAction.Run() called when action was already done");
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("DialogAction ({0})", state.ToString());
        }

        void ChangeState(State state)
        {
            this.state = state;

            if (state == State.END)
            {
                if (Tale.config.DIALOG_FADE_FACTOR > 0)
                {
                    Props.dialog.content.OnPreRenderText -= OnPreRenderContentAlpha;
                }
            }

            if (Hooks.OnDialogUpdate != null)
            {
                Hooks.OnDialogUpdate(this);
            }
        }
    }
}