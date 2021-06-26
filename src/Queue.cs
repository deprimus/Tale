using System.Linq;
using System.Collections.Generic;

namespace TaleUtil
{
    public static class Queue
    {
        private static LinkedList<TaleUtil.Action> data;

        public static TaleUtil.Action Fetch() =>
            data.First.Value;

        public static TaleUtil.Action FetchNext() =>
            data.Count > 1 ? data.ElementAt(1) : null;

        public static TaleUtil.Action FetchLast() =>
            data.Count > 0 ? data.ElementAt(data.Count - 1) : null;

        public static int Count() =>
            data.Count;

        public static void Remove(TaleUtil.Action action) =>
            data.Remove(action);

        public static TaleUtil.Action Enqueue(TaleUtil.Action act)
        {
            data.AddLast(act);
            return act;
        }

        public static void Dequeue() =>
            data.RemoveFirst();


        public static void Init() =>
            data = new LinkedList<TaleUtil.Action>();

        public static void Run()
        {
            if(data.Count > 0 && data.First.Value.Run())
                Dequeue(); // Run if queue is not empty. After running, if the action is done, dequeue.
        }
    }
}