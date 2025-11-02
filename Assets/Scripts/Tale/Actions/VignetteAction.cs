#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;

namespace TaleUtil
{
    public class VignetteAction : Action
    {
        enum State
        {
            SETUP,
            TRANSITION
        }

        float transitionDuration;
        float intensity;
        Color? color;
        float smoothness;
        float roundness;
        bool? rounded;
        Delegates.InterpolationDelegate interpolation;

        float clock;

        float initialIntensity;
        Color initialColor;
        float initialSmoothness;
        float initialRoundness;

        State state;

        VignetteAction(TaleMaster master) : base(master) { }

        public VignetteAction(TaleMaster master, float intensity, float transitionDuration, Color? color, float smoothness, float roundness, bool? rounded, Delegates.InterpolationDelegate interpolation) : base(master)
        {
            Assert.Condition(master.Props.postProcessing.bloom != null, "VignetteAction requires a vignette object (and, therefore, a PostProcessVolume component on the main camera)");

            Assert.Condition(intensity == float.MinValue || (intensity >= 0f && intensity <= 1f), "Vignette intensity must be between 0 and 1 (inclusive)");
            Assert.Condition(smoothness == float.MinValue || (smoothness > 0f && smoothness <= 1f), "Vignette smoothness must be between 0 (exclusive) and 1 (inclusive)");
            Assert.Condition(roundness == float.MinValue || (roundness >= 0f && roundness <= 1f), "Vignette roundness must be between 0 and 1 (inclusive)");

            this.transitionDuration = transitionDuration;
            this.intensity = intensity;
            this.color = color;
            this.smoothness = smoothness;
            this.roundness = roundness;
            this.rounded = rounded;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override Action Clone()
        {
            VignetteAction clone = new VignetteAction(master);
            clone.delta = delta;
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
                    master.Props.postProcessing.vignette.intensity.overrideState  = true;
                    master.Props.postProcessing.vignette.color.overrideState      = true;
                    master.Props.postProcessing.vignette.smoothness.overrideState = true;
                    master.Props.postProcessing.vignette.roundness.overrideState  = true;
                    master.Props.postProcessing.vignette.rounded.overrideState    = true;

                    initialIntensity  = master.Props.postProcessing.vignette.intensity.value;
                    initialColor      = master.Props.postProcessing.vignette.color.value;
                    initialSmoothness = master.Props.postProcessing.vignette.smoothness.value;
                    initialRoundness  = master.Props.postProcessing.vignette.roundness.value;

                    if(intensity == float.MinValue)
                        intensity = initialIntensity;
                    if(color == null)
                        color = initialColor;
                    if(smoothness == float.MinValue)
                        smoothness = initialSmoothness;
                    if(roundness == float.MinValue)
                        roundness = initialRoundness;
                    if(rounded == null)
                        rounded = master.Props.postProcessing.vignette.rounded.value;

                    master.Props.postProcessing.vignette.rounded.value = (bool) rounded;

                    state = State.TRANSITION;

                    break;
                }
                case State.TRANSITION:
                {
                    clock += delta();

                    if(clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    master.Props.postProcessing.vignette.intensity.value  = Math.Interpolate(initialIntensity, intensity, interpolationFactor);
                    master.Props.postProcessing.vignette.color.value      = Math.Interpolate(initialColor, (Color) color, interpolationFactor);
                    master.Props.postProcessing.vignette.smoothness.value = Math.Interpolate(initialSmoothness, smoothness, interpolationFactor);
                    master.Props.postProcessing.vignette.roundness.value  = Math.Interpolate(initialRoundness, roundness, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("VignetteAction ({0})", state.ToString());
        }
    }
}
#endif