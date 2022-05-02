using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class TransitionAction : Action
    {
        public enum Type
        {
            IN,
            OUT
        }

        private enum State
        {
            SETUP,
            TRANSITION
        }

        private Props.TransitionData data;
        private float duration;

        private Type type;

        private string animatorState;
        private string trigger;

        private State state;

        private TransitionAction() { }

        public TransitionAction(string transition, Type type, float duration)
        {
            transition = transition.ToLowerInvariant();

            Assert.Condition(Props.transitions.ContainsKey(transition), string.Format("Unregistered transition '{0}'", transition));

            data = Props.transitions[transition];

            Assert.Condition(data.canvas != null, string.Format("Transition '{0}' does not have a canvas associated with it; did you forget to register it in TaleMaster?", transition));
            Assert.Condition(data.animator != null, string.Format("Transition '{0}' does not have an animator associated with it; did you forget to register it in TaleMaster?", transition));

            this.type = type;
            this.duration = duration;

            animatorState = string.Format(Config.TRANSITION_ANIMATOR_STATE_FORMAT, type == Type.IN ? "In" : "Out");
            trigger = string.Format(Config.TRANSITION_ANIMATOR_TRIGGER_FORMAT, type == Type.IN ? "In" : "Out");

            state = State.SETUP;
        }

        public override Action Clone()
        {
            TransitionAction clone = new TransitionAction();
            clone.data = data;
            clone.duration = duration;
            clone.type = type;
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
                        data.animator.speed = Config.TRANSITION_INSTANT_SPEED;
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
                    data.animator.SetTrigger(Config.TRANSITION_ANIMATOR_TRIGGER_NEUTRAL);

                    if(type ==  Type.IN)
                        data.canvas.SetActive(false);

                    return true;
                }
            }

            return false;
        }
    }
}
