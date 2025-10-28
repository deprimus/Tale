using System.Collections.Generic;

namespace TaleUtil
{
    public class AnyAction : Action
    {
        public Action[] actions;

        public AnyAction Init(Action[] actions)
        {
            this.actions = actions;

            return this;
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);

            for (int i = 0; i < actions.Length; i++) {
                actions[i].SetDeltaCallback(callback);
            }
        }

        public override bool Run()
        {
            for (int i = 0; i < actions.Length; i++) {
                if (actions[i].Run()) {
                    // Finish when any action is done
                    return true;
                }
            }

            return false;
        }

        public override void OnInterrupt()
        {
            for (int i = 0; i < actions.Length; i++) {
                actions[i].OnInterrupt();
            }
        }

        public override string ToString()
        {
            return "AnyAction";
        }
    }
}