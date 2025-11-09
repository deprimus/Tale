using UnityEngine;

namespace TaleUtil {
    public class WaitAction : Action {
        float amount;
        float clock;

        public WaitAction Init(float amount) {
            this.amount = amount;
            clock = 0f;

            return this;
        }

        protected override bool Run() {
            clock += delta();

            return (clock >= amount);
        }

        public override string ToString() {
            return string.Format("WaitAction (<color=#{0}>{1}</color> left)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), Mathf.Max(0f, amount - clock).ToString("0.0"));
        }
    }
}