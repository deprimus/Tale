namespace TaleUtil
{
    public abstract class Action
    {
        public ulong id;

        public abstract bool Run();
        public abstract Action Clone();

        public virtual void OnInterrupt() { }

        // The callback retrieves the current delta time.
        // By changing the callback, you can decide if you want the action to use
        // normal delta time, or unscaled delta time (or maybe custom scaled delta time).
        // For example, an action that changes Time.timeScale should have the delta callback
        // equal to () => Time.unscaledDeltaTime, such that the duration isn't affected by the time scale.
        //
        // This is used by Tale.Unscaled to tell actions to use unscaled delta time.
        public virtual void SetDeltaCallback(Delegates.DeltaDelegate callback) =>
            delta = callback;

        // Default: use scaled delta time
        protected Delegates.DeltaDelegate delta = () => UnityEngine.Time.deltaTime;
    }
}
