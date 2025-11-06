using UnityEngine;

namespace TaleUtil {
    public class CinematicBackgroundAction : Action {
        public enum Type {
            INSTANT,
            CUSTOM,
            CROSSFADE
        }

        enum State {
            INSTANT,
            CROSSFADE_SETUP,
            CROSSFADE,
            CUSTOM_SETUP,
            CUSTOM_TRANSITION_OUT,
            CUSTOM_TRANSITION_IN
        }

        string path;
        float speed;

        State state;

        float clock;

        string customAnimatorState;

        public CinematicBackgroundAction Init(string path, Type type, float speed) {
            Assert.Condition(master.Props.cinematic.background.image != null, "CinematicBackgroundAction requires a background Image object; did you forget to register it in TaleMaster?");

            this.path = path;
            this.speed = speed;

            clock = 0f;

            switch (type) {
                case Type.INSTANT:
                    state = State.INSTANT;
                    break;
                case Type.CUSTOM:
                    Assert.Condition(master.Props.cinematic.background.groupAnimator != null, "CinematicBackgroundAction with type 'Custom' requires a group animator object; did you forget to register it in TaleMaster?");
                    state = State.CUSTOM_SETUP;
                    break;
                case Type.CROSSFADE:
                    Assert.Condition(master.Props.cinematic.background.imageAlt != null, "CinematicBackgroundAction with type 'Crossfade' requires a background (alt) Image object; did you forget to register it in TaleMaster?");
                    state = State.CROSSFADE_SETUP;
                    break;
            }

            return this;
        }

        Sprite LoadSprite() {
            Sprite sprite = Resources.Load<Sprite>(path);
            Assert.Condition(sprite != null, "The cinematic background '" + path + "' is missing");

            return sprite;
        }

        protected override bool Run() {
            switch (state) {
                case State.INSTANT: {
                    master.Props.cinematic.background.GetActiveImage().color = new Color32(255, 255, 255, 255);
                    master.Props.cinematic.background.GetActiveImage().sprite = LoadSprite();
                    return true;
                }
                case State.CROSSFADE_SETUP: {
                    master.Props.cinematic.background.GetPassiveImage().gameObject.SetActive(true);
                    master.Props.cinematic.background.GetPassiveImage().color = new Color32(255, 255, 255, 255);
                    master.Props.cinematic.background.GetPassiveImage().sprite = LoadSprite();

                    state = State.CROSSFADE;

                    break;
                }
                case State.CROSSFADE: {
                    clock += delta();

                    if (clock > speed)
                        clock = speed;

                    Color activeColor = master.Props.cinematic.background.GetActiveImage().color;

                    master.Props.cinematic.background.GetActiveImage().color = new Color32((byte)(activeColor.r * 255), (byte)(activeColor.g * 255), (byte)(activeColor.b * 255), (byte)(255 * (1f - clock / speed)));

                    if (clock == speed) {
                        master.Props.cinematic.background.GetActiveImage().sprite = null;
                        master.Props.cinematic.background.GetActiveImage().gameObject.SetActive(false);
                        master.Props.cinematic.background.Swap();
                        return true;
                    }

                    break;
                }
                case State.CUSTOM_SETUP: {
                    master.Props.cinematic.background.groupAnimator.speed = speed;
                    master.Props.cinematic.background.groupAnimator.SetTrigger(TaleUtil.Config.Editor.CINEMATIC_BACKGROUND_ANIMATOR_TRIGGER);

                    customAnimatorState = string.Format(TaleUtil.Config.Editor.CINEMATIC_BACKGROUND_ANIMATOR_STATE_FORMAT, "Out");

                    state = State.CUSTOM_TRANSITION_OUT;

                    break;
                }
                case State.CUSTOM_TRANSITION_OUT: {
                    AnimatorStateInfo customOutInfo = master.Props.cinematic.background.groupAnimator.GetCurrentAnimatorStateInfo(0);

                    if (!customOutInfo.IsName(customAnimatorState) || customOutInfo.normalizedTime < 1f)
                        break;

                    master.Props.cinematic.background.GetActiveImage().color = new Color32(255, 255, 255, 255);
                    master.Props.cinematic.background.GetActiveImage().sprite = LoadSprite();

                    master.Props.cinematic.background.groupAnimator.speed = 1f;
                    master.Props.cinematic.background.groupAnimator.SetTrigger(TaleUtil.Config.Editor.CINEMATIC_BACKGROUND_ANIMATOR_TRIGGER);

                    customAnimatorState = string.Format(TaleUtil.Config.Editor.CINEMATIC_BACKGROUND_ANIMATOR_STATE_FORMAT, "In");

                    state = State.CUSTOM_TRANSITION_IN;

                    break;
                }
                case State.CUSTOM_TRANSITION_IN: {
                    AnimatorStateInfo customInInfo = master.Props.cinematic.background.groupAnimator.GetCurrentAnimatorStateInfo(0);

                    if (!customInInfo.IsName(customAnimatorState) || customInInfo.normalizedTime < 1f)
                        break;

                    master.Props.cinematic.background.groupAnimator.speed = 1f;
                    return true;
                }
            }

            return false;
        }

        public override string ToString() {
            return string.Format("CinematicBackgroundAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.config.Core.DEBUG_ACCENT_COLOR_PRIMARY), state.ToString());
        }
    }
}