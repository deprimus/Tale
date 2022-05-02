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

        public Type type;
        State state;
        int index;

        TMP_TextInfo contentInfo;

        float timePerChar;
        float clock;
        float screenToWorldUnit;

        DialogAction() { }

        public DialogAction(string actor, string content, string avatar, string voice, bool loopVoice, bool additive)
        {
            Assert.Condition(Props.dialog.content != null,
                "DialogAction requires a content object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");

            if (actor != null)
            {
                Assert.Condition(Props.dialog.actor != null,
                    "DialogAction requires an actor object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");
            }

            this.actor = actor;
            this.content = content;
            this.avatar = avatar;
            this.voice = voice;
            this.loopVoice = loopVoice;

            type = additive ? Type.ADDITIVE : Type.OVERRIDE;
            state = State.SETUP;

            if(Config.DIALOG_CPS > 0)
                timePerChar = 1f / Config.DIALOG_CPS;
            else timePerChar = 0f;

            clock = 0f;

            index = (timePerChar == 0) ? content.Length : 0;
        }

        bool Advance()
        {
            if(Input.GetMouseButtonUp(0) || Input.GetKey(Config.DIALOG_KEY_SKIP))
                return true;

            for(int i = 0; i < Config.DIALOG_KEY_NEXT.Length; ++i)
                if(Input.GetKeyDown(Config.DIALOG_KEY_NEXT[i]))
                    return true;

            return false;
        }

        // Moves the CTC object to the end of the text.
        void RepositionCTC(TMP_TextInfo textInfo, RectTransform ctcTransform, float xOffset, float yOffset, Config.CTCAlignment alignment)
        {
            // Note: This way of repositioning the CTC only works when the TextMeshProUGUI object has the horizontal alignment set to "Left" and the vertical alignment set to "Top".
            TMP_CharacterInfo lastCharInfo = textInfo.characterInfo[contentInfo.characterCount - 1];
            RectTransform contentTransform = Props.dialog.content.rectTransform;

            Vector3 bottomRight = contentTransform.TransformPoint(lastCharInfo.bottomRight);

            float x = bottomRight.x + xOffset * screenToWorldUnit;
            float y = yOffset;

            if (alignment == Config.CTCAlignment.MIDDLE)
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

        bool ActivateCanvasAnimation(string state)
        {
            if (Props.dialog.animator != null)
            {
                Props.dialog.animator.SetTrigger(state);
                return true;
            }

            return false;
        }

        bool ActivateAvatarAnimation(string state)
        {
            if (avatar != null && Props.dialog.avatar != null && Props.dialog.avatarAnimator != null)
            {
                Props.dialog.avatarAnimator.SetTrigger(state);
                return true;
            }

            return false;
        }

        bool ActivateCanvasAnimationIn() =>
            ActivateCanvasAnimation(Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN);
        bool ActivateCanvasAnimationOut() =>
            ActivateCanvasAnimation(Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT);

        bool ActivateAvatarAnimationIn() =>
            ActivateAvatarAnimation(Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN);
        bool ActivateAvatarAnimationOut() =>
            ActivateAvatarAnimation(Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_OUT);

        void ActivateCanvasAnimationNeutral()
        {
            if (Props.dialog.animator != null)
            {
                switch (Config.DIALOG_ANIMATION_IN_MODE)
                {
                    case Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                    case Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
                        Props.dialog.animator.SetTrigger(Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);
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
                switch (Config.DIALOG_ANIMATION_IN_MODE)
                {
                    case Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                    case Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
                        Props.dialog.avatarAnimator.SetTrigger(Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);
                        break;
                    default:
                        break;
                }
            }
        }

        public override Action Clone()
        {
            DialogAction clone = new DialogAction();
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
                        Assert.Condition(Props.audio.group != null,
                            "A voice clip was passed to the dialog action, but no audio group prop is available; did you forget to register it in TaleMaster?");
                        Assert.Condition(Props.audio.voice != null,
                            "A voice clip was passed to the dialog action, but no audio voice prop is available; did you forget to register it in TaleMaster?");

                        if (!Props.audio.group.activeSelf)
                        {
                            Props.audio.group.SetActive(true);
                        }

                        if (!Props.audio.voice.gameObject.activeSelf)
                        {
                            Props.audio.voice.gameObject.SetActive(true);
                        }

                        Props.audio.voice.clip = Resources.Load<AudioClip>(voice);

                        Assert.Condition(Props.audio.voice != null, "The voice audio clip '" + voice + "' is missing");

                        Props.audio.voice.loop = loopVoice;
                    }

                    // Dialog canvas is active; previous action was a Dialog action
                    if (Props.dialog.canvas.activeSelf)
                    {
                        if(type == Type.ADDITIVE)
                        {
                            content = Props.dialog.content.text + Config.DIALOG_ADDITIVE_SEPARATOR + content;
                            index += Props.dialog.content.text.Length + Config.DIALOG_ADDITIVE_SEPARATOR.Length;
                        }

                        state = State.BEGIN_WRITE;
                    }
                    else
                    {
                        Assert.Condition(type == Type.OVERRIDE, "Additive dialog must be preceded by a dialog action");

                        if(actor != null)
                        {
                            Props.dialog.actor.text = "";
                        }

                        Props.dialog.content.text = "";
                        Props.dialog.canvas.SetActive(true);

                        // Activate the animations, in the order specified in the config.
                        switch(Config.DIALOG_ANIMATION_IN_MODE)
                        {
                            case Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
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
                            case Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
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
                            case Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
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
                            case Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
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
                            case Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT:
                                {
                                    bool hasAnimation = ActivateCanvasAnimationIn();
                                    hasAnimation = ActivateAvatarAnimationIn() || hasAnimation;

                                    if(hasAnimation)
                                    {
                                        state = State.TRANSITION_IN;
                                    }
                                    else
                                    {
                                        state = State.BEGIN_WRITE;
                                    }
                                }
                                break;
                            case Config.DialogAnimationInMode.CANVAS_AVATAR_TEXT:
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

                    screenToWorldUnit = Screen.width / 1920f;

                    if (avatar != null)
                    {
                        Assert.Condition(Props.dialog.avatar != null,
                            "An avatar was passed to the dialog action, but no avatar prop is available; did you forget to register it in TaleMaster?");

                        Props.dialog.avatar.sprite = (Sprite) Resources.Load<Sprite>(avatar);

                        Assert.Condition(Props.dialog.avatar.sprite != null, "The avatar '" + avatar + "' is missing");
                    }

                    break;
                }
                case State.TRANSITION_IN:
                {
                    if (Advance())
                    {
                        Props.dialog.animator.speed = Config.TRANSITION_SKIP_SPEED;
                    }

                    if(!Props.dialog.animator.StateFinished(Config.DIALOG_CANVAS_ANIMATOR_STATE_IN))
                    {
                        break;
                    }

                    Props.dialog.animator.speed = 1f;
                    Props.dialog.animator.SetTrigger(Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);

                    if (Props.dialog.avatarAnimator == null)
                    {
                        state = State.BEGIN_WRITE;
                        break;
                    }

                    switch (Config.DIALOG_ANIMATION_IN_MODE)
                    {
                        case Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                            state = State.BEGIN_WRITE;
                            break;
                        case Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                            Props.dialog.avatarAnimator.SetTrigger(Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN);
                            state = State.AVATAR_TRANSITION_IN;
                            break;
                        case Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_TEXT:
                            Props.dialog.avatarAnimator.SetTrigger(Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_IN);
                            state = State.BEGIN_WRITE;
                            break;
                        case Config.DialogAnimationInMode.CANVAS_AVATAR_THEN_TEXT:
                            // Canvas animation done, now wait for the avatar animation
                            state = State.AVATAR_TRANSITION_IN;
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
                    if (Advance())
                    {
                        Props.dialog.avatarAnimator.speed = Config.TRANSITION_SKIP_SPEED;
                    }

                    if (!Props.dialog.avatarAnimator.StateFinished(Config.DIALOG_AVATAR_ANIMATOR_STATE_IN))
                        break;

                    Props.dialog.avatarAnimator.speed = 1f;
                    Props.dialog.avatarAnimator.SetTrigger(Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);

                    if(Props.dialog.animator == null)
                    {
                        state = State.BEGIN_WRITE;
                        break;
                    }

                    switch(Config.DIALOG_ANIMATION_IN_MODE)
                    {
                        case Config.DialogAnimationInMode.CANVAS_THEN_AVATAR_THEN_TEXT:
                            state = State.BEGIN_WRITE;
                            break;
                        case Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_THEN_TEXT:
                            Props.dialog.animator.SetTrigger(Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN);
                            state = State.TRANSITION_IN;
                            break;
                        case Config.DialogAnimationInMode.AVATAR_THEN_CANVAS_TEXT:
                            Props.dialog.animator.SetTrigger(Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN);
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
                        clock += Time.deltaTime;

                        int numChars;

                        if (Advance())
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
                                    Config.DIALOG_CTC_ADDITIVE_OFFSET_X,
                                    Config.DIALOG_CTC_ADDITIVE_OFFSET_Y,
                                    Config.DIALOG_CTC_ADDITIVE_ALIGNMENT);

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
                                    Config.DIALOG_CTC_OVERRIDE_OFFSET_X,
                                    Config.DIALOG_CTC_OVERRIDE_OFFSET_Y,
                                    Config.DIALOG_CTC_OVERRIDE_ALIGNMENT);
                                
                                Props.dialog.ctc.SetActive(true);
                            }

                            state = State.WAIT_FOR_INPUT_OVERRIDE;
                        }
                    }

                    break;
                }
                case State.WAIT_FOR_INPUT_OVERRIDE:
                {
                    if(Advance())
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
                    if(Advance())
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

                        return true;
                    }

                    if (actor != null)
                    {
                        Props.dialog.actor.text = "";
                    }
                    Props.dialog.content.text = "";

                    // Activate the OUT animations in the order specified in the config.
                    switch(Config.DIALOG_ANIMATION_OUT_MODE)
                    {
                        case Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
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
                        case Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
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
                        case Config.DialogAnimationOutMode.CANVAS_AVATAR:
                        {
                            bool hasAnimation = ActivateCanvasAnimationOut();
                            hasAnimation = ActivateAvatarAnimationOut() || hasAnimation;

                            if (hasAnimation)
                            {
                                state = State.TRANSITION_OUT;
                            }
                            else
                            {
                                state = State.BEGIN_WRITE;
                            }
                            break;
                        }
                        default:
                            Props.dialog.canvas.SetActive(false);
                            return true;
                    }

                    break;
                }
                case State.TRANSITION_OUT:
                {
                    if (Advance())
                    {
                        Props.dialog.animator.speed = Config.TRANSITION_SKIP_SPEED;
                    }

                    if(!Props.dialog.animator.StateFinished(Config.DIALOG_CANVAS_ANIMATOR_STATE_OUT))
                    {
                        break;
                    }

                    Props.dialog.animator.speed = 1f;
                    Props.dialog.animator.SetTrigger(Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);

                    if(Props.dialog.avatarAnimator == null)
                    {
                        Props.dialog.canvas.SetActive(false);
                        return true;
                    }

                    switch (Config.DIALOG_ANIMATION_OUT_MODE)
                    {
                        case Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                            if (ActivateAvatarAnimationOut())
                            {
                                state = State.AVATAR_TRANSITION_OUT;
                            }
                            else
                            {
                                goto default;
                            }
                            break;
                        case Config.DialogAnimationOutMode.CANVAS_AVATAR:
                            // Canvas animation finished, now wait for the avatar animation
                            state = State.AVATAR_TRANSITION_OUT;
                            break;
                        case Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            // Fallthrough
                        default:
                            Props.dialog.canvas.SetActive(false);
                            return true;
                    }

                    break;
                }
                case State.AVATAR_TRANSITION_OUT:
                {
                    if (Advance())
                    {
                        Props.dialog.avatarAnimator.speed = Config.TRANSITION_SKIP_SPEED;
                    }

                    if (!Props.dialog.avatarAnimator.StateFinished(Config.DIALOG_AVATAR_ANIMATOR_STATE_OUT))
                    {
                        break;
                    }

                    Props.dialog.avatarAnimator.speed = 1f;
                    Props.dialog.avatarAnimator.SetTrigger(Config.DIALOG_AVATAR_ANIMATOR_TRIGGER_NEUTRAL);

                    switch (Config.DIALOG_ANIMATION_OUT_MODE)
                    {
                        case Config.DialogAnimationOutMode.AVATAR_THEN_CANVAS:
                            if (ActivateCanvasAnimationOut())
                            {
                                state = State.TRANSITION_OUT;
                            }
                            else
                            {
                                goto default;
                            }
                            break;
                        case Config.DialogAnimationOutMode.CANVAS_AVATAR:
                            // Canvas and avatar animations finished.
                            // Fallthrough
                        case Config.DialogAnimationOutMode.CANVAS_THEN_AVATAR:
                            // Fallthrough
                        default:
                            Props.dialog.canvas.SetActive(false);
                            return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}