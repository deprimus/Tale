using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class BloomAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            TRANSITION
        }

        private float transitionDuration;
        private float intensity;
        private Color? color;
        private float threshold;
        private float diffusion;
        private float anamorphicRatio;
        private TaleUtil.Delegates.InterpolationDelegate interpolation;

        private float clock;

        private float initialIntensity;
        private Color initialColor;
        private float initialThreshold;
        private float initialDiffusion;
        private float initialAnamorphicRatio;

        private State state;

        private BloomAction() { }

        public BloomAction(float intensity, float transitionDuration, Color? color, float threshold, float diffusion, float anamorphicRatio, TaleUtil.Delegates.InterpolationDelegate interpolation)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.postProcessing.bloom, "BloomAction requires a bloom object (and, therefore, a PostProcessVolume component on the main camera)");

            TaleUtil.Assert.Condition(intensity == float.MinValue || intensity >= 0f, "Bloom intensity must be at least 0");
            TaleUtil.Assert.Condition(threshold == float.MinValue || threshold >= 0f, "Bloom threshold must be at least 0");
            TaleUtil.Assert.Condition(diffusion == float.MinValue || (diffusion >= 1f && diffusion <= 10f), "Bloom diffusion must be between 1 and 10 (inclusive)");
            TaleUtil.Assert.Condition(anamorphicRatio == float.MinValue || (anamorphicRatio >= -1f && anamorphicRatio <= 1f), "Bloom anamorphic ratio must be between -1 and 1 (inclusive)");

            this.transitionDuration = transitionDuration;
            this.intensity = intensity;
            this.color = color;
            this.threshold = threshold;
            this.diffusion = diffusion;
            this.anamorphicRatio = anamorphicRatio;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override TaleUtil.Action Clone()
        {
            BloomAction clone = new BloomAction();
            clone.transitionDuration = transitionDuration;
            clone.intensity = intensity;
            clone.color = color;
            clone.threshold = threshold;
            clone.diffusion = diffusion;
            clone.anamorphicRatio = anamorphicRatio;
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
                    TaleUtil.Props.postProcessing.bloom.intensity.overrideState       = true;
                    TaleUtil.Props.postProcessing.bloom.color.overrideState           = true;
                    TaleUtil.Props.postProcessing.bloom.threshold.overrideState       = true;
                    TaleUtil.Props.postProcessing.bloom.diffusion.overrideState       = true;
                    TaleUtil.Props.postProcessing.bloom.anamorphicRatio.overrideState = true;

                    initialIntensity       = TaleUtil.Props.postProcessing.bloom.intensity.value;
                    initialColor           = TaleUtil.Props.postProcessing.bloom.color.value;
                    initialThreshold       = TaleUtil.Props.postProcessing.bloom.threshold.value;
                    initialDiffusion       = TaleUtil.Props.postProcessing.bloom.diffusion.value;
                    initialAnamorphicRatio = TaleUtil.Props.postProcessing.bloom.anamorphicRatio.value;

                    if(intensity == float.MinValue)
                        intensity = initialIntensity;
                    if(color == null)
                        color = initialColor;
                    if(threshold == float.MinValue)
                        threshold = initialThreshold;
                    if(diffusion == float.MinValue)
                        diffusion = initialDiffusion;
                    if(anamorphicRatio == float.MinValue)
                        anamorphicRatio = initialAnamorphicRatio;

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    clock += Time.deltaTime;

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    TaleUtil.Props.postProcessing.bloom.intensity.value       = TaleUtil.Math.Interpolate(initialIntensity, intensity, interpolationFactor);
                    TaleUtil.Props.postProcessing.bloom.color.value           = TaleUtil.Math.Interpolate(initialColor, (Color)color, interpolationFactor);
                    TaleUtil.Props.postProcessing.bloom.threshold.value       = TaleUtil.Math.Interpolate(initialThreshold, threshold, interpolationFactor);
                    TaleUtil.Props.postProcessing.bloom.diffusion.value       = TaleUtil.Math.Interpolate(initialDiffusion, diffusion, interpolationFactor);
                    TaleUtil.Props.postProcessing.bloom.anamorphicRatio.value = TaleUtil.Math.Interpolate(initialAnamorphicRatio, anamorphicRatio, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }
    }
}
