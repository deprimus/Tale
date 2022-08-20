using UnityEngine;

namespace TaleUtil
{
    public class WaitAction : Action
    {
        float amount;
        float clock;

        WaitAction() { }

        public WaitAction(float amount)
        {
            this.amount = amount;
            clock = 0f;
        }

        public override Action Clone()
        {
            WaitAction clone = new WaitAction();
            clone.amount = amount;
            clone.clock = clock;

            return clone;
        }

        public override bool Run()
        {
            clock += Time.deltaTime;

            return (clock >= amount);
        }
    }
}