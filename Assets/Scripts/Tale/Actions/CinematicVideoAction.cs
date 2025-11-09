using UnityEngine;
using UnityEngine.Video;

namespace TaleUtil {
    public class CinematicVideoAction : Action {
        public enum DetatchType {
            BEFORE,
            AFTER,
            FIXED
        }

        enum State {
            LOAD,
            WAIT_FOR_PREPARATION,
            PLAY,
            WAIT_WITH_DETATCH_BEFORE,
            WAIT_WITH_DETATCH_AFTER,
            WAIT_WITH_DETATCH_FIXED,
            INSTANT,
            CROSSFADE_SETUP,
            CROSSFADE,
            CUSTOM_SETUP,
            CUSTOM_TRANSITION_OUT,
            CUSTOM_TRANSITION_IN
        }

        string path;
        float speed;
        float detatchTime;
        DetatchType detatchType;

        State state;

        double startTime; // VideoPlayer.time is of type double.

        public CinematicVideoAction Init(string path, float detatchTime, DetatchType detatchType, float speed) {
            Assert.Condition(master.Props.cinematic.video.group != null, "CinematicVideoAction requires a group object; did you forget to register it in TaleMaster?");
            Assert.Condition(master.Props.cinematic.video.player != null, "CinematicVideoAction requires a video player object; did you forget to register it in TaleMaster?");

            this.path = path;
            this.detatchTime = detatchTime;
            this.detatchType = detatchType;
            this.speed = speed;

            startTime = 0f;

            if (path == null)
                state = State.PLAY;
            else state = State.LOAD;

            return this;
        }

        VideoClip LoadVideo() {
            VideoClip clip = Resources.Load<VideoClip>(path);
            Assert.Condition(clip != null, "The cinematic video '" + path + "' is missing");

            return clip;
        }

        protected override bool Run() {
            switch (state) {
                case State.LOAD: {
                    master.Props.cinematic.video.group.SetActive(true);

                    master.Props.cinematic.video.player.playbackSpeed = speed;
                    master.Props.cinematic.video.player.EnableAudioTrack(0, true); // Audio must be set up before Prepare.
                    master.Props.cinematic.video.player.SetTargetAudioSource(0, master.Props.cinematic.video.audio);

                    master.Props.cinematic.video.player.clip = LoadVideo();
                    master.Props.cinematic.video.player.Prepare();

                    state = State.WAIT_FOR_PREPARATION;

                    break;
                }
                case State.WAIT_FOR_PREPARATION: {
                    if (master.Props.cinematic.video.player.isPrepared) {
                        state = State.PLAY;
                    }

                    break;
                }
                case State.PLAY: {
                    master.Props.cinematic.video.player.Play();

                    if (detatchTime < 0f)
                        return true;

                    switch (detatchType) {
                        case DetatchType.BEFORE:
                            state = State.WAIT_WITH_DETATCH_BEFORE;
                            break;
                        case DetatchType.AFTER:
                            state = State.WAIT_WITH_DETATCH_AFTER;
                            startTime = master.Props.cinematic.video.player.time;
                            break;
                        case DetatchType.FIXED:
                            state = State.WAIT_WITH_DETATCH_FIXED;
                            break;
                    }

                    break;
                }
                case State.WAIT_WITH_DETATCH_BEFORE: {
                    // Less than (or equal to) detatchTime seconds left, or the video stopped by reaching the end.
                    if (master.Props.cinematic.video.player.time >= master.Props.cinematic.video.player.length - detatchTime * master.Props.cinematic.video.player.playbackSpeed || (!master.Props.cinematic.video.player.isPlaying)) {
                        return true;
                    }

                    break;
                }
                case State.WAIT_WITH_DETATCH_AFTER: {
                    // At least detatchTime seconds passed, or the video stopped by reaching the end.
                    if (master.Props.cinematic.video.player.time - startTime >= detatchTime * master.Props.cinematic.video.player.playbackSpeed || (!master.Props.cinematic.video.player.isPlaying)) {
                        return true;
                    }

                    break;
                }
                case State.WAIT_WITH_DETATCH_FIXED: {
                    // Time reached, or the video stopped by reaching the end.
                    if (master.Props.cinematic.video.player.time >= detatchTime || (!master.Props.cinematic.video.player.isPlaying)) {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public override string ToString() {
            return string.Format("CinematicVideoAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), state.ToString());
        }
    }
}