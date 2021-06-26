using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class ParallelStopAction : TaleUtil.Action
    {
        private TaleUtil.Parallel.Pointer ptr;

        private ParallelStopAction() { }

        public ParallelStopAction(TaleUtil.Parallel.Pointer ptr)
        {
            this.ptr = ptr;
        }

        public override TaleUtil.Action Clone()
        {
            ParallelStopAction clone = new ParallelStopAction();
            clone.ptr = new TaleUtil.Parallel.Pointer(ptr.start, ptr.size);

            return clone;
        }

        public override bool Run()
        {
            ptr.Stop();
            return true;
        }
    }
}