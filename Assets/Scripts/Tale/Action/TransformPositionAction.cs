using UnityEngine;

namespace TaleUtil
{
    public class TransformPositionAction : Action
    {
        enum State
        {
            SETUP,
            TRANSITION
        }

        Props.Transformable transformable;
        Vector2 pos;
        float transitionDuration;
        Delegates.InterpolationDelegate interpolation;
        bool relative;

        float clock;

        Vector3 initialPos;
        bool isRectTransform;

        State state;

        TransformPositionAction() { }

        // For transforms which can have their references changed (e.g. the camera when switching scenes).
        public TransformPositionAction(Props.Transformable transformable, Vector2 pos, float transitionDuration, Delegates.InterpolationDelegate interpolation, bool relative)
        {
            this.transformable = transformable;
            this.pos = pos;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;
            this.relative = relative;

            clock = 0f;

            state = State.SETUP;
        }

        public TransformPositionAction(Transform transform, Vector2 pos, float transitionDuration, Delegates.InterpolationDelegate interpolation, bool relative)
            : this(new Props.Transformable(transform), pos, transitionDuration, interpolation, relative) { }

        public override Action Clone()
        {
            TransformPositionAction clone = new TransformPositionAction();
            clone.delta = delta;
            clone.transformable = transformable;
            clone.pos = pos;
            clone.transitionDuration = transitionDuration;
            clone.interpolation = interpolation;
            clone.relative = relative;
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
                    if(transformable.transform is RectTransform)
                    {
                        initialPos = ((RectTransform) transformable.transform).anchoredPosition;
                        isRectTransform = true;
                    }
                    else
                    {
                        initialPos = transformable.transform.position;
                        isRectTransform = false;
                    }

                    if(relative)
                        pos = new Vector2(initialPos.x + pos.x, initialPos.y + pos.y);

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    clock += delta();

                    if(clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    float x;
                    float y;

                    // When dealing with defaults:
                    // Don't use the initial position coord, use the current position coord (it may be changed in another script while this action is running).
                    // By doing this, other scripts can modify the coordinates that this action considers 'default' in parallel.

                    if(isRectTransform)
                    {
                        if (pos.x != float.MinValue)
                            x = Math.Interpolate(initialPos.x, pos.x, interpolationFactor);
                        else x = ((RectTransform)transformable.transform).anchoredPosition.x;

                        if (pos.y != float.MinValue)
                            y = Math.Interpolate(initialPos.y, pos.y, interpolationFactor);
                        else y = ((RectTransform)transformable.transform).anchoredPosition.y;

                        ((RectTransform)transformable.transform).anchoredPosition = new Vector3(x, y, transformable.transform.position.z);
                    }
                    else
                    {
                        if (pos.x != float.MinValue)
                            x = Math.Interpolate(initialPos.x, pos.x, interpolationFactor);
                        else x = transformable.transform.position.x;

                        if (pos.y != float.MinValue)
                            y = Math.Interpolate(initialPos.y, pos.y, interpolationFactor);
                        else y = transformable.transform.position.y;

                        transformable.transform.position = new Vector3(x, y, transformable.transform.position.z);
                    }

                    if(clock == transitionDuration)
                        return true;

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
                Run();
            }

            // Move the transform to its final position
            clock = transitionDuration;
            Run();
        }

        public override string ToString()
        {
            return string.Format("TransformPositionAction ({0})", state.ToString());
        }
    }
}
