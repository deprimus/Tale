using System.Text;
using System.Diagnostics;

namespace TaleUtil {
    public static class Debug {
        public static void Log(object msg) =>
            UnityEngine.Debug.Log(string.Format("[TALE] {0}", msg));

        public static void Log(string category, object msg) =>
            Log(string.Format("[{0}] {1}", category, msg));

        public static void LogWarning(object msg) =>
           UnityEngine.Debug.LogWarning(string.Format("[TALE] {0}", msg));

        public static void LogWarning(string category, object msg) =>
            LogWarning(string.Format("[{0}] {1}", category, msg));

        public static void LogError(object msg) =>
            UnityEngine.Debug.LogError(string.Format("[TALE] {0}", msg));

        public static void LogError(string category, object msg) =>
            LogError(string.Format("[{0}] {1}", category, msg));

        public static class Assert {
            [Conditional("UNITY_ASSERTIONS")]
            public static void Condition(bool condition, string msg) =>
                UnityEngine.Debug.Assert(condition, "[TALE] " + msg);

            [Conditional("UNITY_ASSERTIONS")]
            public static void Impossible(string msg) =>
                UnityEngine.Debug.Assert(false, "[TALE] " + msg);
        }

        // These don't stop the execution, and also aren't removed in release builds
        public static class SoftAssert {
            public static bool Condition(bool condition, string msg) {
                if (!condition) {
                    LogWarning(msg);
                }

                return condition;
            }
        }

        public static void StackTraceToString(StackTrace stack, Delegates.FilterDelegate<StackFrame> filter, StringBuilder outStr) {
            for (int i = 0; i < stack.FrameCount; i++) {
                var frame = stack.GetFrame(i);

                if (!filter(frame)) {
                    continue;
                }

                FrameToString(frame, outStr);
            }
        }

        public static void FrameToString(StackFrame frame, StringBuilder outStr) {
            var method = frame.GetMethod();
            var filename = frame.GetFileName();
            var file = string.IsNullOrEmpty(filename) ? "" : Path.AbsoluteToAssetPath(filename);

            outStr.Append(method.DeclaringType.ToString());
            outStr.Append(':');
            outStr.Append(method.Name);

            var p = method.GetParameters();
            outStr.Append('(');
            {
                if (p.Length > 0) {
                    outStr.Append(p[0]);

                    for (int i = 1; i < p.Length; ++i) {
                        outStr.Append(',');
                        outStr.Append(p[i]);
                    }
                }
            }
            outStr.Append(')');

            if (!string.IsNullOrEmpty(file)) {
                outStr.AppendFormat(" (at <a href=\"{0}\" line=\"{1}\">{0}:{1}</a>)", file, frame.GetFileLineNumber());
            }

            outStr.AppendLine();
        }

        internal static bool FilterInternalTaleFrame(StackFrame frame) {
            var type = frame.GetMethod().DeclaringType;

            return type != typeof(TaleMaster) && type != typeof(Action);
        }
    }
}