using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class CameraPositionAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            TRANSITION
        }

        private Vector2 pos;
        private float transitionDuration;
        private TaleUtil.Delegates.InterpolationDelegate interpolation;

        private float clock;

        private Vector3 initialPos;

        private State state;

        private CameraPositionAction() { }

        public CameraPositionAction(Vector2 pos, float transitionDuration, TaleUtil.Delegates.InterpolationDelegate interpolation)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.camera, "CameraPositionAction requires a main camera object (which could not be found)");

            this.pos = pos;
            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override TaleUtil.Action Clone()
        {
            CameraPositionAction clone = new CameraPositionAction();
            clone.pos = pos;
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
                    initialPos = TaleUtil.Props.camera.transform.position;

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    clock += Time.deltaTime;

                    if(clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    float x;
                    float y;

                    // When dealing with defaults:
                    // Don't use the initial position coord, use the current position coord (it may be changed in another script while this action is running).
                    // By doing this, other scripts can modify the coordinates that this action considers 'default' in parallel.

                    if(pos.x != float.MinValue)
                        x = TaleUtil.Math.Interpolate(initialPos.x, pos.x, interpolationFactor);
                    else x = TaleUtil.Props.camera.transform.position.x;

                    if(pos.y != float.MinValue)
                        y = TaleUtil.Math.Interpolate(initialPos.y, pos.y, interpolationFactor);
                    else y = TaleUtil.Props.camera.transform.position.y;

                    TaleUtil.Props.camera.transform.position = new Vector3(x, y, TaleUtil.Props.camera.transform.position.z);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }
    }
}
