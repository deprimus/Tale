using UnityEngine;
using UnityEngine.Video;

namespace TaleUtil {
    public class CinematicVideoAction : Action {
        enum State {
            LOAD,
            WAIT_FOR_PREPARATION,
            RESUME,
            SYNC
        }

        string path;
        float speed;
        float syncTimestamp;

        State state;

        // null path -> resume video
        public CinematicVideoAction Init(string path, float speed) {
            Debug.Assert.Condition(master.Props.cinematic.video.group != null, "CinematicVideoAction requires a group object; did you forget to register it in TaleMaster?");
            Debug.Assert.Condition(master.Props.cinematic.video.player != null, "CinematicVideoAction requires a video player object; did you forget to register it in TaleMaster?");

            this.path = path;
            this.speed = speed;

            if (path != null) {
                state = State.LOAD;
            } else {
                state = State.RESUME;
            }

            return this;
        }

        // Video sync
        public CinematicVideoAction Init(float syncTimestamp) {
            this.syncTimestamp = syncTimestamp;

            state = State.SYNC;

            return this;
        }

        VideoClip LoadVideo() {
            VideoClip clip = Resources.Load<VideoClip>(path);
            Check(clip != null, string.Format("The cinematic video '{0}' is missing", path));

            return clip;
        }

        protected override bool Run() {
            var video = master.Props.cinematic.video;
            var player = master.Props.cinematic.video.player;

            switch (state) {
                case State.LOAD: {
                    video.group.SetActive(true);

                    player.playbackSpeed = speed;
                    player.EnableAudioTrack(0, true); // Audio must be set up before Prepare.
                    player.SetTargetAudioSource(0, video.audio);

                    player.clip = LoadVideo();
                    player.Prepare();

                    state = State.WAIT_FOR_PREPARATION;

                    break;
                }
                case State.RESUME: {
                    player.playbackSpeed = speed;
                    player.Play();
                    return true;
                }
                case State.WAIT_FOR_PREPARATION: {
                    if (player.isPrepared) {
                        player.Play();
                        return true;
                    }

                    break;
                }
                case State.SYNC: {
                    return (!player.isPlaying) || (syncTimestamp != float.MinValue && player.time >= syncTimestamp);
                }
            }

            return false;
        }

        public override string ToString() {
            var player = master.Props.cinematic.video.player;
            var left = "";

            if (state == State.SYNC && player.isPlaying && syncTimestamp != float.MinValue) {
                left = string.Format(", <color=#{0}>{1}</color> left", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), Mathf.Max(0f, (float) (syncTimestamp - player.time)).ToString("0.0"));
            }

            return string.Format("CinematicVideoAction (<color=#{0}>{1}</color>{2})", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), state.ToString(), left);
        }
    }
}