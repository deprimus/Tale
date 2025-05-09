using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    // Triggers can be set from any script, and scripts can be executed in any order.
    // Therefore, for consistency, triggers will take effect during the next frame after they are set.
    // This ensures that all scripts will get the same result when checking for a trigger.
    public static class Triggers
    {
        private static HashSet<string> current;     // Triggers for the current frame.
        private static HashSet<string> accumulator; // Triggers for the next frame.

        public static void Set(string name) =>
            accumulator.Add(name);

        public static bool Get(string name) =>
            current.Contains(name);

        public static bool GetImmediate(string name) =>
            accumulator.Contains(name) || Get(name);

        public static void Init()
        {
            current = new HashSet<string>();
            accumulator = new HashSet<string>();
        }

        public static void Update()
        {
            current = accumulator;
            accumulator = new HashSet<string>();

            if (current.Count > 0 && Hooks.OnTriggerUpdate != null)
            {
                Hooks.OnTriggerUpdate(current);
            }
        }
    }
}