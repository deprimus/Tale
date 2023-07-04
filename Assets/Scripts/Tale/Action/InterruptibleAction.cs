namespace TaleUtil
{
    public class InterruptibleAction : Action
    {
        string trigger;
        Action action;

        InterruptibleAction() { }

        public InterruptibleAction(string trigger, Action action)
        {
            this.trigger = trigger;
            this.action = action;

            // Remove the action from the Tale queue because it will be handled here
            Queue.RemoveLast(action);
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);
            action.SetDeltaCallback(callback);
        }

        public override Action Clone()
        {
            InterruptibleAction clone = new InterruptibleAction();
            clone.delta = delta;
            clone.trigger = trigger;
            clone.action = action;

            return clone;
        }

        public override bool Run()
        {
            if (Triggers.Get(trigger))
            {
                action.OnInterrupt();
                return true;
            }

            return action.Run();
        }

        public override string ToString()
        {
            return string.Format("Interruptible action ({0})", action.ToString());
        }
    }
}