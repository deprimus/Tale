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

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback) {
            base.SetDeltaCallback(callback);
            action.SetDeltaCallback(callback);
        }

        public override bool Run() {
            if (master.Triggers.Get(trigger)) {
                action.OnInterrupt();
                return true;
            }

            return action.Run();
        }

        public override string ToString() {
            return string.Format("Interruptible action (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGB(master.config.Core.DEBUG_ACCENT_COLOR_PRIMARY), action.ToString());
        }
    }
}