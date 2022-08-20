using UnityEngine;

namespace TaleUtil
{
    public class DelayedAction : TaleUtil.Action
    {
        float amount;
        TaleUtil.Action action;

        float clock;

        DelayedAction() { }

        public DelayedAction(float amount, TaleUtil.Action action)
        {
            this.amount = amount;
            this.action = action;

            TaleUtil.Queue.RemoveLast(action);

            clock = 0f;
        }

        public override TaleUtil.Action Clone()
        {
            DelayedAction clone = new DelayedAction();
            clone.amount = amount;
            clone.clock = clock;

            return clone;
        }

        public override bool Run()
        {
            clock += Time.deltaTime;

            if (clock >= amount)
            {
                return action.Run();
            }

            return false;
        }
    }
}