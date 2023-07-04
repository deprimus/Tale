using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class MusicAction : Action
    {
        public enum Mode
        {
            ONCE,
            LOOP,
            SHUFFLE,
            SHUFFLE_LOOP
        }

        enum State
        {
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

        MusicAction() { }

        public MusicAction(List<string> paths, Mode mode, float volume, float pitch)
        {
            Assert.Condition(Props.audio.music != null, "MusicAction requires a music object with an AudioSource component; did you forget to register it in TaleMaster?");
            Assert.Condition(Props.audio.group != null, "MusicAction requires an audio group object; did you forget to register it in TaleMaster?");

            Assert.Condition(paths != null, "Expected a path list (found null)");
            Assert.Condition(paths.Count > 0, "Expected a list with at least one music track (found an empty list)");

            state = State.PLAY;
                
            this.paths = paths;
            this.mode = mode;
            this.volume = volume;
            this.pitch = pitch;
        }

        // Music stop with a fade out duration parameter.
        public MusicAction(float stopDuration, Delegates.InterpolationDelegate interpolation)
        {
            state = State.STOP;

            this.stopDuration = stopDuration;
            this.interpolation = interpolation == null ? Math.Identity : interpolation;
            clock = 0f;
        }

        // Music sync
        public MusicAction(float syncTimestamp)
        {
            state = State.SYNC;

            this.syncTimestamp = syncTimestamp;
        }

        AudioClip LoadAudio(string path)
        {
            AudioClip clip = Resources.Load<AudioClip>(path);
            Assert.Condition(clip != null, "The music clip '" + path + "' is missing");

            return clip;
        }

        // TODO: implement a global music modifier (1.0 = normal, 0.5 = half), and apply it every time.

        void Finish()
        {
            Props.audio.music.clip = null;

            Action next = Queue.FetchNext();

            // The next action isn't a MusicAction.
            if (!(Queue.FetchNext() is MusicAction))
            {
                Props.audio.music.gameObject.SetActive(false);

                // Deactivate the audio group.
                if ((Props.audio.soundGroup == null || !Props.audio.soundGroup.gameObject.activeSelf) && (Props.audio.voice == null || !Props.audio.voice.gameObject.activeSelf))
                    Props.audio.group.SetActive(false);
            }
        }

        void ReinitList()
        {
            currentIndex = -1;

            if(sources == null)
            {
                sources = new List<AudioClip>();

                foreach (string path in paths)
                    sources.Add(LoadAudio(path));
            }

            switch(mode)
            {
                case Mode.ONCE:
                case Mode.LOOP:
                {
                    current = sources;
                    break;
                }
                case Mode.SHUFFLE:
                case Mode.SHUFFLE_LOOP:
                {
                    if(current == null)
                        current = new List<AudioClip>(sources.Count);

                    // Inside-Out Fisher-Yates shuffle.
                    for(int i = 0; i < sources.Count; ++i)
                    {
                        int j = Random.Range(0, i + 1);

                        // Set() is an extension method.
                        if(i != j)
                            current.Set(i, current[j]);
                        current.Set(j, sources[i]);
                    }

                    break;
                }
            }
        }

        void LoadNext()
        {
            ++currentIndex;
            Props.audio.music.clip = current[currentIndex];
            Props.audio.music.Play();
        }

        bool HasNext()
        {
            return currentIndex < current.Count - 1;
        }

        public override Action Clone()
        {
            MusicAction clone = new MusicAction();
            clone.delta = delta;
            clone.paths = new List<string>(paths);
            clone.mode = mode;
            clone.volume = volume;
            clone.pitch = pitch;
            clone.state = state;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.PLAY:
                {
                    Props.audio.group.SetActive(true);
                    Props.audio.music.gameObject.SetActive(true);

                    Props.audio.music.volume = volume;
                    Props.audio.music.pitch = pitch;

                    ReinitList();
                    LoadNext();

                    state = State.WAIT;

                    break;
                }
                case State.WAIT:
                {
                    if(Triggers.Get("tale_music_stop"))
                    {
                        return true;
                    }

                    if(!Props.audio.music.isPlaying)
                    {
                        // The trigger was activated on this frame. Prepare to handle it in the next frame.
                        // Since the music has finished and a trigger was set, there's no point in starting a new song.
                        if(Triggers.GetImmediate("tale_music_stop"))
                            return false;

                        if(HasNext())
                        {
                            LoadNext();
                        }
                        else
                        {
                            switch(mode)
                            {
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
                case State.STOP:
                {
                    initialVolume = Props.audio.music.volume;

                    state = State.FADE_OUT;

                    return false;
                }
                case State.SYNC:
                {
                    // Music is done, or the sync timestamp was reached
                    // If the timestamp is float.MinValue, then wait for the end of the music
                    return (!Props.audio.music.isPlaying || (syncTimestamp != float.MinValue && Props.audio.music.time >= syncTimestamp));
                }
                case State.FADE_OUT:
                {
                    clock += delta();

                    if(clock > stopDuration)
                        clock = stopDuration;

                    float interpolationFactor = interpolation(stopDuration == 0f ? 1f : clock / stopDuration);

                    Props.audio.music.volume = Math.Interpolate(initialVolume, 0f, interpolationFactor);

                    if(clock == stopDuration)
                    {
                        // Signal other music actions to stop. The trigger will be checked in the WAIT state.
                        // If the music stop action is immediately followed by a music play action, that action
                        // will enter the PLAY state, while this trigger is set. That action shouldn't stop, so
                        // don't check the trigger in the PLAY state.
                        Triggers.Set("tale_music_stop");

                        Props.audio.music.Stop();

                        Finish();

                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("MusicAction ({0})", state.ToString());
        }
    }
}