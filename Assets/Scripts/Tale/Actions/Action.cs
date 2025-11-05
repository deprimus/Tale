using System.Collections.Generic;
using System.Linq;

namespace TaleUtil
{
    public abstract class Action
    {
        public ulong id;
        protected TaleMaster master;

        public System.Threading.Tasks.TaskCompletionSource<bool> task;

        protected Delegates.DeltaDelegate delta = () => UnityEngine.Time.deltaTime;

        public Action() { }

        public abstract bool Run();

        public virtual IEnumerable<Action> GetSubactions() { return Enumerable.Empty<Action>(); }

        public virtual void OnInterrupt() {
            foreach (var action in GetSubactions()) {
                action.OnInterrupt();
            }
        }

        // The callback retrieves the current delta time.
        // By changing the callback, you can decide if you want the action to use
        // normal delta time, or unscaled delta time (or maybe custom scaled delta time).
        // For example, an action that changes Time.timeScale should have the delta callback
        // equal to () => Time.unscaledDeltaTime, such that the duration isn't affected by the time scale.
        //
        // This is used by Tale.Unscaled to tell actions to use unscaled delta time.
        public virtual void SetDeltaCallback(Delegates.DeltaDelegate callback) {
            delta = callback;

            foreach (var action in GetSubactions()) {
                action.SetDeltaCallback(callback);
            }
        }

        internal void Inject(TaleMaster master, ulong id) {
            this.id = id;
            this.master = master;
        }
    }
}
