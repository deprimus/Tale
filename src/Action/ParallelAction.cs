namespace TaleUtil
{
    public class ParallelAction : Action
    {
        Action[] actions;

        public ParallelAction() { }

        public ParallelAction(Action[] actions)
        {
            this.actions = actions;

            for(int i = actions.Length - 1; i >= 0; --i)
            {
                Action queueAction = Queue.FetchLast();

                if(queueAction != null && this.actions[i] == queueAction)
                {
                    Queue.Remove(queueAction); // Remove the action from the queue, because it will be added to the parallel list.
                }
            }
        }

        public override Action Clone()
        {
            ParallelAction clone = new ParallelAction();
            clone.actions = new Action[actions.Length];

            for(int i = 0; i < actions.Length; ++i)
                clone.actions[i] = actions[i].Clone();

            return clone;
        }

        public override bool Run()
        {
            Parallel.Add(actions);
            return true;
        }
    }
}