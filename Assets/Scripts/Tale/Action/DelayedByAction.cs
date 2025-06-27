using UnityEngine;

namespace TaleUtil
{
    public class DelayedByAction : Action
    {
        string trigger;
        Action action;

        enum State
        {
            WAIT_FOR_TRIGGER,
            RUN,
            END
        };

        State state;

        DelayedByAction() { }

        public DelayedByAction(string trigger, Action action)
        {
            this.trigger = trigger;
            this.action = action;

            // Remove the action from the Tale queue because it will be handled here
            Queue.RemoveLast(action);

            state = State.WAIT_FOR_TRIGGER;
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);
            action.SetDeltaCallback(callback);
        }

        public override Action Clone()
        {
            DelayedByAction clone = new DelayedByAction();
            clone.delta = delta;
            clone.trigger = trigger;
            clone.state = state;
            clone.action = action.Clone();

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.WAIT_FOR_TRIGGER:
                {
                    if (TaleUtil.Triggers.Get(trigger))
                    {
                        state = State.RUN;
                    }
                    break;
                }
                case State.RUN:
                {
                    bool done = action.Run();

                    if (done)
                    {
                        state = State.END;
                    }

                    return done;
                }
                case State.END:
                {
                    TaleUtil.Log.Warning("DelayedByAction.Run() called when action was already done");
                    return true;
                }
            }

            return false;
        }

        public override void OnInterrupt()
        {
            if (state != State.END)
            {
                state = State.END;
                action.OnInterrupt();
            }
        }

        public override string ToString()
        {
            return string.Format("DelayedByAction ({0}, {1})", state.ToString(), trigger);
        }
    }
}