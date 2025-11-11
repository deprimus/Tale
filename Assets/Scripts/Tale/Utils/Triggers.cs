using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    // Triggers can be set from any script, and scripts can be executed in any order.
    // Therefore, for consistency, triggers will take effect during the next frame after they are set.
    // This ensures that all scripts will get the same result when checking for a trigger.
    public class Triggers
    {
        TaleMaster master;

        HashSet<string> current;     // Triggers for the current frame.
        HashSet<string> accumulator; // Triggers for the next frame.

        public void Set(string name) =>
            accumulator.Add(name);

        public bool Get(string name) =>
            current.Contains(name);

        public bool GetImmediate(string name) =>
            accumulator.Contains(name) || Get(name);

        public Triggers(TaleMaster master)
        {
            this.master = master;
            current = new HashSet<string>();
            accumulator = new HashSet<string>();
        }

        public void Update()
        {
            var temp = current;
            current = accumulator;
            accumulator = temp;
            accumulator.Clear();

            if (current.Count > 0 && master.Hooks.OnTriggerUpdate != null)
            {
                master.Hooks.OnTriggerUpdate(current);
            }
        }
    }
}