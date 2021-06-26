using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class ParallelAction : TaleUtil.Action
    {
        private TaleUtil.Action[] actions;

        public ParallelAction() { }

        public ParallelAction(TaleUtil.Action[] actions)
        {
            this.actions = actions;

            for(int i = actions.Length - 1; i >= 0; --i)
            {
                TaleUtil.Action queueAction = TaleUtil.Queue.FetchLast();

                if(queueAction != null && this.actions[i] == queueAction)
                {
                    TaleUtil.Queue.Remove(queueAction); // Remove the action from the queue, because it will be added to the parallel list.
                }
            }
        }

        public override TaleUtil.Action Clone()
        {
            ParallelAction clone = new ParallelAction();
            clone.actions = new TaleUtil.Action[actions.Length];

            for(int i = 0; i < actions.Length; ++i)
                clone.actions[i] = actions[i].Clone();

            return clone;
        }

        public override bool Run()
        {
            TaleUtil.Parallel.Add(actions);
            return true;
        }
    }
}