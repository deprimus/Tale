namespace TaleUtil
{
    public static class Log
    {
        public static void Info(string category, string msg) =>
            UnityEngine.Debug.Log(string.Format("[TALE] [{0}] {1}", category, msg));

        public static void Warning(string category, string msg) =>
            UnityEngine.Debug.LogWarning(string.Format("[TALE] [{0}] {1}", category, msg));

        public static void Warning(string msg) =>
            UnityEngine.Debug.LogWarning(string.Format("[TALE] {0}", msg));

        public static void Error(string category, string msg) =>
            UnityEngine.Debug.LogError(string.Format("[TALE] [{0}] {1}", category, msg));
    }
}