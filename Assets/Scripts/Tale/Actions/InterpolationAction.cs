using UnityEngine;

namespace TaleUtil {
    public class InterpolationAction<T> : Action {
        enum State {
            TRANSITION_FLOAT,
            TRANSITION_VECTOR,
            TRANSITION_COLOR
        }

        T initial;
        T target;
        Delegates.CallbackDelegate<T> callback;
        float transitionDuration;
        Delegates.InterpolationDelegate interpolation;

        float clock;

        State state;

        public InterpolationAction<T> Init(T initial, T target, Delegates.CallbackDelegate<T> callback, float transitionDuration, Delegates.InterpolationDelegate interpolation) {
            if (typeof(T) == typeof(float)) {
                state = State.TRANSITION_FLOAT;
            } else if (typeof(T) == typeof(Vector3)) {
                state = State.TRANSITION_VECTOR;
            } else if (typeof(T) == typeof(Color)) {
                state = State.TRANSITION_COLOR;
            } else {
                Assert.Impossible("<T> must be either float, Vector3 or Color for InterpolationAction");
            }

            this.initial = initial;
            this.target = target;
            this.callback = callback;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;

            clock = 0f;

            return this;
        }

        void Tick() {
            clock += delta();

            if (clock > transitionDuration)
                clock = transitionDuration;
        }

        protected override bool Run() {
            Tick();

            float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

            switch (state) {
                case State.TRANSITION_FLOAT: {
                    callback((T)(object)Math.Interpolate((float)(object)initial, (float)(object)target, interpolationFactor));
                    break;
                }
                case State.TRANSITION_VECTOR: {
                    callback((T)(object)Math.Interpolate((Vector3)(object)initial, (Vector3)(object)target, interpolationFactor));
                    break;

                }
                case State.TRANSITION_COLOR: {
                    callback((T)(object)Math.Interpolate((Color)(object)initial, (Color)(object)target, interpolationFactor));
                    break;
                }
            }

            return clock == transitionDuration;
        }

        protected override void OnInterrupt() {
            clock = transitionDuration;
            Run();
        }

        public override string ToString() =>
            string.Format("InterpolationAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), state.ToString());
    }
}