using UnityEngine;

namespace TaleUtil
{
    public class TransformRotateAction : Action
    {
        enum State
        {
            SETUP,
            TRANSITION
        }

        Props.Transformable transformable;
        Vector3 rotation;
        float transitionDuration;
        Delegates.InterpolationDelegate interpolation;
        bool relative;

        float clock;

        Vector3 initialRotation;

        State state;

        TransformRotateAction() { }

        public TransformRotateAction(Props.Transformable transformable, Vector3 rotation, float transitionDuration, Delegates.InterpolationDelegate interpolation, bool relative)
        {
            this.transformable = transformable;
            this.rotation = rotation;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;
            this.relative = relative;
            
            // Normalize the angles from any number to 0->360
            // If relative, normalize in SETUP
            if (!relative)
            {
                this.rotation.x = Math.NormalizeAngle(this.rotation.x);
                this.rotation.y = Math.NormalizeAngle(this.rotation.y);
                this.rotation.z = Math.NormalizeAngle(this.rotation.z);
            }

            clock = 0f;

            state = State.SETUP;
        }

        public TransformRotateAction(Transform transform, Vector3 rotation, float transitionDuration, Delegates.InterpolationDelegate interpolation, bool relative)
            : this(new Props.Transformable(transform), rotation, transitionDuration, interpolation, relative) { }

        public override Action Clone()
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
                    {
                        rotation = new Vector3(initialRotation.x + rotation.x, initialRotation.y + rotation.y, initialRotation.z + rotation.z);
                        
                        //rotation.x = Math.NormalizeAngle(rotation.x);
                        //rotation.y = Math.NormalizeAngle(rotation.y);
                        //rotation.z = Math.NormalizeAngle(rotation.z);
                    }
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
                        x = Math.Interpolate(initialRotation.x, rotation.x, interpolationFactor);
                    else x = transformable.transform.eulerAngles.x;

                    if(rotation.y != float.MinValue)
                        y = Math.Interpolate(initialRotation.y, rotation.y, interpolationFactor);
                    else y = transformable.transform.eulerAngles.y;

                    if(rotation.z != float.MinValue)
                        z = Math.Interpolate(initialRotation.z, rotation.z, interpolationFactor);
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
