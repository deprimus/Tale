namespace TaleUtil
{
    public class ParallelAction : Action
    {
        Action[] actions;

        public ParallelAction Init(Action[] actions)
        {
            this.actions = actions;

            return this;
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);

            // Change the delta callback for all children
            for (int i = 0; i < actions.Length; ++i)
            {
                actions[i].SetDeltaCallback(callback);
            }
        }

        public override bool Run()
        {
            master.Parallel.InsertMany(actions);
            return true;
        }

        public override string ToString()
        {
            return "ParallelAction";
        }
    }
}