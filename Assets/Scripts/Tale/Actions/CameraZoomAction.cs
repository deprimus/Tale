using UnityEngine;

namespace TaleUtil {
    public class CameraZoomAction : Action {
        enum State {
            SETUP,
            TRANSITION
        }

        float factor;
        float transitionDuration;
        Delegates.InterpolationDelegate interpolation;

        float clock;

        float initialSize;

        State state;

        public CameraZoomAction Init(float factor, float transitionDuration, Delegates.InterpolationDelegate interpolation) {
            Debug.Assert.Condition(master.Props.camera != null, "CameraZoomAction requires a main camera object (which could not be found)");

            this.factor = factor;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;

            return this;
        }

        protected override bool Run() {
            switch (state) {
                case State.SETUP: {
                    initialSize = master.Props.camera.obj.orthographicSize;
                    factor = (1f / factor) * master.Props.camera.baseOrthographicSize; // 0.5f zoom = (1f / 0.5f) * base size = 2 * base size

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION: {
                    clock += delta();

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    master.Props.camera.obj.orthographicSize = Math.Interpolate(initialSize, factor, interpolationFactor);

                    if (clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }

        public override string ToString() {
            return string.Format("CameraZoomAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), state.ToString());
        }
    }
}
