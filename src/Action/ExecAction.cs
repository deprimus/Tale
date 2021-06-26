using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class ExecAction : TaleUtil.Action
    {
        private TaleUtil.Delegates.ShallowDelegate action;

        private ExecAction() { }

        public ExecAction(TaleUtil.Delegates.ShallowDelegate action)
        {
            this.action = action;
        }

        public override TaleUtil.Action Clone()
        {
            ExecAction clone = new ExecAction();
            clone.action = action;

            return clone;
        }

        public override bool Run()
        {
            action();
            return true;
        }
    }
}