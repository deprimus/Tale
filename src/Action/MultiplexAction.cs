using System.Collections.Generic;

namespace TaleUtil
{
    public class MultiplexAction : Action
    {
        LinkedList<Action> actions;

        MultiplexAction() { }

        // How this works:
        //
        // The Tale.* methods add actions to the queue.
        // In order for Tale.Multiplex(Tale.*, Tale.*, ...) to work, the actions need to be removed from the queue,
        // and to be added to this list.
        // This way, the user can write both:
        //
        // Tale.Cinematic();
        // Tale.Scene();
        //
        // and
        //
        // Tale.Multiplex(
        //     Tale.Cinematic(),
        //     Tale.Scene()
        // );

        public MultiplexAction(Action[] actions)
        {
            this.actions = new LinkedList<Action>();

            for(int i = actions.Length - 1; i >= 0; --i)
            {
                this.actions.AddFirst(actions[i]);

                // Remove the action from the queue, because it will be added to the multiplex list.
                Queue.RemoveLast(actions[i]);
            }
        }

        public override Action Clone()
        {
            MultiplexAction clone = new MultiplexAction();
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
    }
}