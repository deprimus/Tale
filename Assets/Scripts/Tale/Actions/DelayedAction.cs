using UnityEngine;

namespace TaleUtil
{
    public class DelayedAction : Action
    {
        float amount;
        Action action;

        float clock;

        public DelayedAction Init(float amount, Action action)
        {
            this.amount = amount;
            this.action = action;

            clock = 0f;

            return this;
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);
            action.SetDeltaCallback(callback);
        }

        public override bool Run()
        {
            if (clock >= amount)
            {
                return action.Run();
            }
            else
            {
                clock += delta();

                return false;
            }
        }

        public override void OnInterrupt()
        {
            clock = amount;
            action.OnInterrupt();
        }

        public override string ToString()
        {
            return string.Format("DelayedAction ({0} left)", Mathf.Max(0f, amount - clock).ToString("0.0"));
        }
    }
}