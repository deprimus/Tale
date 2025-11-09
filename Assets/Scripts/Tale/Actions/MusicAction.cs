using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil {
    public class MusicAction : Action {
        public enum Mode {
            ONCE,
            LOOP,
            SHUFFLE,
            SHUFFLE_LOOP
        }

        enum State {
            PLAY,
            WAIT,
            STOP,
            SYNC,
            FADE_OUT
        }

        List<string> paths;
        Mode mode;
        float volume;
        float pitch;

        List<AudioClip> sources;
        List<AudioClip> current;
        public int currentIndex;

        float stopDuration;
        float syncTimestamp;
        Delegates.InterpolationDelegate interpolation;
        float clock;
        float initialVolume;

        State state;

        public MusicAction Init(List<string> paths, Mode mode, float volume, float pitch) {
            Assert.Condition(master.Props.audio.music != null, "MusicAction requires a music object with an AudioSource component; did you forget to register it in TaleMaster?");
            Assert.Condition(master.Props.audio.group != null, "MusicAction requires an audio group object; did you forget to register it in TaleMaster?");

            Assert.Condition(paths != null, "Expected a path list (found null)");
            Assert.Condition(paths.Count > 0, "Expected a list with at least one music track (found an empty list)");

            state = State.PLAY;

            this.paths = paths;
            this.mode = mode;
            this.volume = volume;
            this.pitch = pitch;

            return this;
        }

        // Music stop with a fade out duration parameter.
        public MusicAction Init(float stopDuration, Delegates.InterpolationDelegate interpolation) {
            state = State.STOP;

            this.stopDuration = stopDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;
            clock = 0f;

            return this;
        }

        // Music sync
        public MusicAction Init(float syncTimestamp) {
            state = State.SYNC;

            this.syncTimestamp = syncTimestamp;

            return this;
        }

        AudioClip LoadAudio(string path) {
            AudioClip clip = Resources.Load<AudioClip>(path);
            Assert.Condition(clip != null, "The music clip '" + path + "' is missing");

            return clip;
        }

        // TODO: implement a global music modifier (1.0 = normal, 0.5 = half), and apply it every time.

        void Finish() {
            master.Props.audio.music.clip = null;

            Action next = Tale.Master.Queue.FetchNext();

            // The next action isn't a MusicAction.
            if (!(Tale.Master.Queue.FetchNext() is MusicAction)) {
                master.Props.audio.music.gameObject.SetActive(false);

                // Deactivate the audio group.
                if ((master.Props.audio.soundGroup == null || !master.Props.audio.soundGroup.gameObject.activeSelf) && (master.Props.audio.voice == null || !master.Props.audio.voice.gameObject.activeSelf))
                    master.Props.audio.group.SetActive(false);
            }
        }

        void ReinitList() {
            currentIndex = -1;

            if (sources == null) {
                sources = new List<AudioClip>();

                foreach (string path in paths)
                    sources.Add(LoadAudio(path));
            }

            switch (mode) {
                case Mode.ONCE:
                case Mode.LOOP: {
                    current = sources;
                    break;
                }
                case Mode.SHUFFLE:
                case Mode.SHUFFLE_LOOP: {
                    if (current == null)
                        current = new List<AudioClip>(sources.Count);

                    // Inside-Out Fisher-Yates shuffle.
                    for (int i = 0; i < sources.Count; ++i) {
                        int j = Random.Range(0, i + 1);

                        // Set() is an extension method.
                        if (i != j)
                            current.Set(i, current[j]);
                        current.Set(j, sources[i]);
                    }

                    break;
                }
            }
        }

        void LoadNext() {
            ++currentIndex;
            master.Props.audio.music.clip = current[currentIndex];
            master.Props.audio.music.Play();
        }

        bool HasNext() {
            return currentIndex < current.Count - 1;
        }

        protected override bool Run() {
            switch (state) {
                case State.PLAY: {
                    master.Props.audio.group.SetActive(true);
                    master.Props.audio.music.gameObject.SetActive(true);

                    master.Props.audio.music.volume = volume;
                    master.Props.audio.music.pitch = pitch;

                    ReinitList();
                    LoadNext();

                    state = State.WAIT;

                    break;
                }
                case State.WAIT: {
                    if (master.Triggers.Get("tale_music_stop")) {
                        return true;
                    }

                    if (!master.Props.audio.music.isPlaying) {
                        // The trigger was activated on this frame. Prepare to handle it in the next frame.
                        // Since the music has finished and a trigger was set, there's no point in starting a new song.
                        if (master.Triggers.GetImmediate("tale_music_stop"))
                            return false;

                        if (HasNext()) {
                            LoadNext();
                        } else {
                            switch (mode) {
                                case Mode.ONCE:
                                case Mode.SHUFFLE:
                                    Finish();
                                    return true;
                                default:
                                    ReinitList();
                                    LoadNext();
                                    return false;
                            }
                        }
                    }

                    break;
                }
                case State.STOP: {
                    initialVolume = master.Props.audio.music.volume;

                    state = State.FADE_OUT;

                    return false;
                }
                case State.SYNC: {
                    // Music is done, or the sync timestamp was reached
                    // If the timestamp is float.MinValue, then wait for the end of the music
                    return (!master.Props.audio.music.isPlaying || (syncTimestamp != float.MinValue && master.Props.audio.music.time >= syncTimestamp));
                }
                case State.FADE_OUT: {
                    clock += delta();

                    if (clock > stopDuration)
                        clock = stopDuration;

                    float interpolationFactor = interpolation(stopDuration == 0f ? 1f : clock / stopDuration);

                    master.Props.audio.music.volume = Math.Interpolate(initialVolume, 0f, interpolationFactor);

                    if (clock == stopDuration) {
                        // Signal other music actions to stop. The trigger will be checked in the WAIT state.
                        // If the music stop action is immediately followed by a music play action, that action
                        // will enter the PLAY state, while this trigger is set. That action shouldn't stop, so
                        // don't check the trigger in the PLAY state.
                        master.Triggers.Set("tale_music_stop");

                        master.Props.audio.music.Stop();

                        Finish();

                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        public override string ToString() {
            return string.Format("MusicAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), state.ToString());
        }
    }
}