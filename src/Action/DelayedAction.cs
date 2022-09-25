using UnityEngine;

namespace TaleUtil
{
    public class DelayedAction : Action
    {
        float amount;
        Action action;

        float clock;

        DelayedAction() { }

        public DelayedAction(float amount, Action action)
        {
            this.amount = amount;
            this.action = action;

            // Remove the action from the Tale queue because it will be handled here
            Queue.RemoveLast(action);

            clock = 0f;
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);
            action.SetDeltaCallback(callback);
        }

        public override Action Clone()
        {
            DelayedAction clone = new DelayedAction();
            clone.delta = delta;
            clone.amount = amount;
            clone.clock = clock;

            return clone;
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
    }
}