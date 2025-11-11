using System.Collections.Generic;

namespace TaleUtil {
    public class ParallelAction : Action {
        Action[] actions;

        public ParallelAction Init(Action[] actions) {
            this.actions = actions;

            return this;
        }

        protected override bool Run() {
            master.Parallel.InsertMany(actions);
            return true;
        }

        public override IEnumerable<Action> GetSubactions() =>
            actions;

        public override string ToString() =>
            "ParallelAction";
    }
}