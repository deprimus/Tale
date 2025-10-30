using UnityEngine;

namespace TaleUtil
{
    public class ChoiceAction<TArgs, TChoice> : Action
    {
        enum State
        {
            SETUP,
            WAIT_FOR_CHOICE,
            END
        }

        string style;
        TArgs args;
        TChoice[] choices;
        State state;

        public ChoiceAction<TArgs, TChoice> Init(string style, TArgs args, TChoice[] choices)
        {
            SoftAssert.Condition(master.Props.choice.styles.ContainsKey(style.ToLowerInvariant()),
                    string.Format("Unknown choice style '{0}'; did you forget to register it in TaleMaster?", style));

            this.style = style;
            this.args = args;
            this.choices = choices;

            state = State.SETUP;

            return this;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                {
                    var obj = master.Props.choice.styles[style];

                    var picker = obj.GetComponent<Scripts.Choice.ChoiceMaster<TArgs, TChoice>>();
                    var canvas = obj.GetComponent<Canvas>();

                    if (picker == null)
                    {
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
                case State.WAIT_FOR_CHOICE:
                {
                    return false;
                }
                case State.END:
                {
                    return true;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return string.Format("ChoiceAction ({0})", style);
        }
    }
}