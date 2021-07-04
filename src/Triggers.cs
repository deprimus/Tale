using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
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
        }
    }
}