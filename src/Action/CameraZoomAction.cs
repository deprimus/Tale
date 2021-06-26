using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class CameraZoomAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            TRANSITION
        }

        private float factor;
        private float transitionDuration;
        private TaleUtil.Delegates.InterpolationDelegate interpolation;

        private float clock;

        private float initialSize;

        private State state;

        private CameraZoomAction() { }

        public CameraZoomAction(float factor, float transitionDuration, TaleUtil.Delegates.InterpolationDelegate interpolation)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.camera, "CameraZoomAction requires a main camera object (which could not be found)");

            this.factor = factor;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override TaleUtil.Action Clone()
        {
            CameraZoomAction clone = new CameraZoomAction();
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
                    initialSize = TaleUtil.Props.camera.obj.orthographicSize;
                    factor = (1f / factor) * TaleUtil.Props.camera.baseOrthographicSize; // 0.5f zoom = (1f / 0.5f) * base size = 2 * base size

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    clock += Time.deltaTime;

                    if(clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    TaleUtil.Props.camera.obj.orthographicSize = TaleUtil.Math.Interpolate(initialSize, factor, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }
    }
}
