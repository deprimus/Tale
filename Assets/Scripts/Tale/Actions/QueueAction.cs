using System.Collections.Generic;

namespace TaleUtil {
    public class QueueAction : Action {
        Action[] actions;
        int index;

        public QueueAction Init(Action[] actions) {
            this.actions = actions;
            index = 0;

            for (var i = 0; i < actions.Length; ++i) {
                actions[i].parent = this;
            }

            return this;
        }

        public override Action FetchNext() {
            var next = index + 1;
            return next < actions.Length ? actions[next] : parent?.FetchNext();
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