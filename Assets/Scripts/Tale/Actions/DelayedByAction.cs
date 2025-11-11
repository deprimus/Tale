using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil {
    public class DelayedByAction : Action {
        string trigger;
        Action action;

        enum State {
            WAIT_FOR_TRIGGER,
            RUN,
            END
        };

        State state;

        public DelayedByAction Init(string trigger, Action action) {
            this.trigger = trigger;
            this.action = action;

            state = State.WAIT_FOR_TRIGGER;

            return this;
        }

        protected override bool Run() {
            switch (state) {
                case State.WAIT_FOR_TRIGGER: {
                    if (master.Triggers.Get(trigger)) {
                        state = State.RUN;
                    }
                    break;
                }
                case State.RUN: {
                    bool done = action.Execute();

                    if (done) {
                        state = State.END;
                    }

                    return done;
                }
                case State.END: {
                    TaleUtil.Log.Warning("DelayedByAction.Run() called when action was already done");
                    return true;
                }
            }

            return false;
        }

        public override IEnumerable<Action> GetSubactions() {
            yield return action;
        }

        public override string ToString() =>
            string.Format("DelayedByAction ({0}, {1})", state.ToString(), trigger);
    }
}