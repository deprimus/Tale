using UnityEngine;

namespace TaleUtil
{
    public class ChoiceAction : Action
    {
        enum State
        {
            SETUP,
            WAIT_FOR_CHOICE,
            END
        }

        string style;
        object args;
        object choices;
        State state;

        ChoiceAction() { }

        public ChoiceAction(string style, object args, object choices)
        {
            SoftAssert.Condition(Props.choice.styles.entries.ContainsKey(style),
                    string.Format("Unknown choice style '{0}'; did you forget to register it in TaleMaster?", style));

            this.style = style;
            this.args = args;
            this.choices = choices;

            state = State.SETUP;
        }

        public override Action Clone()
        {
            ChoiceAction clone = new ChoiceAction();
            clone.delta = delta;
            clone.args = args;       // TODO: these are passed by reference; clone them properly
            clone.choices = choices;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                {
                    var prefab = TaleUtil.Props.choice.styles.entries[style];

                    var obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, TaleUtil.Props.choice.container);
                    var master = obj.GetComponent<TaleUtil.Scripts.ChoiceMaster>();

                    if (master == null)
                    {
                        Log.Error("CHOICE", string.Format("No ChoiceMaster script attached to prefab for choice style '{0}'; make sure to add exactly one ChoiceMaster component to the root object of the prefab", style));
                    }

                    state = State.WAIT_FOR_CHOICE;

                    master.Construct(args, choices, () => {
                        GameObject.Destroy(obj);
                        state = State.END;
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