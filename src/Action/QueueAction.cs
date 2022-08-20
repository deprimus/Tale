using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class QueueAction : TaleUtil.Action
    {
        private LinkedList<TaleUtil.Action> actions;

        private QueueAction() { }

        public QueueAction(TaleUtil.Action[] actions)
        {
            this.actions = new LinkedList<TaleUtil.Action>();

            for(int i = actions.Length - 1; i >= 0; --i)
            {
                this.actions.AddFirst(actions[i]);

                // Remove the action from the big Tale queue, because it will be added to this internal queue.
                TaleUtil.Queue.RemoveLast(actions[i]);
            }
        }

        public override TaleUtil.Action Clone()
        {
            QueueAction clone = new QueueAction();
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
    }
}