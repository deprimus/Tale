using UnityEngine;

namespace TaleUtil
{
    public class WaitForAction : Action
    {
        string trigger;

        public WaitForAction Init(string trigger)
        {
            this.trigger = trigger;

            return this;
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