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

    // These don't stop the execution, and also aren't removed in release builds
    public static class SoftAssert
    {
        public static bool Condition(bool condition, string msg)
        {
            if (!condition)
            {
                Log.Warning(msg);
            }

            return condition;
        }
    }
}