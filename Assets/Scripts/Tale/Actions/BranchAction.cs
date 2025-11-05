using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class BranchAction : Action
    {
        enum State
        {
            SETUP,
            RUN,
            END
        };

        string flag;
        Delegates.BranchDelegate<ulong> action;

        State state;
        TaleUtil.Action returned;

        public BranchAction Init(string flag, Delegates.BranchDelegate<ulong> action)
        {
            this.flag = flag;
            this.action = action;

            state = State.SETUP;

            return this;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                {
                    returned = action(master.Flags.Get(flag));

                    if (returned == null)
                    {
                        state = State.END;
                        return true;
                    }

                    returned.SetDeltaCallback(delta);

                    state = State.RUN;

                    return returned.Run();
                }
                case State.RUN:
                {
                    return returned.Run();
                }
                case State.END:
                {
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<Action> GetSubactions() {
            if (returned == null) {
                yield break;
            }

            yield return returned;
        }

        public override string ToString() =>
            string.Format("BranchAction (<color=#{0}>{1}</color>, <color=#{2}>{3}</color>)", ColorUtility.ToHtmlStringRGB(master.Config.Core.DEBUG_ACCENT_COLOR_SECONDARY), flag, ColorUtility.ToHtmlStringRGB(master.Config.Core.DEBUG_ACCENT_COLOR_PRIMARY), state.ToString());
    }
}