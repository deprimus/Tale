using System.Collections.Generic;

namespace TaleUtil
{
    public class BindAction : Action
    {
        public Action primary;
        public Action secondary;

        bool secondaryDone;

        BindAction() { }

        // How this works:
        //
        // The Tale.* methods add actions to the queue.
        // In order for Tale.Bind(Tale.*, Tale.*) to work, the actions need to be removed from the queue,
        // and to be stored here.
        // This way, the user can write both:
        //
        // Tale.Dialog();
        // Tale.Transition();
        //
        // and
        //
        // Tale.Bind(
        //     Tale.Dialog(),
        //     Tale.Transition()
        // );

        public BindAction(Action primary, Action secondary)
        {
            this.primary = primary;
            this.secondary = secondary;

            this.secondaryDone = false;

            Queue.RemoveLast(secondary);
            Queue.RemoveLast(primary);
        }

        public override void SetDeltaCallback(Delegates.DeltaDelegate callback)
        {
            base.SetDeltaCallback(callback);

            primary.SetDeltaCallback(callback);
            secondary.SetDeltaCallback(callback);
        }

        public override Action Clone()
        {
            BindAction clone = new BindAction();
            clone.delta = delta;
            clone.primary = primary;
            clone.secondary = secondary;
            clone.secondaryDone = false;

            return clone;
        }

        public override bool Run()
        {
            if (primary.Run())
            {
                if (!secondaryDone)
                {
                    secondary.OnInterrupt();
                }
                return true;
            }

            if (!secondaryDone)
            {
                secondaryDone = secondary.Run();
            }

            return false;
        }

        public override string ToString()
        {
            return "BindAction";
        }
    }
}