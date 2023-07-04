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
            clone.delta = delta;
            clone.amount = amount;
            clone.clock = clock;

            return clone;
        }

        public override bool Run()
        {
            clock += delta();

            return (clock >= amount);
        }

        public override string ToString()
        {
            return string.Format("WaitAction ({0} left)", Mathf.Max(0f, amount - clock).ToString("0.0"));
        }
    }
}