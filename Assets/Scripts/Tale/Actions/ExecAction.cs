namespace TaleUtil
{
    public class ExecAction : Action
    {
        Delegates.ShallowDelegate action;

        public ExecAction Init(Delegates.ShallowDelegate action)
        {
            this.action = action;

            return this;
        }

        public override bool Run()
        {
            action();
            return true;
        }

        public override void OnInterrupt()
        {
            action();
        }

        public override string ToString()
        {
            return "ExecAction";
        }
    }
}