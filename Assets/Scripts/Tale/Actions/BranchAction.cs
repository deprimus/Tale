using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil {
    public class BranchAction : Action {
        enum State {
            SETUP,
            RUN,
            END
        };

        string flag;
        Delegates.BranchDelegate<ulong> action;

        State state;
        Action returned;

        public BranchAction Init(string flag, Delegates.BranchDelegate<ulong> action) {
            this.flag = flag;
            this.action = action;

            state = State.SETUP;

            return this;
        }

        protected override bool Run() {
            switch (state) {
                case State.SETUP: {
                    returned = action(master.Flags.Get(flag));

                    if (returned == null) {
                        state = State.END;
                        return true;
                    }

                    returned.SetDeltaCallback(delta);

                    state = State.RUN;

                    return returned.Execute();
                }
                case State.RUN: {
                    return returned.Execute();
                }
                case State.END: {
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
            string.Format("BranchAction (<color=#{0}>{1}</color>, <color=#{2}>{3}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_SECONDARY), flag, ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), state.ToString());
    }
}