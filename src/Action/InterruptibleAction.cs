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

        public override Action Clone()
        {
            InterruptibleAction clone = new InterruptibleAction();
            clone.trigger = trigger;
            clone.action = action;

            return clone;
        }

        public override bool Run()
        {
            return (Triggers.Get(trigger) || action.Run());
        }
    }
}