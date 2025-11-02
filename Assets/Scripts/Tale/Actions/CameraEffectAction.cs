#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;

namespace TaleUtil
{
    public class CameraEffectAction : Action
    {
        enum State
        {
            SETUP,
            TRANSITION_IN,
            TRANSITION_OUT
        }

        float transitionDuration;
        Texture lut;
        Delegates.InterpolationDelegate interpolation;

        float clock;
        float initialContribution;
        State state;

        CameraEffectAction(TaleMaster master) : base(master) { }

        public CameraEffectAction(TaleMaster master, string effect, float transitionDuration, Delegates.InterpolationDelegate interpolation) : base(master)
        {
            Assert.Condition(master.Props.postProcessing.colorGrading != null, "CameraEffectAction requires a color grading object (and, therefore, a PostProcessVolume component on the main camera)");

            if (effect != null)
            {
                effect = effect.ToLower();
                Assert.Condition(master.Props.cameraEffects.ContainsKey(effect), string.Format("Unregistered camera effect '{0}'", effect));

                lut = master.Props.cameraEffects[effect];
            }
            else
            {
                lut = null;
            }

            this.transitionDuration = transitionDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;

            clock = 0f;

            state = State.SETUP;
        }

        public override Action Clone()
        {
            CameraEffectAction clone = new CameraEffectAction(master);
            clone.delta = delta;
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
                    //master.Props.postProcessing.colorGrading.active = true;
                    master.Props.postProcessing.colorGrading.ldrLut.overrideState = true;
                    master.Props.postProcessing.colorGrading.ldrLutContribution.overrideState = true;

                    if(lut != null)
                    {
                        master.Props.postProcessing.colorGrading.ldrLut.value = lut;
                        master.Props.postProcessing.colorGrading.ldrLutContribution.value = 0f;

                        state = State.TRANSITION_IN;
                    }
                    else
                    {
                        initialContribution = master.Props.postProcessing.colorGrading.ldrLutContribution.value;
                        state = State.TRANSITION_OUT;
                    }

                    break;
                }
                case State.TRANSITION_IN:
                {
                    clock += delta();

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    master.Props.postProcessing.colorGrading.ldrLutContribution.value = Math.Interpolate(0f, 1f, interpolationFactor);

                    if(clock == transitionDuration)
                        return true;

                    break;
                }
                case State.TRANSITION_OUT:
                {
                    clock += delta();

                    if (clock > transitionDuration)
                        clock = transitionDuration;

                    float interpolationFactor = interpolation(transitionDuration == 0f ? 1f : clock / transitionDuration);

                    master.Props.postProcessing.colorGrading.ldrLutContribution.value = Math.Interpolate(initialContribution, 0f, interpolationFactor);

                    if(clock == transitionDuration)
                    {
                        master.Props.postProcessing.colorGrading.ldrLut.value = null;
                        master.Props.postProcessing.colorGrading.ldrLut.overrideState = false;
                        //master.Props.postProcessing.colorGrading.active = false;

                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("CameraEffectAction ({0})", state.ToString());
        }
    }
}
#endif