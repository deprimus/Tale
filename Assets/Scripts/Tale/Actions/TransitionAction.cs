using UnityEngine;

namespace TaleUtil {
    public class TransitionAction : Action {
        public enum Type {
            IN,
            OUT
        }

        enum State {
            SETUP,
            SETUP_IN,
            TRANSITION
        }

        string transition;
        TaleUtil.Props.TransitionData data;
        float duration;

        Type type;

        string animatorState;
        string trigger;

        State state;

        public TransitionAction Init(string transition, Type type, float duration) {
            this.type = type;

            switch (type) {
                case Type.OUT: {
                    PrepareTransition(transition, type, duration);
                    break;
                }
                case Type.IN: {
                    // Actual transition will be checked at runtime
                    this.duration = duration;

                    state = State.SETUP_IN;
                    break;
                }
            }

            return this;
        }

        void PrepareTransition(string transition, Type type, float duration) {
            Debug.Assert.Condition(!string.IsNullOrEmpty(transition), "Transition name can't be null or empty");

            this.transition = transition.ToLowerInvariant();

            Debug.Assert.Condition(master.Props.transitions.entries.ContainsKey(this.transition), string.Format("Unknown transition '{0}'", this.transition));

            data = master.Props.transitions.entries[this.transition];

            Debug.Assert.Condition(data.canvas != null, string.Format("Transition '{0}' does not have a canvas associated with it; did you forget to register it in TaleMaster?", transition));
            Debug.Assert.Condition(data.animator != null, string.Format("Transition '{0}' does not have an animator associated with it; did you forget to register it in TaleMaster?", transition));

            this.type = type;
            this.duration = duration;

            animatorState = string.Format(Config.Editor.TRANSITION_ANIMATOR_STATE_FORMAT, type == Type.IN ? "In" : "Out");
            trigger = string.Format(Config.Editor.TRANSITION_ANIMATOR_TRIGGER_FORMAT, type == Type.IN ? "In" : "Out");

            state = State.SETUP;
        }

        protected override bool Run() {
            switch (state) {
                case State.SETUP: {
                    if (type == Type.OUT) {
                        // TODO: it would be nice to also show the stack of the first call
                        Check(!master.Props.transitions.HasLastTransition(), "Tale.TransitionOut called twice in a row; did you forget to call Tale.TransitionIn?");

                        master.Props.transitions.lastName = transition;
                        master.Props.transitions.lastDuration = duration;
                    }

                    if (!data.canvas.activeSelf)
                        data.canvas.SetActive(true);

                    if (duration == 0f)
                        data.animator.speed = master.Config.Transitions.INSTANT_SPEED;
                    else data.animator.speed = 1f / duration;

                    data.animator.SetTrigger(trigger);

                    state = State.TRANSITION;

                    break;
                }
                case State.SETUP_IN: {
                    if (!master.Props.transitions.HasLastTransition()) {
                        Log.Warning("There is no last transition; Tale.TransitionIn will do nothing");
                        return true;
                    }

                    float duration = this.duration == Tale.Default.FLOAT ? master.Props.transitions.lastDuration : this.duration;

                    PrepareTransition(master.Props.transitions.lastName, Type.IN, duration);

                    master.Props.transitions.ResetLast();

                    break;
                }
                case State.TRANSITION: {
                    AnimatorStateInfo info = data.animator.GetCurrentAnimatorStateInfo(0);

                    if (!info.IsName(animatorState) || info.normalizedTime < 1f)
                        break;

                    data.animator.speed = 1f;
                    data.animator.SetTrigger(TaleUtil.Config.Editor.TRANSITION_ANIMATOR_TRIGGER_NEUTRAL);

                    if (type == Type.IN)
                        data.canvas.SetActive(false);

                    return true;
                }
            }

            return false;
        }

        public override string ToString() {
            string name = "";

            if (!string.IsNullOrEmpty(transition)) {
                name = string.Format("<color=#{0}>{1}</color>, ", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_SECONDARY), transition);
            }

            return string.Format("TransitionAction ({0}<color=#{1}>{2}</color>, <color=#{1}>{3}</color>)", name, ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), type.ToString(), state.ToString());
        }
    }
}
