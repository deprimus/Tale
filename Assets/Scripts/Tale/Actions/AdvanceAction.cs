using UnityEngine;

namespace TaleUtil {
    public class AdvanceAction : Action {
        enum State {
            SETUP,
            WAIT_FOR_INPUT
        }

        State state;

        public AdvanceAction Init() {
            Assert.Condition(master.Props.advanceCanvas != null, string.Format("Advance Canvas is null; did you forget to register it in TaleMaster?"));

            state = State.SETUP;

            return this;
        }

        public override bool Run() {
            switch (state) {
                case State.SETUP: {
                    if (!master.Props.advanceCanvas.activeSelf)
                        master.Props.advanceCanvas.SetActive(true);

                    state = State.WAIT_FOR_INPUT;

                    break;
                }
                case State.WAIT_FOR_INPUT: {
                    if (master.Input.Advance()) {
                        master.Props.advanceCanvas.SetActive(false);
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public override string ToString() =>
            string.Format("AdvanceAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGB(master.config.Core.DEBUG_ACCENT_COLOR_PRIMARY), state.ToString());
    }
}
