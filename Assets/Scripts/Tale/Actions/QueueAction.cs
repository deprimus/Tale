using System.Collections.Generic;

namespace TaleUtil {
    public class QueueAction : Action {
        Action[] actions;
        int index;

        public QueueAction Init(Action[] actions) {
            this.actions = actions;
            index = 0;

            return this;
        }

        protected override bool Run() {
            if (index < actions.Length && actions[index].Execute()) {
                ++index;
            }

            return index == actions.Length;
        }

        public override IEnumerable<Action> GetSubactions() {
            for (int i = index; i < actions.Length; ++i) {
                yield return actions[i];
            }
        }

        public override string ToString() =>
            "QueueAction";
    }
}