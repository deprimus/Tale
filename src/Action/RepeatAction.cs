using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class RepeatAction : TaleUtil.Action
    {
        private ulong count;
        private LinkedList<TaleUtil.Action> actions;

        private LinkedList<TaleUtil.Action> current;
        private LinkedListNode<TaleUtil.Action> currentNode;

        private RepeatAction() { }

        public RepeatAction(ulong count, TaleUtil.Action[] actions)
        {
            this.count = count;
            this.actions = new LinkedList<TaleUtil.Action>();

            for (int i = actions.Length - 1; i >= 0; --i)
            {
                this.actions.AddFirst(actions[i]);

                TaleUtil.Action queueAction = TaleUtil.Queue.FetchLast();

                if (queueAction != null && actions[i] == queueAction)
                {
                    TaleUtil.Queue.Remove(queueAction); // Remove the action from the queue, because it will be added to the repeat list.
                }
            }

            RebuildList();
        }

        private void RebuildList()
        {
            current = new LinkedList<TaleUtil.Action>();
            LinkedListNode<TaleUtil.Action> node = actions.First;

            while(node != null)
            {
                current.AddLast(node.Value.Clone());
                node = node.Next;
            }

            currentNode = current.First;
        }

        public override TaleUtil.Action Clone()
        {
            RepeatAction clone = new RepeatAction();
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
            if(actions.Count == 0)
                return true;

            if(currentNode.Value.Run())
            {
                currentNode = currentNode.Next;

                if(currentNode == null)
                {
                    switch(count)
                    {
                        case 0:
                            // 0 = repeat forever
                            RebuildList();
                            return false;

                        case 1:
                            // 1 = this was the last repetition
                            return true;

                        default:
                            --count;
                            RebuildList();

                            return false;
                    }
                }
            }

            return false;
        }
    }
}