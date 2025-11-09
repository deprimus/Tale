using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil {
    public class RepeatAction : Action {
        enum State {
            SETUP,
            RUN
        }

        ulong count;
        State state;

        TaleUtil.Action currentAction;
        Delegates.ActionDelegate action;

        public RepeatAction Init(ulong count, Delegates.ActionDelegate action) {
            this.count = count;
            this.action = action;

            state = State.SETUP;

            return this;
        }

        protected override bool Run() {
            switch (state) {
                case State.SETUP: {
                    GetAction();

                    state = State.RUN;
                    return RunAction();
                }
                case State.RUN: {
                    return RunAction();
                }
            }

            return false;
        }

        void GetAction() {
            currentAction = action();
            currentAction.SetDeltaCallback(delta);
        }

        bool RunAction() {
            if (currentAction.Execute()) {
                switch (count) {
                    case 0: {
                        // 0 = repeat forever
                        state = State.SETUP;
                        return false;
                    }
                    case 1: {
                        // 1 = this was the last repetition
                        return true;
                    }
                    default: {
                        --count;
                        state = State.SETUP;
                        return false;
                    }
                }
            }

            return false;
        }

        public override IEnumerable<Action> GetSubactions() {
            if (currentAction == null) {
                yield break;
            }

            yield return currentAction;
        }

        public override string ToString() {
            string left = (count == 0 ? "loop" : string.Format("<color=#{0}>{1}</color> time{2}", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), count.ToString(), (count > 1 ? "s" : "")));

            return string.Format("RepeatAction ({0})", left);
        }
    }
}