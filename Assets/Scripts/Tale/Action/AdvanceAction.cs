using UnityEngine;

namespace TaleUtil
{
    public class AdvanceAction : Action
    {
        enum State
        {
            SETUP,
            WAIT_FOR_INPUT
        }

        State state;

        public AdvanceAction()
        {
            Assert.Condition(Props.advanceCanvas != null, string.Format("Advance Canvas is null; did you forget to register it in TaleMaster?"));

            state = State.SETUP;
        }

        public override Action Clone()
        {
            AdvanceAction clone = new AdvanceAction();
            clone.delta = delta;
            clone.state = state;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                {
                    if(!Props.advanceCanvas.activeSelf)
                        Props.advanceCanvas.SetActive(true);

                    state = State.WAIT_FOR_INPUT;

                    break;
                }
                case State.WAIT_FOR_INPUT:
                {
                    if (Input.Advance())
                    {
                        Props.advanceCanvas.SetActive(false);
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("AdvanceAction ({0})", state.ToString());
        }
    }
}
