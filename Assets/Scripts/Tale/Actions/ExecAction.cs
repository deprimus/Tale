namespace TaleUtil {
    public class ExecAction : Action {
        Delegates.ShallowDelegate action;

        public ExecAction Init(Delegates.ShallowDelegate action) {
            this.action = action;

            return this;
        }

        protected override bool Run() {
            action();
            return true;
        }

        protected override void OnInterrupt() {
            action();
        }

        public override string ToString() {
            return "ExecAction";
        }
    }
}