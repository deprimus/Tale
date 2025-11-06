using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil {
    public class DelayedAction : Action {
        float amount;
        Action action;

        float clock;

        public DelayedAction Init(float amount, Action action) {
            this.amount = amount;
            this.action = action;

            clock = 0f;

            return this;
        }

        protected override bool Run() {
            if (clock >= amount) {
                return action.Execute();
            } else {
                clock += delta();

                return false;
            }
        }

        public override IEnumerable<Action> GetSubactions() {
            yield return action;
        }

        public override string ToString() =>
            string.Format("DelayedAction (<color=#{0}>{1}</color> left)", ColorUtility.ToHtmlStringRGBA(master.config.Core.DEBUG_ACCENT_COLOR_PRIMARY), Mathf.Max(0f, amount - clock).ToString("0.0"));
    }
}