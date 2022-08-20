using UnityEngine;

namespace TaleUtil
{
    public class SoundAction : Action
    {
        enum State
        {
            PLAY,
            STOP,
            WAIT
        }

        public int channel;
        string path;
        float volume;
        float pitch;

        State state;

        SoundAction() { }

        public SoundAction(int channel, string path, float volume, float pitch)
        {
            Assert.Condition(Props.audio.soundGroup != null, "SoundAction requires a sound group object; did you forget to register it in TaleMaster?");
            Assert.Condition(Props.audio.group != null, "SoundAction requires an audio group object; did you forget to register it in TaleMaster?");
            Assert.Condition(channel >= 0 && channel < Props.audio.sound.Length, string.Format("Invalid sound channel '{0}'. Expected channel between '{1}' and '{2}' (inclusive)", channel, 0, Props.audio.sound.Length - 1));
            Assert.Condition(Props.audio.sound[channel] != null, string.Format("Channel '{0}' does not have an audio source associated with it; did you forget to register it in TaleMaster?", channel));

            this.channel = channel;
            this.path = path;
            this.volume = volume;
            this.pitch = pitch;

            if(path != null)
                state = State.PLAY;
            else state = State.STOP;
        }

        AudioClip LoadAudio()
        {
            AudioClip clip = Resources.Load<AudioClip>(path);
            Assert.Condition(clip != null, "The sound clip '" + path + "' is missing");

            return clip;
        }

        void Finish()
        {
            Props.audio.sound[channel].clip = null;

            Action next = Queue.FetchNext();

            // The next action isn't a SoundAction.
            if(!(Queue.FetchNext() is SoundAction))
            {
                Props.audio.sound[channel].gameObject.SetActive(false);

                // Check if all sound channels are inactive. If so, deactivate the sound group.
                bool areSoundChannelsInactive = true;

                for (int i = 0; i < Props.audio.sound.Length; ++i)
                {
                    if(Props.audio.sound[i] != null && Props.audio.sound[i].gameObject.activeSelf)
                    {
                        areSoundChannelsInactive = false;
                        break;
                    }
                }

                if (areSoundChannelsInactive)
                {
                    // Deactivate the sound group.
                    Props.audio.soundGroup.SetActive(false);

                    // Deactivate the audio group.
                    if ((Props.audio.music == null || !Props.audio.music.gameObject.activeSelf) && (Props.audio.voice == null || !Props.audio.voice.gameObject.activeSelf))
                        Props.audio.group.SetActive(false);
                }
            }
            else if(((SoundAction)Queue.FetchNext()).channel != channel)
            {
                // If this channel is not used in the next action, deactivate it.
                Props.audio.sound[channel].gameObject.SetActive(false);
            }
        }

        public override Action Clone()
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
                    Props.audio.group.SetActive(true);
                    Props.audio.soundGroup.SetActive(true);
                    Props.audio.sound[channel].gameObject.SetActive(true);

                    Props.audio.sound[channel].volume = volume;
                    Props.audio.sound[channel].pitch = pitch;

                    Props.audio.sound[channel].clip = LoadAudio();
                    Props.audio.sound[channel].Play();

                    state = State.WAIT;

                    break;
                }
                case State.STOP:
                {
                    Props.audio.sound[channel].Stop();

                    Finish();

                    return true;
                }
                case State.WAIT:
                {
                    if (!Props.audio.sound[channel].isPlaying)
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