using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class ParallelQueueAction : TaleUtil.Action
    {
        private LinkedList<TaleUtil.Action> actions;

        private ParallelQueueAction() { }

        public ParallelQueueAction(TaleUtil.Action[] actions)
        {
            this.actions = new LinkedList<TaleUtil.Action>();

            for(int i = actions.Length - 1; i >= 0; --i)
            {
                this.actions.AddFirst(actions[i]);

                TaleUtil.Action queueAction = TaleUtil.Queue.FetchLast();

                if(queueAction != null && actions[i] == queueAction)
                {
                    TaleUtil.Queue.Remove(queueAction); // Remove the action from the queue, because it will be added to the parallel queue.
                }
            }
        }

        public override TaleUtil.Action Clone()
        {
            ParallelQueueAction clone = new ParallelQueueAction();
            clone.actions = new LinkedList<TaleUtil.Action>();

            LinkedListNode<TaleUtil.Action> node = actions.First;

            while (node != null)
            {
                clone.actions.AddLast(node.Value.Clone());
                node = node.Next;
            }

            return clone;
        }

        public override bool Run()
        {
            LinkedListNode<TaleUtil.Action> node = actions.First;

            if(node == null)
                return true;

            if(node.Value.Run())
                actions.RemoveFirst();

            return actions.Count == 0;
        }

        public override string ToString()
        {
            return "ParallelQueueAction";
        }
    }
}