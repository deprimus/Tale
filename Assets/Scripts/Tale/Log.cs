using UnityEngine;

namespace TaleUtil
{
    public static class Log
    {
        public static void Info(string category, string msg) =>
            Debug.Log("[TALE] [" + category + "] " + msg);

        public static void Warning(string category, string msg) =>
            Debug.LogWarning("[TALE] [" + category + "] " + msg);

        public static void Warning(string msg) =>
            Debug.LogWarning("[TALE] " + msg);

        public static void Error(string category, string msg) =>
            Debug.LogError("[TALE] [" + category + "] " + msg);
    }
}