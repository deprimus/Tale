using System.Linq;
using System.Collections.Generic;

namespace TaleUtil
{
    public static class Queue
    {
        private static LinkedList<TaleUtil.Action> data;

        private static ulong totalActionCount = 0;

        public static TaleUtil.Action Fetch() =>
            data.First.Value;

        public static TaleUtil.Action FetchNext() =>
            data.Count > 1 ? data.ElementAt(1) : null;

        public static TaleUtil.Action FetchLast() =>
            data.Count > 0 ? data.ElementAt(data.Count - 1) : null;

        public static TaleUtil.Action FetchIfAny() =>
            data.Count > 0 ? data.First.Value : null;

        public static int Count() =>
            data.Count;

        public static void ForceClear() =>
            data.Clear();

        public static void Remove(TaleUtil.Action action) =>
            data.Remove(action);

        public static void RemoveLast(TaleUtil.Action action)
        {
            // If the queue has actions, and the last action is this one, remove it.
            // This is used by multiple actions, such as QueueAction, which handle the
            // actions on their own.
            Action queueAction = FetchLast();

            if(queueAction != null && action == queueAction)
            {
                data.RemoveLast();
            }
        }

        public static TaleUtil.Action Enqueue(TaleUtil.Action act)
        {
            data.AddLast(act);
            ++totalActionCount;
            return act;
        }

        public static void Dequeue() =>
            data.RemoveFirst();


        public static void Init() =>
            data = new LinkedList<TaleUtil.Action>();

        public static bool Run()
        {
            // As simple as it gets.
            if(data.Count > 0 && data.First.Value.Run())
                Dequeue(); // Run if queue is not empty. After running, if the action is done, dequeue.

            return data.Count == 0;
        }

        public static ulong GetTotalActionCount()
        {
            return totalActionCount;
        }
    }
}