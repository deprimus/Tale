namespace TaleUtil
{
    public class RepeatAction : Action
    {
        enum State { 
            SETUP,
            RUN
        }

        ulong count;
        State state;

        TaleUtil.Action currentAction;
        Delegates.ActionDelegate action;

        public RepeatAction Init(ulong count, Delegates.ActionDelegate action) {
            this.count = count;
            this.action = action;

            state = State.SETUP;

            return this;
        }

        public override bool Run()
        {
            switch (state) {
                case State.SETUP: {
                    GetAction();

                    state = State.RUN;
                    return RunAction();
                }
                case State.RUN: {
                    return RunAction();
                }
            }

            return false;
        }

        void GetAction() {
            currentAction = action();
            currentAction.SetDeltaCallback(delta);
        }

        bool RunAction() {
            if (currentAction.Run()) {
                switch (count) {
                    case 0: {
                        // 0 = repeat forever
                        state = State.SETUP;
                        return false;
                    }
                    case 1: {
                        // 1 = this was the last repetition
                        return true;
                    }
                    default: {
                        --count;
                        state = State.SETUP;
                        return false;
                    }
                }
            }

            return false;
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback) {
            base.SetDeltaCallback(callback);

            if (currentAction != null) {
                currentAction.SetDeltaCallback(callback);
            }
        }

        public override string ToString()
        {
            string left = (count == 0 ? "loop" : count.ToString() + " left");

            return string.Format("RepeatAction ({0}, {1})", left, currentAction?.ToString());
        }
    }
}