using System.Linq;
using System.Collections.Generic;

namespace TaleUtil
{
    public class Queue
    {
        // TODO: use a list
        private LinkedList<TaleUtil.Action> data;

        private ulong totalActionCount = 0;

        public Queue() {
            data = new LinkedList<TaleUtil.Action>();
        }

        public TaleUtil.Action Fetch() =>
            data.First.Value;

        public TaleUtil.Action FetchNext() =>
            data.Count > 1 ? data.ElementAt(1) : null;

        public TaleUtil.Action FetchLast() =>
            data.Count > 0 ? data.ElementAt(data.Count - 1) : null;

        public TaleUtil.Action FetchIfAny() =>
            data.Count > 0 ? data.First.Value : null;

        public int Count() =>
            data.Count;

        public void ForceClear() =>
            data.Clear();

        // If the queue has actions, and the last action is this one, remove it.
        // This is used by multiple actions, such as QueueAction, which handle the
        // actions on their own.
        public void RemoveLast(TaleUtil.Action action)
        {
            Action queueAction = FetchLast();

            if (queueAction != null && action == queueAction)
            {
                data.RemoveLast();
            }
        }

        public void RemoveLast(TaleUtil.Action[] actions) {
            // C# arg order is left -> right, so the 'params' keyword always pushes to the queue in-order
            for (int i = actions.Length - 1; i >= 0; --i) {
                RemoveLast(actions[i]);
            }
        }

        public TaleUtil.Action Enqueue(TaleUtil.Action act)
        {
            data.AddLast(act);
            ++totalActionCount;
            return act;
        }

        public void Dequeue() =>
            data.RemoveFirst();

        public bool Run()
        {
            if (data.Count > 0 && data.First.Value.Run())
                Dequeue(); // Run if queue is not empty. After running, if the action is done, dequeue.

            return data.Count == 0;
        }

        public ulong GetTotalActionCount()
        {
            return totalActionCount;
        }
    }
}