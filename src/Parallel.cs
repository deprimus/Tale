using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public static class Parallel
    {
        private static ulong lastId = 0;
        private static LinkedList<TaleUtil.Action> data;

        public static LinkedListNode<TaleUtil.Action> Fetch() =>
            data.First;

        public static ulong Add(params TaleUtil.Action[] actions)
        {
            if(actions.Length == 0)
                return 0;

            for(int i = 0; i < actions.Length; ++i)
            {
                actions[i].id = ++lastId;
                data.AddLast(actions[i]);
            }

            return actions[0].id;
        }

        public static void Remove(ulong id)
        {
            Remove(id, 1);
        }

        public static void Remove(ulong id, ulong size)
        {
            if(size == 0)
                return;

            LinkedListNode<TaleUtil.Action> node = data.First;

            while (node != null)
            {
                if (node.Value.id == id)
                {
                    do
                    {
                        LinkedListNode<TaleUtil.Action> next = node.Next;
                        data.Remove(node);
                        node = node.Next;

                        --size;
                    } while(node != null && size > 0);

                    return;
                }

                node = node.Next;
            }
        }

        public static void Init() => data = new LinkedList<TaleUtil.Action>();

        public static void Run()
        {
            LinkedListNode<TaleUtil.Action> node = data.First;

            while(node != null)
            {
                if(node.Value.Run())
                {
                    LinkedListNode<TaleUtil.Action> next = node.Next;
                    data.Remove(node);
                    node = next;
                }
                else
                {
                    node = node.Next;
                }
            }
        }

        public class Pointer
        {
            public ulong start;
            public ulong size;

            public Pointer() : this(0, 0) { }

            public Pointer(ulong start, ulong size)
            {
                this.start = start;
                this.size = size;
            }

            public void Stop()
            {
                if(start == 0 && size == 0)
                    TaleUtil.Queue.Enqueue(new TaleUtil.ParallelStopAction(this));
                else Remove(start, size);
            }
        }
    }
}