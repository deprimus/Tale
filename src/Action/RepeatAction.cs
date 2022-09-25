namespace TaleUtil
{
    public class RepeatAction : Action
    {
        ulong count;
        Action action;
        Action originalAction;

        RepeatAction() { }

        public RepeatAction(ulong count, Action action)
        {
            this.count = count;
            originalAction = action;

            Queue.RemoveLast(action);

            this.action = originalAction.Clone();
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);

            originalAction.SetDeltaCallback(callback);
            action.SetDeltaCallback(callback);
        }

        public override Action Clone()
        {
            RepeatAction clone = new RepeatAction(count, originalAction);
            clone.delta = delta;

            return clone;
        }

        public override bool Run()
        {
            if(action.Run())
            {
                switch (count)
                {
                    case 0:
                        // 0 = repeat forever
                        action = originalAction.Clone();
                        return false;

                    case 1:
                        // 1 = this was the last repetition
                        return true;

                    default:
                        --count;
                        action = originalAction.Clone();

                        return false;
                }
            }

            return false;
        }
    }
}