namespace TaleUtil {
    public class MultiplexAction : Action
    {
        public Collections.FastUnorderedList<Action> actions;

        public MultiplexAction Init(Action[] actions)
        {
            if (this.actions == null) {
                this.actions = new Collections.FastUnorderedList<Action>(actions.Length);
            }

            this.actions.InsertMany(actions);

            return this;
        }

        public override bool Run()
        {
            for (int i = 0; i < actions.Count;) {
                if (actions[i].Run()) {
                    actions.Remove(i);
                } else {
                    ++i;
                }
            }

            return actions.Count == 0; // Finish when all actions are done.
        }

        public override void OnInterrupt()
        {
            while (actions.Count > 0) {
                actions[0].OnInterrupt();
                actions.Remove(0);
            }
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback) {
            base.SetDeltaCallback(callback);

            for (int i = 0; i < actions.Count; ++i) {
                actions[i].SetDeltaCallback(callback);
            }
        }

        public override string ToString()
        {
            return "MultiplexAction";
        }
    }
}