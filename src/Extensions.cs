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
        
        // These extension methods rely on 'triangle' animators.
        // Idle --trigger1--> state1 --trigger2-->Idle
        // Idle --trigger3--> state2 --trigger2-->Idle
        // etc
        public static bool StateFinished(this Animator animator, string state)
        {
            return animator.GetCurrentAnimatorStateInfo(0).StateFinished(state);
        }
        public static bool StateFinished(this AnimatorStateInfo info, string state)
        {
            return (info.IsName(state) && info.normalizedTime >= 1f);
        }
    }
}
