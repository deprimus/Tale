using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class TransitionAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            TRANSITION
        }

        private TaleUtil.Props.TransitionData data;
        private float duration;

        private bool isIn;

        private string animatorState;
        private string trigger;

        private State state;

        private TransitionAction() { }

        public TransitionAction(string transition, bool isIn, float duration)
        {
            transition = transition.ToLowerInvariant();

            TaleUtil.Assert.Condition(TaleUtil.Props.transitions.ContainsKey(transition), string.Format("Unregistered transition '{0}'", transition));

            data = TaleUtil.Props.transitions[transition];

            TaleUtil.Assert.NotNull(data.canvas, string.Format("Transition '{0}' does not have a canvas associated with it; did you forget to register it in TaleMaster?", transition));
            TaleUtil.Assert.NotNull(data.animator, string.Format("Transition '{0}' does not have an animator associated with it; did you forget to register it in TaleMaster?", transition));

            this.isIn = isIn;
            this.duration = duration;

            animatorState = string.Format(TaleUtil.Config.TRANSITION_ANIMATOR_STATE_FORMAT, isIn ? "In" : "Out");
            trigger = string.Format(TaleUtil.Config.TRANSITION_ANIMATOR_TRIGGER_FORMAT, isIn ? "In" : "Out");

            state = State.SETUP;
        }

        public override TaleUtil.Action Clone()
        {
            TransitionAction clone = new TransitionAction();
            clone.data = data;
            clone.duration = duration;
            clone.isIn = isIn;
            clone.animatorState = animatorState;
            clone.trigger = trigger;
            clone.state = state;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                {
                    if(!data.canvas.activeSelf)
                        data.canvas.SetActive(true);

                    if(duration == 0)
                        data.animator.speed = TaleUtil.Config.TRANSITION_INSTANT_SPEED;
                    else data.animator.speed = 1f / duration;

                    data.animator.SetTrigger(trigger);

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    AnimatorStateInfo info = data.animator.GetCurrentAnimatorStateInfo(0);

                    if(!info.IsName(animatorState) || info.normalizedTime < 1f)
                        break;

                    data.animator.speed = 1f;
                    data.animator.SetTrigger(TaleUtil.Config.TRANSITION_ANIMATOR_TRIGGER_NEUTRAL);

                    if(isIn)
                        data.canvas.SetActive(false);

                    return true;
                }
            }

            return false;
        }
    }
}
