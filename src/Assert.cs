using UnityEngine;

namespace TaleUtil
{
    public static class Assert
    {
        public static void Condition(bool condition, string msg) =>
            Debug.Assert(condition, "[TALE] " + msg);

        public static void Impossible(string msg) =>
            Debug.Assert(false, "[TALE] " + msg);
    }
}