using UnityEngine;

namespace TaleUtil
{
    public class WaitForAction : Action
    {
        string trigger;

        WaitForAction() { }

        public WaitForAction(string trigger)
        {
            this.trigger = trigger;
        }

        public override Action Clone()
        {
            WaitForAction clone = new WaitForAction();
            clone.delta = delta;
            clone.trigger = trigger;

            return clone;
        }

        public override bool Run()
        {
            return TaleUtil.Triggers.Get(trigger);
        }

        public override string ToString()
        {
            return string.Format("WaitForAction ({0})", trigger);
        }
    }
}