using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class WaitAction : TaleUtil.Action
    {
        private float amount;
        private float clock;

        private WaitAction() { }

        public WaitAction(float amount)
        {
            this.amount = amount;
            clock = 0f;
        }

        public override TaleUtil.Action Clone()
        {
            WaitAction clone = new WaitAction();
            clone.amount = amount;
            clone.clock = clock;

            return clone;
        }

        public override bool Run()
        {
            clock += Time.deltaTime;

            if(clock >= amount)
                return true;
            return false;
        }
    }
}