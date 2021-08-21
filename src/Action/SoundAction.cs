using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class SoundAction : TaleUtil.Action
    {
        private enum State
        {
            PLAY,
            STOP,
            WAIT
        }

        public int channel;
        private string path;
        private float volume;
        private float pitch;

        private State state;

        private SoundAction() { }

        public SoundAction(int channel, string path, float volume, float pitch)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.audio.soundGroup, "SoundAction requires a sound group object; did you forget to register it in TaleMaster?");
            TaleUtil.Assert.NotNull(TaleUtil.Props.audio.group, "SoundAction requires an audio group object; did you forget to register it in TaleMaster?");
            TaleUtil.Assert.Condition(channel >= 0 && channel < TaleUtil.Props.audio.sound.Length, string.Format("Invalid sound channel '{0}'. Expected channel between '{1}' and '{2}' (inclusive)", channel, 0, TaleUtil.Props.audio.sound.Length - 1));
            TaleUtil.Assert.NotNull(TaleUtil.Props.audio.sound[channel], string.Format("Channel '{0}' does not have an audio source associated with it; did you forget to register it in TaleMaster?", channel));

            this.channel = channel;
            this.path = path;
            this.volume = volume;
            this.pitch = pitch;

            if(path != null)
                state = State.PLAY;
            else state = State.STOP;
        }

        private AudioClip LoadAudio()
        {
            AudioClip clip = Resources.Load<AudioClip>(path);
            TaleUtil.Assert.NotNull(clip, "The sound clip '" + path + "' is missing");

            return clip;
        }

        private void Finish()
        {
            TaleUtil.Props.audio.sound[channel].clip = null;

            TaleUtil.Action next = TaleUtil.Queue.FetchNext();

            // The next action isn't a SoundAction.
            if(!(TaleUtil.Queue.FetchNext() is SoundAction))
            {
                TaleUtil.Props.audio.sound[channel].gameObject.SetActive(false);

                // Check if all sound channels are inactive. If so, deactivate the sound group.
                bool areSoundChannelsInactive = true;

                for (int i = 0; i < TaleUtil.Props.audio.sound.Length; ++i)
                {
                    if(TaleUtil.Props.audio.sound[i] != null && TaleUtil.Props.audio.sound[i].gameObject.activeSelf)
                    {
                        areSoundChannelsInactive = false;
                        break;
                    }
                }

                if (areSoundChannelsInactive)
                {
                    // Deactivate the sound group.
                    TaleUtil.Props.audio.soundGroup.SetActive(false);

                    // Deactivate the audio group.
                    if ((TaleUtil.Props.audio.music == null || !TaleUtil.Props.audio.music.gameObject.activeSelf) && (TaleUtil.Props.audio.voice == null || !TaleUtil.Props.audio.voice.gameObject.activeSelf))
                        TaleUtil.Props.audio.group.SetActive(false);
                }
            }
            else if(((SoundAction)TaleUtil.Queue.FetchNext()).channel != channel)
            {
                // If this channel is not used in the next action, deactivate it.
                TaleUtil.Props.audio.sound[channel].gameObject.SetActive(false);
            }
        }

        public override TaleUtil.Action Clone()
        {
            SoundAction clone = new SoundAction();
            clone.channel = channel;
            clone.path = path;
            clone.volume = volume;
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
                    TaleUtil.Props.audio.soundGroup.SetActive(true);
                    TaleUtil.Props.audio.sound[channel].gameObject.SetActive(true);

                    TaleUtil.Props.audio.sound[channel].volume = volume;
                    TaleUtil.Props.audio.sound[channel].pitch = pitch;

                    TaleUtil.Props.audio.sound[channel].clip = LoadAudio();
                    TaleUtil.Props.audio.sound[channel].Play();

                    state = State.WAIT;

                    break;
                }
                case State.STOP:
                {
                    TaleUtil.Props.audio.sound[channel].Stop();

                    Finish();

                    return true;
                }
                case State.WAIT:
                {
                    if (!TaleUtil.Props.audio.sound[channel].isPlaying)
                    {
                        Finish();

                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}