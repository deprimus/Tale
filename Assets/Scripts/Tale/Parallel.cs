using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class Parallel
    {
        private ulong lastId = 0;
        private LinkedList<TaleUtil.Action> data;

        public LinkedListNode<TaleUtil.Action> Fetch() =>
            data.First;


        public Parallel() {
            data = new LinkedList<TaleUtil.Action>();
        }

        public ulong Add(params TaleUtil.Action[] actions)
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

        public void Remove(ulong id)
        {
            Remove(id, 1);
        }

        public void Remove(ulong id, ulong size)
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

        public void ForceClear() =>
            data.Clear();

        public void Run()
        {
            LinkedListNode<TaleUtil.Action> node = data.First;

            while(node != null)
            {
                // Run the action. If it's done, remove it. Move on to the next action, repeat.
                // That's it.
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
    }
}