using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public static class Assert
    {
        public static void NotNull(object obj, string msg) =>
            Debug.Assert(obj != null, "[TALE] " + msg);

        public static void Condition(bool condition, string msg) =>
            Debug.Assert(condition, "[TALE] " + msg);

        public static void Impossible(string msg) =>
            Debug.Assert(false, "[TALE] " + msg);
    }
}