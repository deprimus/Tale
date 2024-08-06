#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;

namespace TaleUtil
{
    public class BloomAction : Action
    {
        enum State
        {
            SETUP,
            TRANSITION
        }

        float transitionDuration;
        float intensity;
        Color? color;
        float threshold;
        float diffusion;
        float anamorphicRatio;
        Delegates.InterpolationDelegate interpolation;

        float clock;

        float initialIntensity;
        Color initialColor;
        float initialThreshold;
        float initialDiffusion;
        float initialAnamorphicRatio;

        State state;

        BloomAction() { }

        public BloomAction(float intensity, float transitionDuration, Color? color, float threshold, float diffusion, float anamorphicRatio, Delegates.InterpolationDelegate interpolation)
        {
            Assert.Condition(Props.postProcessing.bloom != null, "BloomAction requires a bloom object (and, therefore, a PostProcessVolume component on the main camera)");

            Assert.Condition(intensity == float.MinValue || intensity >= 0f, "Bloom intensity must be at least 0");
            Assert.Condition(threshold == float.MinValue || threshold >= 0f, "Bloom threshold must be at least 0");
            Assert.Condition(diffusion == float.MinValue || (diffusion >= 1f && diffusion <= 10f), "Bloom diffusion must be between 1 and 10 (inclusive)");
            Assert.Condition(anamorphicRatio == float.MinValue || (anamorphicRatio >= -1f && anamorphicRatio <= 1f), "Bloom anamorphic ratio must be between -1 and 1 (inclusive)");

            this.transitionDuration = transitionDuration;
            this.intensity = intensity;
            this.color = color;
            this.threshold = threshold;
            this.diffusion = diffusion;
            this.anamorphicRatio = anamorphicRatio;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override Action Clone()
        {
            BloomAction clone = new BloomAction();
            clone.delta = delta;
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
                    Props.postProcessing.bloom.intensity.overrideState       = true;
                    Props.postProcessing.bloom.color.overrideState           = true;
                    Props.postProcessing.bloom.threshold.overrideState       = true;
                    Props.postProcessing.bloom.diffusion.overrideState       = true;
                    Props.postProcessing.bloom.anamorphicRatio.overrideState = true;

                    initialIntensity       = Props.postProcessing.bloom.intensity.value;
                    initialColor           = Props.postProcessing.bloom.color.value;
                    initialThreshold       = Props.postProcessing.bloom.threshold.value;
                    initialDiffusion       = Props.postProcessing.bloom.diffusion.value;
                    initialAnamorphicRatio = Props.postProcessing.bloom.anamorphicRatio.value;

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
                    clock += delta();

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    Props.postProcessing.bloom.intensity.value       = Math.Interpolate(initialIntensity, intensity, interpolationFactor);
                    Props.postProcessing.bloom.color.value           = Math.Interpolate(initialColor, (Color)color, interpolationFactor);
                    Props.postProcessing.bloom.threshold.value       = Math.Interpolate(initialThreshold, threshold, interpolationFactor);
                    Props.postProcessing.bloom.diffusion.value       = Math.Interpolate(initialDiffusion, diffusion, interpolationFactor);
                    Props.postProcessing.bloom.anamorphicRatio.value = Math.Interpolate(initialAnamorphicRatio, anamorphicRatio, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("BloomAction ({0})", state.ToString());
        }
    }
}
#endif