namespace TaleUtil
{
    public class BranchAction : Action
    {
        enum State
        {
            SETUP,
            RUN,
            END
        };

        string flag;
        Delegates.BranchDelegate<ulong> action;

        State state;
        TaleUtil.Action returned;

        BranchAction() { }

        public BranchAction(string flag, Delegates.BranchDelegate<ulong> action)
        {
            this.flag = flag;
            this.action = action;

            state = State.SETUP;
        }

        public override Action Clone()
        {
            BranchAction clone = new BranchAction();
            clone.delta = delta;
            clone.flag = flag;
            clone.action = action;
            clone.state = state;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                {
                    returned = action(TaleUtil.Flags.Get(flag));

                    if (returned == null)
                    {
                        state = State.END;
                        return true;
                    }

                    state = State.RUN;
                    break;
                }
                case State.RUN:
                {
                    return returned.Run();
                }
                case State.END:
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnInterrupt()
        {
            state = State.END; // TODO: Implement this properly
        }

        public override string ToString()
        {
            return string.Format("BranchAction ({0}, {1})", flag, state.ToString());
        }
    }
}