#pragma warning disable 0162 // Disable the 'unreachable code' warning caused by config constants.

using UnityEngine;
using TMPro;

namespace TaleUtil
{
    public class DialogAction : Action
    {
        public enum Type
        {
            OVERRIDE,
            ADDITIVE
        }

        enum State
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
            TRANSITION_OUT
        }

        string actor;
        string content;
        string avatar;
        string voice;

        bool loopVoice;
        bool reverb;

        public Type type;
        State state;
        int index;

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
            state = State.SETUP;

            if(Tale.config.DIALOG_CPS > 0)
                timePerChar = 1f / Tale.config.DIALOG_CPS;
            else timePerChar = 0f;

            clock = 0f;

            index = (timePerChar == 0) ? content.Length : 0;

            hasAnimation = HasAnimation();
            hasAvatarAnimation = HasAnimateableAvatar();
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
            // TODO: When the text is done writing, continue with the fade effect.
            // Currently, it cuts off because of this check.
            if (Props.dialog.content.maxVisibleCharacters == textInfo.characterCount)
            {
                return;
            }

            // For some reason, changing ANY character also changes the first rendered character.
            // I guess this is because they have the same material index? No clue.
            // This is a hack that works.

            int m = textInfo.characterInfo[0].materialReferenceIndex;
            int v = textInfo.characterInfo[0].vertexIndex;

            Color originalFirst = textInfo.meshInfo[m].colors32[v];

            for (int i = System.Math.Max(Props.dialog.content.maxVisibleCharacters - (int) Tale.config.DIALOG_FADE_FACTOR, 0); i < Props.dialog.content.maxVisibleCharacters; ++i)
            {
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Color color = textInfo.meshInfo[materialIndex].colors32[vertexIndex];
                color.a = Mathf.Clamp01((1f / Tale.config.DIALOG_FADE_FACTOR) * (Props.dialog.content.maxVisibleCharacters - i));

                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 0] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 1] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 2] = color;
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + 3] = color;
            }

            for (int i = 0; i < 4; ++i)
            {
                textInfo.meshInfo[m].colors32[v + i] = originalFirst;
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
                    if (Props.dialog.canvas.activeSelf)
                    {
                        if(type == Type.ADDITIVE)
                        {
                            content = Props.dialog.content.text + Tale.config.DIALOG_ADDITIVE_SEPARATOR + content;
                            index += Props.dialog.content.text.Length + Tale.config.DIALOG_ADDITIVE_SEPARATOR.Length;
                        }

                        state = State.BEGIN_WRITE;
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
                        Props.dialog.canvas.SetActive(true);

                        // Activate the animations, in the order specified in the Tale.config.
                        switch(Tale.config.DIALOG_ANIMATION_IN_MODE)
                        {
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                                if(ActivateCanvasAnimationIn())
                                {
                                    state = State.TRANSITION_IN;
                                }
                                else
                                {
                                    if(ActivateAvatarAnimationIn())
                                    {
                                        state = State.AVATAR_TRANSITION_IN;
                                    }
                                    else
                                    {
                                        state = State.BEGIN_WRITE;
                                    }
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                                if (ActivateCanvasAnimationIn())
                                {
                                    state = State.TRANSITION_IN;
                                }
                                else
                                {
                                    ActivateAvatarAnimationIn();
                                    state = State.BEGIN_WRITE;
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                                if (ActivateAvatarAnimationIn())
                                {
                                    state = State.AVATAR_TRANSITION_IN;
                                }
                                else
                                {
                                    if (ActivateCanvasAnimationIn())
                                    {
                                        state = State.TRANSITION_IN;
                                    }
                                    else
                                    {
                                        state = State.BEGIN_WRITE;
                                    }
                                }
                                break;
                            case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                                if (ActivateAvatarAnimationIn())
                                {
                                    state = State.AVATAR_TRANSITION_IN;
                                }
                                else
                                {
                                    ActivateCanvasAnimationIn();
                                    state = State.BEGIN_WRITE;
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
                                    state = State.TRANSITION_IN;
                                }
                                else if (hasAvatarAnimation)
                                {
                                    ActivateAvatarAnimationIn();
                                    state = State.AVATAR_TRANSITION_IN;
                                }
                                else
                                {
                                    state = State.BEGIN_WRITE;
                                }
                            }
                            break;
                            case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
                                ActivateCanvasAnimationIn();
                                ActivateAvatarAnimationIn();
                                state = State.BEGIN_WRITE;
                                break;
                            default:
                                Log.Warning("Dialog", "Unreachable block reached: default case in SETUP (report this to the devs)");
                                state = State.BEGIN_WRITE;
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

                    Props.dialog.content.OnPreRenderText += OnPreRenderContentAlpha;

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
                        state = State.BEGIN_WRITE;
                        break;
                    }

                    switch (Tale.config.DIALOG_ANIMATION_IN_MODE)
                    {
                        case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                            state = State.BEGIN_WRITE;
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                            if (ActivateAvatarAnimationIn())
                            {
                                state = State.AVATAR_TRANSITION_IN;
                            }
                            else
                            {
                                state = State.BEGIN_WRITE;
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                            ActivateAvatarAnimationIn();
                            state = State.BEGIN_WRITE;
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT:
                            // Canvas animation done, now wait for the avatar animation
                            if (hasAvatarAnimation)
                            {
                                state = State.AVATAR_TRANSITION_IN;
                            }
                            else
                            {
                                state = State.BEGIN_WRITE;
                            }
                            break;
                        default:
                            Log.Warning("Dialog", "Unreachable block reached: default case in TRANSITION_IN (report this to the devs)");
                            state = State.BEGIN_WRITE;
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
                        state = State.BEGIN_WRITE;
                        break;
                    }

                    switch(Tale.config.DIALOG_ANIMATION_IN_MODE)
                    {
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                            state = State.BEGIN_WRITE;
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                            if (ActivateCanvasAnimationIn())
                            {
                                state = State.TRANSITION_IN;
                            }
                            else
                            {
                                state = State.BEGIN_WRITE;
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                            ActivateCanvasAnimationIn();
                            state = State.BEGIN_WRITE;
                            break;
                        case TaleUtil.Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT:
                            state = State.BEGIN_WRITE;
                            break;
                        default:
                            Log.Warning("Dialog", "Unreachable block reached: default case in AVATAR_TRANSITION_IN (report this to the devs)");
                            state = State.BEGIN_WRITE;
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

                    if (voice != null)
                    {
                        Props.audio.voice.Play();
                    }

                    state = State.WRITE;

                    break;
                }
                case State.WRITE:
                {
                    if(index < contentInfo.characterCount)
                    {
                        clock += delta();

                        int numChars;

                        if (Input.Advance())
                        {
                            numChars = content.Length - index;
                        }
                        else
                        {
                            numChars = (int)Mathf.Floor(clock / timePerChar);
                        }

                        if(numChars > 0)
                        {
                            clock = clock % timePerChar;
                            index += numChars;

                            Props.dialog.content.maxVisibleCharacters = index;
                        }
                    }
                    else
                    {
                        if(loopVoice && Props.audio.voice != null)
                        {
                            Props.audio.voice.loop = false;
                        }

                        if((Queue.FetchNext() is DialogAction) && ((DialogAction) Queue.FetchNext()).type == Type.ADDITIVE)
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

                            state = State.WAIT_FOR_INPUT_ADDITIVE;
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

                            state = State.WAIT_FOR_INPUT_OVERRIDE;
                        }
                    }

                    break;
                }
                case State.WAIT_FOR_INPUT_OVERRIDE:
                {
                    if(Input.Advance())
                    {
                        if (Props.dialog.ctc != null)
                        {
                            Props.dialog.ctc.SetActive(false);
                        }
                        state = State.END_WRITE;

                        // If the animations were playing while the text was written,
                        // set the neutral trigger so that the animators return to the idle state.
                        ActivateCanvasAnimationNeutral();
                        ActivateAvatarAnimationNeutral();
                    }

                    break;
                }
                case State.WAIT_FOR_INPUT_ADDITIVE:
                {
                    if(Input.Advance())
                    {
                        if (Props.dialog.actc != null)
                        {
                            Props.dialog.actc.SetActive(false);
                        }
                        state = State.END_WRITE;

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

                    Action next = Queue.FetchNext();

                    if(next is DialogAction)
                    {
                        if(((DialogAction) next).type == Type.OVERRIDE)
                        {
                            if (actor != null)
                            {
                                Props.dialog.actor.text = "";
                            }
                            Props.dialog.content.text = "";
                        }

                        Props.dialog.content.OnPreRenderText -= OnPreRenderContentAlpha;
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
                                state = State.TRANSITION_OUT;
                            }
                            else if(ActivateAvatarAnimationOut())
                            {
                                state = State.AVATAR_TRANSITION_OUT;
                            }
                            else
                            {
                                goto default; // No animations
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            if (ActivateAvatarAnimationOut())
                            {
                                state = State.AVATAR_TRANSITION_OUT;
                            }
                            else if (ActivateCanvasAnimationOut())
                            {
                                state = State.TRANSITION_OUT;
                            }
                            else
                            {
                                goto default; // No animations
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_AVATAR:
                        {
                            bool hasAnimation = ActivateCanvasAnimationOut();
                            hasAnimation = ActivateAvatarAnimationOut() || hasAnimation;

                            if (hasAnimation)
                            {
                                ActivateCanvasAnimationOut();
                                ActivateAvatarAnimationOut();
                                state = State.TRANSITION_OUT;
                            }
                            else if(hasAvatarAnimation)
                            {
                                ActivateAvatarAnimationOut();
                                state = State.AVATAR_TRANSITION_OUT;
                            }
                            else
                            {
                                state = State.BEGIN_WRITE;
                            }
                            break;
                        }
                        default:
                            Props.dialog.canvas.SetActive(false);
                            Props.dialog.content.OnPreRenderText -= OnPreRenderContentAlpha;
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
                        Props.dialog.canvas.SetActive(false);
                        Props.dialog.content.OnPreRenderText -= OnPreRenderContentAlpha;
                        return true;
                    }

                    switch (Tale.config.DIALOG_ANIMATION_OUT_MODE)
                    {
                        case TaleUtil.Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                            if (ActivateAvatarAnimationOut())
                            {
                                state = State.AVATAR_TRANSITION_OUT;
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
                                state = State.AVATAR_TRANSITION_OUT;
                            }
                            else
                            {
                                goto default; // There's no avatar, done
                            }
                            break;
                        case TaleUtil.Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            // Fallthrough
                        default:
                            Props.dialog.canvas.SetActive(false);
                            Props.dialog.content.OnPreRenderText -= OnPreRenderContentAlpha;
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
                                state = State.TRANSITION_OUT;
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
                            Props.dialog.canvas.SetActive(false);
                            Props.dialog.content.OnPreRenderText -= OnPreRenderContentAlpha;
                            return true;
                    }

                    break;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("DialogAction ({0})", state.ToString());
        }
    }
}