using System.Collections.Generic;

namespace TaleUtil {
    public class BindAction : Action {
        public Action primary;
        public Action secondary;

        bool secondaryDone;

        public BindAction Init(Action primary, Action secondary) {
            this.primary = primary;
            this.secondary = secondary;

            secondaryDone = false;

            return this;
        }

        protected override bool Run() {
            if (primary.Execute()) {
                if (!secondaryDone) {
                    secondary.Interrupt();
                }
                return true;
            }

            if (!secondaryDone) {
                secondaryDone = secondary.Execute();
            }

            return false;
        }

        public override IEnumerable<Action> GetSubactions() {
            yield return primary;
            yield return secondary;
        }

        public override string ToString() =>
            "BindAction";
    }
}