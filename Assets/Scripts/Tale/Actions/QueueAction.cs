using System.Collections.Generic;

namespace TaleUtil
{
    public class QueueAction : Action
    {
        Action[] actions;
        int index;

        public QueueAction Init(Action[] actions)
        {
            this.actions = actions;
            index = 0;

            return this;
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);

            for (int i = index; i < actions.Length; i++) {
                actions[i].SetDeltaCallback(callback);
            }
        }

        public override bool Run()
        {
            if (index < actions.Length) {
                if (actions[index].Run()) {
                    ++index;
                }
            }

            return index == actions.Length;
        }

        public override void OnInterrupt()
        {
            for (int i = index; i < actions.Length; i++) {
                actions[i].OnInterrupt();
            }

            index = actions.Length;
        }

        public override string ToString()
        {
            return "QueueAction";
        }
    }
}