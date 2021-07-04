using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class TransformRotateAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            TRANSITION
        }

        private TaleUtil.Props.Transformable transformable;
        private Vector3 rotation;
        private float transitionDuration;
        private TaleUtil.Delegates.InterpolationDelegate interpolation;
        private bool relative;

        private float clock;

        private Vector3 initialRotation;

        private State state;

        private TransformRotateAction() { }

        public TransformRotateAction(TaleUtil.Props.Transformable transformable, Vector3 rotation, float transitionDuration, TaleUtil.Delegates.InterpolationDelegate interpolation, bool relative)
        {
            this.transformable = transformable;
            this.rotation = rotation;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;
            this.relative = relative;

            clock = 0f;

            state = State.SETUP;
        }

        public TransformRotateAction(Transform transform, Vector3 rotation, float transitionDuration, TaleUtil.Delegates.InterpolationDelegate interpolation, bool relative)
            : this(new TaleUtil.Props.Transformable(transform), rotation, transitionDuration, interpolation, relative) { }

        public override TaleUtil.Action Clone()
        {
            TransformRotateAction clone = new TransformRotateAction();
            clone.transformable = transformable;
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
                    initialRotation = transformable.transform.eulerAngles;

                    state = State.TRANSITION;

                    if(relative)
                        rotation = new Vector3(initialRotation.x + rotation.x, initialRotation.y + rotation.y, initialRotation.z + rotation.z);

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
                    else x = transformable.transform.eulerAngles.x;

                    if(rotation.y != float.MinValue)
                        y = TaleUtil.Math.Interpolate(initialRotation.y, rotation.y, interpolationFactor);
                    else y = transformable.transform.eulerAngles.y;

                    if(rotation.z != float.MinValue)
                        z = TaleUtil.Math.Interpolate(initialRotation.z, rotation.z, interpolationFactor);
                    else z = transformable.transform.eulerAngles.z;


                    transformable.transform.eulerAngles = new Vector3(x, y, z);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }
    }
}
