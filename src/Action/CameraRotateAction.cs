using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class CameraRotateAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            TRANSITION
        }

        private Vector3 rotation;
        private float transitionDuration;
        private TaleUtil.Delegates.InterpolationDelegate interpolation;

        private float clock;

        private Vector3 initialRotation;

        private State state;

        private CameraRotateAction() { }

        public CameraRotateAction(Vector3 rotation, float transitionDuration, TaleUtil.Delegates.InterpolationDelegate interpolation)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.camera, "CameraRotateAction requires a main camera object (which could not be found)");

            this.rotation = rotation;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override TaleUtil.Action Clone()
        {
            CameraRotateAction clone = new CameraRotateAction();
            clone.rotation = rotation;
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
                    initialRotation = TaleUtil.Props.camera.transform.eulerAngles;

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    clock += Time.deltaTime;

                    if(clock > transitionDuration)
                        clock = transitionDuration;

                    // TODO: Make a method called TickClock which returns the factor, and does what the 3 lines from above do. Here and everywhere else.
                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    float x;
                    float y;
                    float z;

                    // When dealing with defaults:
                    // Don't use the initial position coord, use the current position coord (it may be changed in another script while this action is running).
                    // By doing this, other scripts can modify the coordinates that this action considers 'default' in parallel.

                    // TODO: Use user-defined interpolation

                    if(rotation.x != float.MinValue)
                        x = TaleUtil.Math.Interpolate(initialRotation.x, rotation.x, interpolationFactor);
                    else x = TaleUtil.Props.camera.transform.eulerAngles.x;

                    if(rotation.y != float.MinValue)
                        y = TaleUtil.Math.Interpolate(initialRotation.y, rotation.y, interpolationFactor);
                    else y = TaleUtil.Props.camera.transform.eulerAngles.y;

                    if(rotation.z != float.MinValue)
                        z = TaleUtil.Math.Interpolate(initialRotation.z, rotation.z, interpolationFactor);
                    else z = TaleUtil.Props.camera.transform.eulerAngles.z;


                    TaleUtil.Props.camera.transform.eulerAngles = new Vector3(x, y, z);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }
    }
}
