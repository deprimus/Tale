using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class Cues
    {
        class CueToken : IDisposable
        {
            readonly Cues cues;
            readonly string channel;
            readonly Delegates.CallbackDelegate<string> hook;

            public CueToken(Cues cues, string channel, Delegates.CallbackDelegate<string> hook)
            {
                this.cues = cues;
                this.channel = channel;
                this.hook = hook;
            }

            public void Dispose()
            {
                cues.Unhook(channel, hook);
            }
        }

        TaleMaster master;

        Dictionary<string, string> entries;
        Dictionary<string, Delegates.CallbackDelegate<string>> hooks; // Hooks can be set even if there is no entry for a specified channel.

        public void Set(string channel, string cue) {
            entries[channel] = cue;
            CallHooks(channel, cue);
        }

        public string Get(string channel) =>
            entries.GetValueOrDefault(channel);

        public void Clear(string channel) {
            entries.Remove(channel);
            CallHooks(channel, null);
        }

        public IDisposable Hook(string channel, Delegates.CallbackDelegate<string> hook) {
            hooks[channel] = hooks.GetValueOrDefault(channel) + hook;
            return new CueToken(this, channel, hook);
        }

        void Unhook(string channel, Delegates.CallbackDelegate<string> hook) =>
            hooks[channel] -= hook;

        void CallHooks(string channel, string cue) {
            if (hooks.TryGetValue(channel, out Delegates.CallbackDelegate<string> hook)) {
                hook(cue);
            }
        }

        public Cues(TaleMaster master)
        {
            this.master = master;
            entries = new Dictionary<string, string>();
            hooks = new Dictionary<string, Delegates.CallbackDelegate<string>>();
        }
    }
}