using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public static class Extensions
    {
        public static void Set<T>(this List<T> list, int index, T value)
        {
            if(index < 0 || index > list.Count)
                throw new System.IndexOutOfRangeException("The index must be 0 <= index <= list size.");
            else if(index == list.Count)
                list.Add(value);
            else list[index] = value;
        }
    }
}
