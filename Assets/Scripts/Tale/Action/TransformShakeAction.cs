using UnityEngine;

namespace TaleUtil
{
    public class TransformShakeAction : Action
    {
        enum State
        {
            SETUP,
            TRANSITION
        }

        Props.Transformable transformable;
        Vector2 magnitude;
        float transitionDuration;
        Delegates.InterpolationDelegate interpolation;

        float clock;

        Vector3 initialPos;
        bool isRectTransform;

        State state;

        TransformShakeAction() { }

        // For transforms which can have their references changed (e.g. the camera when switching scenes).
        public TransformShakeAction(Props.Transformable transformable, Vector2 magnitude, float transitionDuration, Delegates.InterpolationDelegate interpolation)
        {
            this.transformable = transformable;
            this.magnitude = magnitude;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public TransformShakeAction(Transform transform, Vector2 magnitude, float transitionDuration, Delegates.InterpolationDelegate interpolation)
            : this(new Props.Transformable(transform), magnitude, transitionDuration, interpolation) { }

        public override Action Clone()
        {
            TransformShakeAction clone = new TransformShakeAction();
            clone.delta = delta;
            clone.transformable = transformable;
            clone.magnitude = magnitude;
            clone.transitionDuration = transitionDuration;
            clone.interpolation = interpolation;
            clone.clock = clock;
            clone.state = state;
            clone.initialPos = initialPos;
            clone.isRectTransform = isRectTransform;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                    {
                        if (transformable.transform is RectTransform)
                        {
                            initialPos = ((RectTransform)transformable.transform).anchoredPosition;
                            isRectTransform = true;
                        }
                        else
                        {
                            initialPos = transformable.transform.position;
                            isRectTransform = false;
                        }

                        state = State.TRANSITION;

                        break;
                    }
                case State.TRANSITION:
                    {
                        clock += delta();

                        if (clock > transitionDuration)
                            clock = transitionDuration;

                        float x = initialPos.x;
                        float y = initialPos.y;

                        float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                        // Since the interpolation function goes like 0 -> 1, and we need 0 -> n -> 0, try to mirror the function.
                        // Note that this works with the interpolation functions provided by Tale, but may not work properly with custom functions
                        if (clock / transitionDuration > 0.5f)
                        {
                            interpolationFactor = 1f - interpolationFactor;
                        }

                        // Since n is usually 0.5 in interpolation functions, the result is 0 -> 0.5 -> 0, so bring it back up to 0 -> 1 -> 0
                        // (it's usually 0.5 because at the time 0.5, the value is 0.5, regardless if it's linear or with ease in/out)
                        interpolationFactor *= 2f;

                        // If it's the end of the action, move the transform back to the initial position
                        // (if there are defaults, don't touch those)
                        // Otherwise, just shake randomly.
                        if (clock < transitionDuration)
                        {
                            x = initialPos.x + Random.Range(-magnitude.x * interpolationFactor, magnitude.x * interpolationFactor);
                            y = initialPos.y + Random.Range(-magnitude.y * interpolationFactor, magnitude.y * interpolationFactor);
                        }

                        // When dealing with defaults:
                        // Don't use the initial position coord, use the current position coord (it may be changed in another script while this action is running).
                        // By doing this, other scripts can modify the coordinates that this action considers 'default' in parallel.

                        if (isRectTransform)
                        {
                            if (magnitude.x == float.MinValue)
                                x = ((RectTransform)transformable.transform).anchoredPosition.x;

                            if (magnitude.y == float.MinValue)
                                y = ((RectTransform)transformable.transform).anchoredPosition.y;

                            ((RectTransform)transformable.transform).anchoredPosition = new Vector3(x, y, transformable.transform.position.z);
                        }
                        else
                        {
                            if (magnitude.x == float.MinValue)
                                x = transformable.transform.position.x;

                            if (magnitude.y == float.MinValue)
                                y = transformable.transform.position.y;

                            transformable.transform.position = new Vector3(x, y, transformable.transform.position.z);
                        }

                        if (clock == transitionDuration)
                        {
                            return true;
                        }

                        break;
                    }
            }

            return false;
        }

        public override void OnInterrupt()
        {
            if (state == State.SETUP)
            {
                // Initialize member fields
                state = State.SETUP;
                Run();
            }

            // Move  transform back to its initial position
            clock = transitionDuration;
            Run();
        }

        public override string ToString()
        {
            return string.Format("TransformShakeAction ({0})", state.ToString());
        }
    }
}
