using System.Collections.Generic;

namespace TaleUtil {
    public class AnyAction : Action {
        public Action[] actions;

        public AnyAction Init(Action[] actions) {
            this.actions = actions;

            return this;
        }

        protected override bool Run() {
            // Finish when any action is done
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i].Execute()) {
                    for (int j = 0; j < actions.Length; ++j) {
                        if (j != i) {
                            actions[j].Interrupt();
                        }
                    }

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