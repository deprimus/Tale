using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class MusicAction : TaleUtil.Action
    {
        public enum Mode
        {
            ONCE,
            LOOP,
            SHUFFLE,
            SHUFFLE_LOOP
        }

        private enum State
        {
            PLAY,
            WAIT,
            STOP,
            FADE_OUT
        }

        private List<string> paths;
        private Mode mode;
        private float volume;
        private float pitch;

        private List<AudioClip> sources;
        private List<AudioClip> current;
        public int currentIndex;

        private float stopDuration;
        TaleUtil.Delegates.InterpolationDelegate interpolation;
        private float clock;
        private float initialVolume;

        private State state;

        private MusicAction() { }

        public MusicAction(List<string> paths, Mode mode, float volume, float pitch)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.audio.music, "MusicAction requires a music object with an AudioSource component; did you forget to register it in TaleMaster?");
            TaleUtil.Assert.NotNull(TaleUtil.Props.audio.group, "MusicAction requires an audio group object; did you forget to register it in TaleMaster?");

            TaleUtil.Assert.NotNull(paths, "Expected a path list (found null)");
            TaleUtil.Assert.Condition(paths.Count > 0, "Expected a list with at least one music track (found an empty list)");

            state = State.PLAY;
                
            this.paths = paths;
            this.mode = mode;
            this.volume = volume;
            this.pitch = pitch;
        }

        // Music stop with a fade out duration parameter.
        public MusicAction(float stopDuration, TaleUtil.Delegates.InterpolationDelegate interpolation)
        {
            state = State.STOP;

            this.stopDuration = stopDuration;
            this.interpolation = interpolation == null ? TaleUtil.Math.Identity : interpolation;
            this.clock = 0f;
        }

        private AudioClip LoadAudio(string path)
        {
            AudioClip clip = Resources.Load<AudioClip>(path);
            TaleUtil.Assert.NotNull(clip, "The music clip '" + path + "' is missing");

            return clip;
        }

        // TODO: implement a global music modifier (1.0 = normal, 0.5 = half), and apply it every time.

        private void Finish()
        {
            TaleUtil.Props.audio.music.clip = null;

            TaleUtil.Action next = TaleUtil.Queue.FetchNext();

            // The next action isn't a MusicAction.
            if (!(TaleUtil.Queue.FetchNext() is MusicAction))
            {
                TaleUtil.Props.audio.music.gameObject.SetActive(false);

                // Deactivate the audio group.
                if ((TaleUtil.Props.audio.soundGroup == null || !TaleUtil.Props.audio.soundGroup.gameObject.activeSelf) && (TaleUtil.Props.audio.voice == null || !TaleUtil.Props.audio.voice.gameObject.activeSelf))
                    TaleUtil.Props.audio.group.SetActive(false);
            }
        }

        private void ReinitList()
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

        private void LoadNext()
        {
            ++currentIndex;
            TaleUtil.Props.audio.music.clip = current[currentIndex];
            TaleUtil.Props.audio.music.Play();
        }

        private bool HasNext()
        {
            return currentIndex < current.Count - 1;
        }

        public override TaleUtil.Action Clone()
        {
            MusicAction clone = new MusicAction();
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
                    TaleUtil.Props.audio.group.SetActive(true);
                    TaleUtil.Props.audio.music.gameObject.SetActive(true);

                    TaleUtil.Props.audio.music.volume = volume;
                    TaleUtil.Props.audio.music.pitch = pitch;

                    ReinitList();
                    LoadNext();

                    state = State.WAIT;

                    break;
                }
                case State.WAIT:
                {
                    if(TaleUtil.Triggers.Get("tale_music_stop"))
                    {
                        return true;
                    }

                    if(!TaleUtil.Props.audio.music.isPlaying)
                    {
                        // The trigger was activated on this frame. Prepare to handle it in the next frame.
                        if(TaleUtil.Triggers.GetImmediate("tale_music_stop"))
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
                    initialVolume = TaleUtil.Props.audio.music.volume;

                    state = State.FADE_OUT;

                    return false;
                }
                case State.FADE_OUT:
                {
                    clock += Time.deltaTime;

                    if(clock > stopDuration)
                        clock = stopDuration;

                    float interpolationFactor = interpolation(stopDuration == 0f ? 1f : clock / stopDuration);

                    TaleUtil.Props.audio.music.volume = TaleUtil.Math.Interpolate(initialVolume, 0f, interpolationFactor);

                    if(clock == stopDuration)
                    {
                        // Signal other music actions to stop. The trigger will be checked in the WAIT state.
                        // If the music stop action is immediately followed by a music play action, that action
                        // will enter the PLAY state, while this trigger is set. That action shouldn't stop, so
                        // don't check the trigger in the PLAY state.
                        TaleUtil.Triggers.Set("tale_music_stop");

                        TaleUtil.Props.audio.music.Stop();

                        Finish();

                        return true;
                    }

                    return false;
                }
            }

            return false;
        }
    }
}