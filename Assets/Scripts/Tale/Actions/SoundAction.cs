using UnityEngine;

namespace TaleUtil {
    public class SoundAction : Action {
        enum State {
            PLAY,
            STOP,
            SYNC,
            WAIT
        }

        public int channel;
        string path;
        float volume;
        float pitch;
        float syncTimestamp;

        State state;

        // null path -> stop sound
        public SoundAction Init(int channel, string path, float volume, float pitch) {
            Debug.Assert.Condition(master.Props.audio.soundGroup != null, "SoundAction requires a sound group object; did you forget to register it in TaleMaster?");
            Debug.Assert.Condition(master.Props.audio.group != null, "SoundAction requires an audio group object; did you forget to register it in TaleMaster?");
            Debug.Assert.Condition(channel >= 0 && channel < master.Props.audio.sound.Length, string.Format("Invalid sound channel '{0}'. Expected channel between '{1}' and '{2}' (inclusive)", channel, 0, master.Props.audio.sound.Length - 1));
            Debug.Assert.Condition(master.Props.audio.sound[channel] != null, string.Format("Channel '{0}' does not have an audio source associated with it; did you forget to register it in TaleMaster?", channel));

            this.channel = channel;
            this.path = path;
            this.volume = volume;
            this.pitch = pitch;

            if (path != null) {
                state = State.PLAY;
            } else {
                state = State.STOP;
            }

            return this;
        }

        // Sound sync
        public SoundAction Init(int channel, float syncTimestamp) {
            state = State.SYNC;

            this.channel = channel;
            this.syncTimestamp = syncTimestamp;

            return this;
        }

        AudioClip LoadAudio() {
            AudioClip clip = Resources.Load<AudioClip>(path);
            Check(clip != null, string.Format("The sound clip '{0}' is missing", path));

            return clip;
        }

        void Finish() {
            master.Props.audio.sound[channel].clip = null;

            Action next = Tale.Master.Queue.FetchNext();

            // The next action isn't a SoundAction.
            if (!(Tale.Master.Queue.FetchNext() is SoundAction)) {
                master.Props.audio.sound[channel].gameObject.SetActive(false);

                // Check if all sound channels are inactive. If so, deactivate the sound group.
                bool areSoundChannelsInactive = true;

                for (int i = 0; i < master.Props.audio.sound.Length; ++i) {
                    if (master.Props.audio.sound[i] != null && master.Props.audio.sound[i].gameObject.activeSelf) {
                        areSoundChannelsInactive = false;
                        break;
                    }
                }

                if (areSoundChannelsInactive) {
                    // Deactivate the sound group.
                    master.Props.audio.soundGroup.SetActive(false);

                    // Deactivate the audio group.
                    if ((master.Props.audio.music == null || !master.Props.audio.music.gameObject.activeSelf) && (master.Props.audio.voice == null || !master.Props.audio.voice.gameObject.activeSelf))
                        master.Props.audio.group.SetActive(false);
                }
            } else if (((SoundAction)Tale.Master.Queue.FetchNext()).channel != channel) {
                // If this channel is not used in the next action, deactivate it.
                master.Props.audio.sound[channel].gameObject.SetActive(false);
            }
        }

        protected override bool Run() {
            switch (state) {
                case State.PLAY: {
                    master.Props.audio.group.SetActive(true);
                    master.Props.audio.soundGroup.SetActive(true);
                    master.Props.audio.sound[channel].gameObject.SetActive(true);

                    master.Props.audio.sound[channel].volume = volume;
                    master.Props.audio.sound[channel].pitch = pitch;

                    master.Props.audio.sound[channel].clip = LoadAudio();
                    master.Props.audio.sound[channel].Play();

                    state = State.WAIT;

                    break;
                }
                case State.STOP: {
                    master.Props.audio.sound[channel].Stop();

                    Finish();

                    return true;
                }
                case State.SYNC: {
                    // Sound is done, or the sync timestamp was reached
                    // If the timestamp is float.MinValue, then wait for the end of the sound
                    return (!master.Props.audio.sound[channel].isPlaying || (syncTimestamp != float.MinValue && master.Props.audio.sound[channel].time >= syncTimestamp));
                }
                case State.WAIT: {
                    if (!master.Props.audio.sound[channel].isPlaying) {
                        Finish();

                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public override string ToString() =>
            string.Format("SoundAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), state.ToString());
    }
}