using System.Collections.Generic;

namespace TaleUtil
{
    public class MultiplexAction : Action
    {
        // TODO: replace linked list with something which doesn't suck
        public LinkedList<Action> actions;

        public MultiplexAction Init(Action[] actions)
        {
            this.actions = new LinkedList<Action>();

            for(int i = actions.Length - 1; i >= 0; --i)
            {
                this.actions.AddFirst(actions[i]);
            }

            return this;
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);

            LinkedListNode<Action> node = actions.First;

            // Change the delta callback for all children
            while (node != null)
            {
                node.Value.SetDeltaCallback(callback);
                node = node.Next;
            }
        }

        public override bool Run()
        {
            LinkedListNode<Action> node = actions.First;

            while(node != null)
            {
                if(node.Value.Run())
                {
                    LinkedListNode<Action> next = node.Next;
                    actions.Remove(node);
                    node = next;
                }
                else
                {
                    node = node.Next;
                }
            }

            return (actions.Count == 0); // Finish when all actions are done.
        }

        public override void OnInterrupt()
        {
            LinkedListNode<Action> node = actions.First;

            while (node != null)
            {
                node.Value.OnInterrupt();
                actions.RemoveFirst();
                node = actions.First;
            }
        }

        public override string ToString()
        {
            return "MultiplexAction";
        }
    }
}