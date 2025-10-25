using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public static class Flags
    {
        private static Dictionary<string, ulong> entries;

        public static void Set(string name, ulong value = 1) =>
            entries[name] = value;

        public static void Increment(string name, ulong value = 1) =>
            entries[name] = Get(name) + value;

        public static ulong Get(string name)
        {
            if (entries.TryGetValue(name, out var value))
            {
                return value;
            }

            return 0ul;
        }

        public static void Remove(string name) =>
            entries.Remove(name);

        public static Dictionary<string, ulong> Entries() => entries;

        public static void Init()
        {
            entries = new Dictionary<string, ulong>();
        }
    }
}