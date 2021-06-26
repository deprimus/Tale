using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class CameraEffectAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            TRANSITION_IN,
            TRANSITION_OUT
        }

        private float transitionDuration;
        private Texture lut;
        private TaleUtil.Delegates.InterpolationDelegate interpolation;

        private float clock;
        private float initialContribution;
        private State state;

        private CameraEffectAction() { }

        public CameraEffectAction(string effect, float transitionDuration, TaleUtil.Delegates.InterpolationDelegate interpolation)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.postProcessing.colorGrading, "CameraEffectAction requires a color grading object (and, therefore, a PostProcessVolume component on the main camera)");

            if (effect != null)
            {
                effect = effect.ToLower();
                TaleUtil.Assert.Condition(TaleUtil.Props.cameraEffects.ContainsKey(effect), string.Format("Unregistered camera effect '{0}'", effect));

                lut = TaleUtil.Props.cameraEffects[effect];
            }
            else
            {
                lut = null;
            }

            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override TaleUtil.Action Clone()
        {
            CameraEffectAction clone = new CameraEffectAction();
            clone.transitionDuration = transitionDuration;
            clone.lut = lut;
            clone.interpolation = interpolation;
            clone.clock = clock;
            clone.initialContribution = initialContribution;
            clone.state = state;

            return clone;
        }

        public override bool Run()
        {
            switch(state)
            {
                case State.SETUP:
                {
                    //TaleUtil.Props.postProcessing.colorGrading.active = true;
                    TaleUtil.Props.postProcessing.colorGrading.ldrLut.overrideState = true;
                    TaleUtil.Props.postProcessing.colorGrading.ldrLutContribution.overrideState = true;

                    if(lut != null)
                    {
                        TaleUtil.Props.postProcessing.colorGrading.ldrLut.value = lut;
                        TaleUtil.Props.postProcessing.colorGrading.ldrLutContribution.value = 0f;

                        state = State.TRANSITION_IN;
                    }
                    else
                    {
                        initialContribution = TaleUtil.Props.postProcessing.colorGrading.ldrLutContribution.value;
                        state = State.TRANSITION_OUT;
                    }

                    break;
                }
                case State.TRANSITION_IN:
                {
                    clock += Time.deltaTime;

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    TaleUtil.Props.postProcessing.colorGrading.ldrLutContribution.value = TaleUtil.Math.Interpolate(0f, 1f, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
                case State.TRANSITION_OUT:
                {
                    clock += Time.deltaTime;

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    TaleUtil.Props.postProcessing.colorGrading.ldrLutContribution.value = TaleUtil.Math.Interpolate(initialContribution, 0f, interpolationFactor);

                    if(clock == transitionDuration)
                    {
                        TaleUtil.Props.postProcessing.colorGrading.ldrLut.value = null;
                        TaleUtil.Props.postProcessing.colorGrading.ldrLut.overrideState = false;
                        //TaleUtil.Props.postProcessing.colorGrading.active = false;

                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}