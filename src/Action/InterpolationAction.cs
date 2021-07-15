using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class InterpolationAction<T> : TaleUtil.Action
    {
        private enum State
        {
            TRANSITION_FLOAT,
            TRANSITION_VECTOR,
            TRANSITION_COLOR
        }

        private T initial;
        private T target;
        private TaleUtil.Delegates.CallbackDelegate<T> callback;
        private float transitionDuration;
        private TaleUtil.Delegates.InterpolationDelegate interpolation;

        private float clock;

        private State state;

        private InterpolationAction() { }

        public InterpolationAction(T initial, T target, TaleUtil.Delegates.CallbackDelegate<T> callback, float transitionDuration, TaleUtil.Delegates.InterpolationDelegate interpolation)
        {
            if (typeof(T) == typeof(float))
            {
                state = State.TRANSITION_FLOAT;
            }
            else if (typeof(T) == typeof(Vector3))
            {
                state = State.TRANSITION_VECTOR;
            }
            else if (typeof(T) == typeof(Color))
            {
                state = State.TRANSITION_COLOR;
            }
            else
            {
                TaleUtil.Assert.Impossible("<T> must be either float, Vector3 or Color for InterpolationAction");
            }

            this.initial = initial;
            this.target = target;
            this.callback = callback;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;

            clock = 0f;
        }

        public override TaleUtil.Action Clone()
        {
            InterpolationAction<T> clone = new InterpolationAction<T>();
            clone.initial = initial;
            clone.target = target;
            clone.callback = callback;
            clone.transitionDuration = transitionDuration;
            clone.interpolation = interpolation;
            clone.clock = clock;
            clone.state = state;

            return clone;
        }

        private void Tick()
        {
            clock += Time.deltaTime;

            if (clock > transitionDuration)
                clock = transitionDuration;
        }

        public override bool Run()
        {
            Tick();

            float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

            switch (state)
            {
                case State.TRANSITION_FLOAT:
                {
                    callback((T) (object) TaleUtil.Math.Interpolate((float) (object) initial, (float) (object) target, interpolationFactor));
                    break;
                }
                case State.TRANSITION_VECTOR:
                {
                    callback((T)(object)TaleUtil.Math.Interpolate((Vector3) (object) initial, (Vector3) (object) target, interpolationFactor));
                    break;

                }
                case State.TRANSITION_COLOR:
                {
                    callback((T) (object) TaleUtil.Math.Interpolate((Color) (object) initial, (Color) (object) target, interpolationFactor));
                    break;
                }
            }

            return clock == transitionDuration;
        }
    }
}