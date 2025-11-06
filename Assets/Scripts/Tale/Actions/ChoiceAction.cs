using UnityEngine;

namespace TaleUtil {
    public class ChoiceAction<TArgs, TChoice> : Action {
        enum State {
            SETUP,
            WAIT_FOR_CHOICE,
            END
        }

        string style;
        TArgs args;
        TChoice[] choices;
        State state;

        public ChoiceAction<TArgs, TChoice> Init(string style, TArgs args, TChoice[] choices) {
            SoftAssert.Condition(master.Props.choice.styles.ContainsKey(style.ToLowerInvariant()),
                    string.Format("Unknown choice style '{0}'; did you forget to register it in TaleMaster?", style));

            this.style = style;
            this.args = args;
            this.choices = choices;

            state = State.SETUP;

            return this;
        }

        protected override bool Run() {
            switch (state) {
                case State.SETUP: {
                    var obj = master.Props.choice.styles[style];

                    // TaleUtil.Props.Choice.Reset() disables all styles since it doesn't have access to the ChoiceMaster types,
                    // so activate the object if it was previously reset.
                    if (!obj.activeSelf) {
                        obj.SetActive(true);
                    }

                    var picker = obj.GetComponent<Scripts.Choice.ChoiceMaster<TArgs, TChoice>>();
                    var canvas = obj.GetComponent<Canvas>();

                    if (picker == null) {
                        Log.Error("CHOICE", string.Format("No ChoiceMaster script attached to object for choice style '{0}'; make sure to add exactly one ChoiceMaster component to the root object", style));
                    }

                    state = State.WAIT_FOR_CHOICE;

                    picker.enabled = true;

                    if (canvas != null) {
                        canvas.enabled = true;
                    }

                    picker.Present(args, choices, () => {
                        state = State.END;

                        picker.enabled = false;

                        if (canvas != null) {
                            canvas.enabled = false;
                        }
                    });

                    return false;
                }
                case State.WAIT_FOR_CHOICE: {
                    return false;
                }
                case State.END: {
                    return true;
                }
            }

            return true;
        }
        public override string ToString() =>
            string.Format("ChoiceAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Core.DEBUG_ACCENT_COLOR_SECONDARY), style);
    }
}