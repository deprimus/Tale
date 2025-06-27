using System.Collections.Generic;

namespace TaleUtil
{
    public class AnyAction : Action
    {
        public LinkedList<Action> actions;

        AnyAction() { }

        public AnyAction(Action[] actions)
        {
            this.actions = new LinkedList<Action>();

            for(int i = actions.Length - 1; i >= 0; --i)
            {
                this.actions.AddFirst(actions[i]);

                // Remove the action from the queue, because it will be added to the list.
                Queue.RemoveLast(actions[i]);
            }
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

        public override Action Clone()
        {
            AnyAction clone = new AnyAction();
            clone.delta = delta;
            clone.actions = new LinkedList<Action>();

            LinkedListNode<Action> node = actions.First;

            while(node != null)
            {
                clone.actions.AddLast(node.Value.Clone());
                node = node.Next;
            }

            return clone;
        }

        public override bool Run()
        {
            LinkedListNode<Action> node = actions.First;

            while(node != null)
            {
                if(node.Value.Run())
                {
                    // Finish when any action is done
                    actions.Clear();
                    return true;
                }
                else
                {
                    node = node.Next;
                }
            }

            return false;
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
            return "AnyAction";
        }
    }
}