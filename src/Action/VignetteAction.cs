using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class VignetteAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            TRANSITION
        }

        private float transitionDuration;
        private float intensity;
        private Color? color;
        private float smoothness;
        private float roundness;
        private bool? rounded;
        private TaleUtil.Delegates.InterpolationDelegate interpolation;

        private float clock;

        private float initialIntensity;
        private Color initialColor;
        private float initialSmoothness;
        private float initialRoundness;

        private State state;

        private VignetteAction() { }

        public VignetteAction(float intensity, float transitionDuration, Color? color, float smoothness, float roundness, bool? rounded, TaleUtil.Delegates.InterpolationDelegate interpolation)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.postProcessing.bloom, "VignetteAction requires a vignette object (and, therefore, a PostProcessVolume component on the main camera)");

            TaleUtil.Assert.Condition(intensity == float.MinValue || (intensity >= 0f && intensity <= 1f), "Vignette intensity must be between 0 and 1 (inclusive)");
            TaleUtil.Assert.Condition(smoothness == float.MinValue || (smoothness > 0f && smoothness <= 1f), "Vignette smoothness must be between 0 (exclusive) and 1 (inclusive)");
            TaleUtil.Assert.Condition(roundness == float.MinValue || (roundness >= 0f && roundness <= 1f), "Vignette roundness must be between 0 and 1 (inclusive)");

            this.transitionDuration = transitionDuration;
            this.intensity = intensity;
            this.color = color;
            this.smoothness = smoothness;
            this.roundness = roundness;
            this.rounded = rounded;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override TaleUtil.Action Clone()
        {
            VignetteAction clone = new VignetteAction();
            clone.transitionDuration = transitionDuration;
            clone.intensity = intensity;
            clone.color = color;
            clone.smoothness = smoothness;
            clone.roundness = roundness;
            clone.rounded = rounded;
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
                    TaleUtil.Props.postProcessing.vignette.intensity.overrideState  = true;
                    TaleUtil.Props.postProcessing.vignette.color.overrideState      = true;
                    TaleUtil.Props.postProcessing.vignette.smoothness.overrideState = true;
                    TaleUtil.Props.postProcessing.vignette.roundness.overrideState  = true;
                    TaleUtil.Props.postProcessing.vignette.rounded.overrideState    = true;

                    initialIntensity  = TaleUtil.Props.postProcessing.vignette.intensity.value;
                    initialColor      = TaleUtil.Props.postProcessing.vignette.color.value;
                    initialSmoothness = TaleUtil.Props.postProcessing.vignette.smoothness.value;
                    initialRoundness  = TaleUtil.Props.postProcessing.vignette.roundness.value;

                    if(intensity == float.MinValue)
                        intensity = initialIntensity;
                    if(color == null)
                        color = initialColor;
                    if(smoothness == float.MinValue)
                        smoothness = initialSmoothness;
                    if(roundness == float.MinValue)
                        roundness = initialRoundness;
                    if(rounded == null)
                        rounded = TaleUtil.Props.postProcessing.vignette.rounded.value;

                    TaleUtil.Props.postProcessing.vignette.rounded.value = (bool) rounded;

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    clock += Time.deltaTime;

                    if(clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    TaleUtil.Props.postProcessing.vignette.intensity.value  = TaleUtil.Math.Interpolate(initialIntensity, intensity, interpolationFactor);
                    TaleUtil.Props.postProcessing.vignette.color.value      = TaleUtil.Math.Interpolate(initialColor, (Color) color, interpolationFactor);
                    TaleUtil.Props.postProcessing.vignette.smoothness.value = TaleUtil.Math.Interpolate(initialSmoothness, smoothness, interpolationFactor);
                    TaleUtil.Props.postProcessing.vignette.roundness.value  = TaleUtil.Math.Interpolate(initialRoundness, roundness, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }
    }
}
