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

        public BranchAction Init(string flag, Delegates.BranchDelegate<ulong> action)
        {
            this.flag = flag;
            this.action = action;

            state = State.SETUP;

            return this;
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

                    returned.SetDeltaCallback(delta);

                    state = State.RUN;

                    return returned.Run();
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

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback) {
            base.SetDeltaCallback(callback);

            if (returned != null) {
                returned.SetDeltaCallback(callback);
            }
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