#pragma warning disable 0162 // Disable the 'unreachable code' warning caused by config constants.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TaleUtil
{
    public class CinematicBackgroundAction : Action
    {
        public enum Type
        {
            INSTANT,
            CUSTOM,
            CROSSFADE
        }

        private enum State
        {
            INSTANT,
            CROSSFADE_SETUP,
            CROSSFADE,
            CUSTOM_SETUP,
            CUSTOM_TRANSITION_OUT,
            CUSTOM_TRANSITION_IN
        }

        private string path;
        private float speed;

        private State state;

        private float clock;

        private string customAnimatorState;

        private CinematicBackgroundAction() { }

        public CinematicBackgroundAction(string path, Type type, float speed)
        {
            Assert.Condition(Props.cinematic.background.image != null, "CinematicBackgroundAction requires a background Image object; did you forget to register it in TaleMaster?");

            this.path = path;
            this.speed = speed;

            this.clock = 0f;

            switch (type)
            {
                case Type.INSTANT:
                    state = State.INSTANT;
                    break;
                case Type.CUSTOM:
                    Assert.Condition(Props.cinematic.background.groupAnimator != null, "CinematicBackgroundAction with type 'Custom' requires a group animator object; did you forget to register it in TaleMaster?");
                    state = State.CUSTOM_SETUP;
                    break;
                case Type.CROSSFADE:
                    Assert.Condition(Props.cinematic.background.imageAlt != null, "CinematicBackgroundAction with type 'Crossfade' requires a background (alt) Image object; did you forget to register it in TaleMaster?");
                    state = State.CROSSFADE_SETUP;
                    break;
            }
        }

        private Sprite LoadSprite()
        {
            Sprite sprite = Resources.Load<Sprite>(path);
            Assert.Condition(sprite != null, "The cinematic background '" + path + "' is missing");

            return sprite;
        }

        public override Action Clone()
        {
            CinematicBackgroundAction clone = new CinematicBackgroundAction();
            clone.path = path;
            clone.speed = speed;
            clone.state = state;
            clone.clock = clock;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.INSTANT:
                {
                    Props.cinematic.background.GetActiveImage().color = new Color32(255, 255, 255, 255);
                    Props.cinematic.background.GetActiveImage().sprite = LoadSprite();
                    return true;

                    break;
                }
                case State.CROSSFADE_SETUP:
                {
                    Props.cinematic.background.GetPassiveImage().gameObject.SetActive(true);
                    Props.cinematic.background.GetPassiveImage().color = new Color32(255, 255, 255, 255);
                    Props.cinematic.background.GetPassiveImage().sprite = LoadSprite();

                    state = State.CROSSFADE;

                    break;
                }
                case State.CROSSFADE:
                {
                    clock += Time.deltaTime;

                    if(clock > speed)
                        clock = speed;

                    Props.cinematic.background.GetActiveImage().color = new Color32(255, 255, 255, (byte) (255 * (1f - clock / speed)));

                    if(clock == speed)
                    {
                        Props.cinematic.background.GetActiveImage().sprite = null;
                        Props.cinematic.background.GetActiveImage().gameObject.SetActive(false);
                        Props.cinematic.background.Swap();
                        return true;
                    }

                    break;
                }
                case State.CUSTOM_SETUP:
                {
                    Props.cinematic.background.groupAnimator.speed = speed;
                    Props.cinematic.background.groupAnimator.SetTrigger(Config.CINEMATIC_BACKGROUND_ANIMATOR_TRIGGER);

                    customAnimatorState = string.Format(Config.CINEMATIC_BACKGROUND_ANIMATOR_STATE_FORMAT, "Out");

                    state = State.CUSTOM_TRANSITION_OUT;

                    break;
                }
                case State.CUSTOM_TRANSITION_OUT:
                {
                    AnimatorStateInfo customOutInfo = Props.cinematic.background.groupAnimator.GetCurrentAnimatorStateInfo(0);

                    if(!customOutInfo.IsName(customAnimatorState) || customOutInfo.normalizedTime < 1f)
                        break;

                    Props.cinematic.background.GetActiveImage().color = new Color32(255, 255, 255, 255);
                    Props.cinematic.background.GetActiveImage().sprite = LoadSprite();

                    Props.cinematic.background.groupAnimator.speed = 1f;
                    Props.cinematic.background.groupAnimator.SetTrigger(Config.CINEMATIC_BACKGROUND_ANIMATOR_TRIGGER);

                    customAnimatorState = string.Format(Config.CINEMATIC_BACKGROUND_ANIMATOR_STATE_FORMAT, "In");

                    state = State.CUSTOM_TRANSITION_IN;

                    break;
                }
                case State.CUSTOM_TRANSITION_IN:
                {
                    AnimatorStateInfo customInInfo = Props.cinematic.background.groupAnimator.GetCurrentAnimatorStateInfo(0);

                    if (!customInInfo.IsName(customAnimatorState) || customInInfo.normalizedTime < 1f)
                        break;

                    Props.cinematic.background.groupAnimator.speed = 1f;
                    return true;

                    break;
                }
            }

            return false;
        }
    }
}