#pragma warning disable 0162 // Disable the 'unreachable code' warning caused by config constants.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TaleUtil
{
    public class DialogAction : TaleUtil.Action
    {
        public enum Type
        {
            OVERRIDE,
            ADDITIVE
        }

        private enum State
        {
            SETUP,
            TRANSITION_IN,
            BEGIN_WRITE,
            WRITE,
            WAIT_FOR_INPUT_OVERRIDE,
            WAIT_FOR_INPUT_ADDITIVE,
            END_WRITE,
            TRANSITION_OUT
        }

        private string actor;
        private string content;

        public Type type;
        private State state;
        private int index;

        TMP_TextInfo contentInfo;

        private float timePerChar;
        private float clock;
        private float screenToWorldUnit;

        private DialogAction() { }

        public DialogAction(string actor, string content, bool additive)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.dialog.content, "DialogAction requires a content object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");

            if(actor != null)
                TaleUtil.Assert.NotNull(TaleUtil.Props.dialog.actor, "DialogAction requires an actor object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");

            this.actor = actor;
            this.content = content;

            type = additive ? Type.ADDITIVE : Type.OVERRIDE;
            state = State.SETUP;

            if(TaleUtil.Config.DIALOG_CPS > 0)
                timePerChar = 1f / TaleUtil.Config.DIALOG_CPS;
            else timePerChar = 0f;

            clock = 0f;

            index = (timePerChar == 0) ? content.Length : 0;
        }

        private bool Advance()
        {
            if(Input.GetMouseButtonUp(0) || Input.GetKey(TaleUtil.Config.DIALOG_KEY_SKIP))
                return true;

            for(int i = 0; i < TaleUtil.Config.DIALOG_KEY_NEXT.Length; ++i)
                if(Input.GetKeyDown(TaleUtil.Config.DIALOG_KEY_NEXT[i]))
                    return true;

            return false;
        }

        private void RepositionCTC(TMP_TextInfo textInfo, RectTransform ctcTransform, float xOffset, float yOffset, TaleUtil.Config.CTCAlignment alignment)
        {
            // Note: This way of repositioning the CTC only works when the TextMeshProUGUI object has the horizontal alignment set to "Left" and the vertical alignment set to "Top".
            TMP_CharacterInfo lastCharInfo = textInfo.characterInfo[contentInfo.characterCount - 1];
            RectTransform contentTransform = TaleUtil.Props.dialog.content.rectTransform;

            Vector3 bottomRight = contentTransform.TransformPoint(lastCharInfo.bottomRight);
            float baseline = contentTransform.TransformPoint(new Vector3(0, lastCharInfo.baseLine, 0)).y;

            float x = bottomRight.x + xOffset * screenToWorldUnit;
            float y = yOffset;

            if (alignment == TaleUtil.Config.CTCAlignment.MIDDLE)
            {
                y += contentTransform.TransformPoint(new Vector2(0, contentInfo.lineInfo[contentInfo.lineCount - 1].descender + contentInfo.lineInfo[contentInfo.lineCount - 1].lineHeight / 2)).y;
            }
            else
            {
                y += contentTransform.TransformPoint(new Vector2(0, lastCharInfo.baseLine)).y;
            }

            ctcTransform.position = new Vector3(x, y, bottomRight.z);
        }

        public override TaleUtil.Action Clone()
        {
            DialogAction clone = new DialogAction();
            clone.actor = actor;
            clone.content = content;
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
                    if(TaleUtil.Props.dialog.canvas.activeSelf)
                    {
                        if(type == Type.ADDITIVE)
                        {
                            content = TaleUtil.Props.dialog.content.text + TaleUtil.Config.DIALOG_ADDITIVE_SEPARATOR + content;
                            index += TaleUtil.Props.dialog.content.text.Length + TaleUtil.Config.DIALOG_ADDITIVE_SEPARATOR.Length;
                        }

                        state = State.BEGIN_WRITE;
                        break;
                    }

                    Debug.Assert(type == Type.OVERRIDE, "[TALE] Additive dialog must be preceded by a dialog action");

                    if(actor != null)
                        TaleUtil.Props.dialog.actor.text = "";

                    TaleUtil.Props.dialog.content.text = "";
                    TaleUtil.Props.dialog.canvas.SetActive(true);

                    if(TaleUtil.Props.dialog.animator != null)
                    {
                        TaleUtil.Props.dialog.animator.SetTrigger(TaleUtil.Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_IN);
                        state = State.TRANSITION_IN;
                    }
                    else
                    {
                        state = State.BEGIN_WRITE;
                    }

                    screenToWorldUnit = Screen.width / 1920f;

                    break;
                }
                case State.TRANSITION_IN:
                {
                    AnimatorStateInfo inInfo = TaleUtil.Props.dialog.animator.GetCurrentAnimatorStateInfo(0);

                    if(Advance())
                        TaleUtil.Props.dialog.animator.speed = TaleUtil.Config.TRANSITION_SKIP_SPEED;

                    if(!inInfo.IsName(TaleUtil.Config.DIALOG_CANVAS_ANIMATOR_STATE_IN) || inInfo.normalizedTime < 1f)
                        break;

                    TaleUtil.Props.dialog.animator.speed = 1f;
                    TaleUtil.Props.dialog.animator.SetTrigger(TaleUtil.Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);

                    state = State.BEGIN_WRITE;

                    break;
                }
                case State.BEGIN_WRITE:
                {
                    if(actor != null)
                        TaleUtil.Props.dialog.actor.text = actor;
                    TaleUtil.Props.dialog.content.text = content;

                    contentInfo = TaleUtil.Props.dialog.content.textInfo;

                    TaleUtil.Props.dialog.content.maxVisibleCharacters = index;

                    state = State.WRITE;

                    break;
                }
                case State.WRITE:
                {
                    if(index < contentInfo.characterCount)
                    {
                        clock += Time.deltaTime;

                        int numChars;

                        if(Advance())
                            numChars = content.Length - index;
                        else numChars = (int) Mathf.Floor(clock / timePerChar);

                        //clock += Time.deltaTime; // Clock here.

                        if(numChars > 0)
                        {
                            clock = clock % timePerChar;
                            index += numChars;

                            TaleUtil.Props.dialog.content.maxVisibleCharacters = index;
                        }
                    }
                    else
                    {
                        if((TaleUtil.Queue.FetchNext() is DialogAction) && ((DialogAction) TaleUtil.Queue.FetchNext()).type == Type.ADDITIVE)
                        {
                            if (TaleUtil.Props.dialog.actc != null)
                            {
                                RepositionCTC(
                                    contentInfo,
                                    TaleUtil.Props.dialog.actcTransform,
                                    TaleUtil.Config.DIALOG_CTC_ADDITIVE_OFFSET_X,
                                    TaleUtil.Config.DIALOG_CTC_ADDITIVE_OFFSET_Y,
                                    TaleUtil.Config.DIALOG_CTC_ADDITIVE_ALIGNMENT);

                                TaleUtil.Props.dialog.actc.SetActive(true);
                            }

                            state = State.WAIT_FOR_INPUT_ADDITIVE;
                        }
                        else
                        {
                            if(TaleUtil.Props.dialog.ctc != null)
                            {
                                RepositionCTC(
                                    contentInfo,
                                    TaleUtil.Props.dialog.ctcTransform,
                                    TaleUtil.Config.DIALOG_CTC_OVERRIDE_OFFSET_X,
                                    TaleUtil.Config.DIALOG_CTC_OVERRIDE_OFFSET_Y,
                                    TaleUtil.Config.DIALOG_CTC_OVERRIDE_ALIGNMENT);
                                
                                TaleUtil.Props.dialog.ctc.SetActive(true);
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
                        if(TaleUtil.Props.dialog.ctc != null)
                            TaleUtil.Props.dialog.ctc.SetActive(false);
                        state = State.END_WRITE;
                    }

                    break;
                }
                case State.WAIT_FOR_INPUT_ADDITIVE:
                {
                    if(Advance())
                    {
                        if(TaleUtil.Props.dialog.actc != null)
                            TaleUtil.Props.dialog.actc.SetActive(false);
                        state = State.END_WRITE;
                    }

                    break;
                }
                case State.END_WRITE:
                { 
                    TaleUtil.Action next = TaleUtil.Queue.FetchNext();

                    if(next is DialogAction)
                    {
                        if(((DialogAction) next).type == Type.OVERRIDE)
                        {
                            if(actor != null)
                                TaleUtil.Props.dialog.actor.text = "";
                            TaleUtil.Props.dialog.content.text = "";
                        }

                        return  true;
                    }

                    if(actor != null)
                        TaleUtil.Props.dialog.actor.text = "";
                    TaleUtil.Props.dialog.content.text = "";

                    if(TaleUtil.Props.dialog.animator != null)
                    {
                        TaleUtil.Props.dialog.animator.SetTrigger(TaleUtil.Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_OUT);
                        state = State.TRANSITION_OUT;
                    }
                    else
                    {
                        TaleUtil.Props.dialog.canvas.SetActive(false);
                        return true;
                    }

                    break;
                }
                case State.TRANSITION_OUT:
                {
                    AnimatorStateInfo outInfo = TaleUtil.Props.dialog.animator.GetCurrentAnimatorStateInfo(0);

                    if(Advance())
                        TaleUtil.Props.dialog.animator.speed = TaleUtil.Config.TRANSITION_SKIP_SPEED;

                    if(!outInfo.IsName(TaleUtil.Config.DIALOG_CANVAS_ANIMATOR_STATE_OUT) || outInfo.normalizedTime < 1f)
                        break;

                    TaleUtil.Props.dialog.animator.speed = 1f;
                    TaleUtil.Props.dialog.animator.SetTrigger(TaleUtil.Config.DIALOG_CANVAS_ANIMATOR_TRIGGER_NEUTRAL);
                    TaleUtil.Props.dialog.canvas.SetActive(false);

                    return true;
                }
            }

            return false;
        }
    }
}