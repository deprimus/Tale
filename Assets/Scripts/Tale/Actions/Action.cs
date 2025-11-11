using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TaleUtil {
    /// <summary>
    /// The most important object in Tale. Extend it and create your own custom actions :)
    /// 
    /// <para><b>Examples</b></para>
    /// Basic example: <see cref="TaleUtil.WaitAction">WaitAction</see>
    /// <para/>
    /// Executing subactions: <see cref="TaleUtil.QueueAction">QueueAction</see>, <see cref="TaleUtil.MultiplexAction">MultiplexAction</see>, <see cref="TaleUtil.AnyAction">AnyAction</see>
    /// <para/>
    /// Working with subactions (without executing): <see cref="TaleUtil.ParallelAction">ParallelAction</see>
    /// <para/>
    /// </summary>
    /// 
    /// <remarks>
    /// <para><b>Subactions</b></para>
    /// Whenever you work with other actions inside your custom <c>Action</c>, you must return them in <c>GetSubactions()</c>.
    /// <para/>
    /// If you intend to execute subactions, you must either call <c>Execute()</c> until it returns <c>true</c>, or you must call <c>Interrupt()</c> once.
    /// <para/>
    /// Whenever an action's <c>Execute()</c> returns <c>true</c>, or its <c>Interrupt()</c> is called, the action must not be used in any way afterwards.
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

        /// <summary>
        /// Checks a condition and throws an exception if it's false.
        /// </summary>
        /// <remarks>
        /// Inside <see cref="Run">Run</see>, this should be used instead of Unity's <c>Debug.Assert</c>, so Tale can show you exactly where the action was created when it logs the exception.
        /// </remarks>
        protected void Check(bool condition, string msg) {
            if (!condition) {
                throw new Exception(msg);
            }
        }
        #endregion

        #region Fields
        internal ulong id;
        protected TaleMaster master;
        System.Type type;

        public System.Threading.Tasks.TaskCompletionSource<bool> task;

        protected Delegates.DeltaDelegate delta = () => UnityEngine.Time.deltaTime;

#if UNITY_ASSERTIONS
        StackTrace stack;
#endif

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
                    Debug.Assert.Impossible("Attempted to execute an action that was already finished; Action.Execute() must never be called after it returned true");
                    return true;
                }
            }

            var finished = false;

            try {
                finished = Run();
            } catch (System.Exception e) {
                LogException(e);
            }

            if (finished) {
                ReturnToPool();
            }

            return finished;
        }

        public virtual void Interrupt() {
            if (execState == ExecutionState.FINISHED) {
                Log.Warning("Interrupt() called on a finished action");
                return;
            }

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

#if UNITY_ASSERTIONS
            stack = new StackTrace(1, true);
#endif

            execState = ExecutionState.READY;
        }

        void ReturnToPool() {
            execState = ExecutionState.FINISHED;
            master.ReturnAction(type, this);
        }

        void LogException(System.Exception e) {
            var str = new StringBuilder();
            var exStack = new StackTrace(e, true);

            str.AppendFormat("{0}: {1}\n", e.GetType().Name, e.Message);

            Debug.StackTraceToString(exStack, Debug.FilterInternalTaleFrame, str);

#if UNITY_ASSERTIONS
            str.AppendLine("<internal Tale magic>");

            Debug.StackTraceToString(stack, Debug.FilterInternalTaleFrame, str);

            str.AppendLine("-----\nReal runtime stack:");
#endif

            Log.Error(type.ToString(), str.ToString());
        }

        class Exception : System.Exception {
            public Exception(string msg) : base(msg) { }
        }
    }
}
