using UnityEngine;

namespace TaleUtil
{
    public class CameraZoomAction : Action
    {
        enum State
        {
            SETUP,
            TRANSITION
        }

        float factor;
        float transitionDuration;
        Delegates.InterpolationDelegate interpolation;

        float clock;

        float initialSize;

        State state;

        CameraZoomAction() { }

        public CameraZoomAction(float factor, float transitionDuration, Delegates.InterpolationDelegate interpolation)
        {
            Assert.Condition(Props.camera != null, "CameraZoomAction requires a main camera object (which could not be found)");

            this.factor = factor;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override Action Clone()
        {
            CameraZoomAction clone = new CameraZoomAction();
            clone.delta = delta;
            clone.factor = factor;
            clone.transitionDuration = transitionDuration;
            clone.interpolation = interpolation;
            clone.clock = clock;
            clone.state = state;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                {
                    initialSize = Props.camera.obj.orthographicSize;
                    factor = (1f / factor) * Props.camera.baseOrthographicSize; // 0.5f zoom = (1f / 0.5f) * base size = 2 * base size

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    clock += delta();

                    if(clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    Props.camera.obj.orthographicSize = Math.Interpolate(initialSize, factor, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("CameraZoomAction ({0})", state.ToString());
        }
    }
}
