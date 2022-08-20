namespace TaleUtil
{
    public class RepeatAction : TaleUtil.Action
    {
        ulong count;
        TaleUtil.Action action;
        TaleUtil.Action originalAction;

        RepeatAction() { }

        public RepeatAction(ulong count, Action action)
        {
            this.count = count;
            originalAction = action;
            this.action = originalAction.Clone();

            TaleUtil.Queue.RemoveLast(action);
        }

        public override TaleUtil.Action Clone()
        {
            RepeatAction clone = new RepeatAction(count, originalAction);

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