using UnityEngine;

namespace TaleUtil {
    public class InterruptibleAction : Action {
        string trigger;
        Action action;

        public InterruptibleAction Init(string trigger, Action action) {
            this.trigger = trigger;
            this.action = action;

            return this;
        }

        protected override bool Run() {
            if (master.Triggers.Get(trigger)) {
                action.Interrupt();
                return true;
            }

            return action.Execute();
        }

        public override string ToString() {
            return string.Format("Interruptible action (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), action.ToString());
        }
    }
}