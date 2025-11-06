using System.Collections.Generic;
using System.Linq;

namespace TaleUtil {
    /// <summary>
    /// The most important object in Tale :)
    /// </summary>
    /// <remarks>
    /// Whenever you work directly with an action, you must either call <c>Execute()</c> until it returns <c>true</c>, or you must call <c>Interrupt()</c> once.
    /// <para/>
    /// The action must not be used after that.
    /// </remarks>
    public abstract class Action {
        #region API
        /// <remarks>
        /// This function runs once every frame until the action is done.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if the action is done, <c>false</c> otherwise.
        /// </returns>
        protected abstract bool Run();

        /// <summary>
        /// Called when the action should immediately complete.
        /// </summary>
        protected virtual void OnInterrupt() { }

        /// <returns>
        /// All child actions used by this action.
        /// </returns>
        public virtual IEnumerable<Action> GetSubactions() { return Enumerable.Empty<Action>(); }

        /// <remarks>
        /// Supports TextMeshPro formatting.
        /// </remarks>
        /// <returns>
        /// A human-readable string containing debug information about the action.
        /// </returns>
        public override string ToString() => "Action";
        #endregion

        #region Fields
        internal ulong id;
        protected TaleMaster master;
        System.Type type;

        public System.Threading.Tasks.TaskCompletionSource<bool> task;

        protected Delegates.DeltaDelegate delta = () => UnityEngine.Time.deltaTime;

        enum ExecutionState {
            READY,
            RUNNING,
            FINISHED
        }

        ExecutionState execState;
        #endregion

        #region Public Stuff
        public bool IsRunning { get { return execState == ExecutionState.RUNNING; } }

        public Action() {
            type = GetType();
            execState = ExecutionState.FINISHED;
        }

        /// <remarks>Once this returns <c>true</c>, it must not be called again and the action must not be used from that point on, since it will return to the internal action pool.</remarks>
        /// <returns><c>true</c> if the action is done, <c>false</c> otherwise.</returns>
        public bool Execute() {
            switch (execState) {
                case ExecutionState.READY: {
                    execState = ExecutionState.RUNNING;
                    break;
                }
                case ExecutionState.FINISHED: {
                    Assert.Impossible("Attempted to execute an action that was already finished; Action.Execute() must never be called after it returned true");
                    return true;
                }
            }

            var finished = Run();

            if (finished) {
                ReturnToPool();
            }

            return finished;
        }

        public virtual void Interrupt() {
            SoftAssert.Condition(execState != ExecutionState.FINISHED, "Interrupt() called on a finished action");

            OnInterrupt();

            foreach (var action in GetSubactions()) {
                action.Interrupt();
            }

            ReturnToPool();
        }

        // The callback retrieves the current delta time.
        // By changing the callback, you can decide if you want the action to use
        // normal delta time, or unscaled delta time (or maybe custom scaled delta time).
        // For example, an action that changes Time.timeScale should have the delta callback
        // equal to () => Time.unscaledDeltaTime, such that the duration isn't affected by the time scale.
        //
        // This is used by Tale.Unscaled to tell actions to use unscaled delta time.
        public void SetDeltaCallback(Delegates.DeltaDelegate callback) {
            delta = callback;

            foreach (var action in GetSubactions()) {
                action.SetDeltaCallback(callback);
            }
        }
        #endregion

        internal void Inject(TaleMaster master, ulong id) {
            this.id = id;
            this.master = master;

            execState = ExecutionState.READY;
        }

        void ReturnToPool() {
            execState = ExecutionState.FINISHED;
            master.ReturnAction(type, this);
        }
    }
}
