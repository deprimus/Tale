using System.Collections.Generic;

namespace TaleUtil {
    public class AnyAction : Action {
        public Action[] actions;

        public AnyAction Init(Action[] actions) {
            this.actions = actions;

            return this;
        }

        public override bool Run() {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i].Run()) {
                    // Finish when any action is done
                    return true;
                }
            }

            return false;
        }

        public override IEnumerable<Action> GetSubactions() =>
            actions;

        public override string ToString() =>
            "AnyAction";
    }
}