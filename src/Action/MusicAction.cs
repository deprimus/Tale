using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class MusicAction : TaleUtil.Action
    {
        private enum State
        {
            PLAY,
            STOP,
            WAIT
        }

        private string path;
        private float volume;
        private float pitch;

        private State state;

        public MusicAction() { }

        public MusicAction(string path, float volume, float pitch)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.audio.music, "MusicAction requires a music object with an AudioSource component; did you forget to register it in TaleMaster?");
            TaleUtil.Assert.NotNull(TaleUtil.Props.audio.group, "MusicAction requires an audio group object; did you forget to register it in TaleMaster?");

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
            TaleUtil.Assert.NotNull(clip, "The music clip '" + path + "' is missing");

            return clip;
        }

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

        public override TaleUtil.Action Clone()
        {
            MusicAction clone = new MusicAction();
            clone.path = path;
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

                    TaleUtil.Props.audio.music.clip = LoadAudio();
                    TaleUtil.Props.audio.music.Play();

                    state = State.WAIT;

                    break;
                }
                case State.STOP:
                {
                    TaleUtil.Props.audio.music.Stop();

                    Finish();

                    return true;
                }
                case State.WAIT:
                {
                    if (!TaleUtil.Props.audio.music.isPlaying)
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