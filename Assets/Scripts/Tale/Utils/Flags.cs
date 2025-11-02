using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class Flags
    {
        Dictionary<string, ulong> entries;

        public Flags() {
            entries = new Dictionary<string, ulong>();
        }

        public void Set(string name, ulong value = 1) =>
            entries[name] = value;

        public void Increment(string name, ulong value = 1) =>
            entries[name] = Get(name) + value;

        public ulong Get(string name)
        {
            if (entries.TryGetValue(name, out var value))
            {
                return value;
            }

            return 0ul;
        }

        public void Remove(string name) =>
            entries.Remove(name);

        public Dictionary<string, ulong> Entries() => entries;
    }
}