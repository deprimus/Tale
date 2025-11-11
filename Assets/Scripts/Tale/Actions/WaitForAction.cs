using UnityEngine;

namespace TaleUtil {
    public class WaitForAction : Action {
        string trigger;

        public WaitForAction Init(string trigger) {
            this.trigger = trigger;

            return this;
        }

        protected override bool Run() {
            return master.Triggers.Get(trigger);
        }

        public override string ToString() =>
            string.Format("WaitForAction ({0})", trigger);
    }
}