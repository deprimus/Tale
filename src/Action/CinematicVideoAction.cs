#pragma warning disable 0162 // Disable the 'unreachable code' warning caused by config constants.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

namespace TaleUtil
{
    public class CinematicVideoAction : TaleUtil.Action
    {
        public enum DetatchType
        {
            BEFORE,
            AFTER,
            FIXED
        }

        private enum State
        {
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

        private string path;
        private float speed;
        private float detatchTime;
        private DetatchType detatchType;

        private State state;

        private double startTime; // VideoPlayer.time is of type double.

        private CinematicVideoAction() { }

        public CinematicVideoAction(string path, float detatchTime, DetatchType detatchType, float speed)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.cinematic.video.group, "CinematicVideoAction requires a group object; did you forget to register it in TaleMaster?");
            TaleUtil.Assert.NotNull(TaleUtil.Props.cinematic.video.player, "CinematicVideoAction requires a video player object; did you forget to register it in TaleMaster?");

            this.path = path;
            this.detatchTime = detatchTime;
            this.detatchType = detatchType;
            this.speed = speed;

            this.startTime = 0f;

            if(path == null)
                state = State.PLAY;
            else state = State.LOAD;
        }

        public override TaleUtil.Action Clone()
        {
            CinematicVideoAction clone = new CinematicVideoAction();
            clone.path = path;
            clone.detatchTime = detatchTime;
            clone.detatchType = detatchType;
            clone.state = state;
            clone.speed = speed;
            clone.startTime = startTime;

            return clone;
        }

        private VideoClip LoadVideo()
        {
            VideoClip clip = Resources.Load<VideoClip>(path);
            TaleUtil.Assert.NotNull(clip, "The cinematic video '" + clip + "' is missing");

            return clip;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.LOAD:
                {
                    TaleUtil.Props.cinematic.video.group.SetActive(true);

                    TaleUtil.Props.cinematic.video.player.playbackSpeed = speed;
                    TaleUtil.Props.cinematic.video.player.EnableAudioTrack(0, true); // Audio must be set up before Prepare.
                    TaleUtil.Props.cinematic.video.player.SetTargetAudioSource(0, TaleUtil.Props.cinematic.video.audio);

                    TaleUtil.Props.cinematic.video.player.clip = LoadVideo();
                    TaleUtil.Props.cinematic.video.player.Prepare();

                    state = State.WAIT_FOR_PREPARATION;

                    break;
                }
                case State.WAIT_FOR_PREPARATION:
                {
                    if(TaleUtil.Props.cinematic.video.player.isPrepared)
                    {
                        state = State.PLAY;
                    }

                    break;
                }
                case State.PLAY:
                {
                    TaleUtil.Props.cinematic.video.player.Play();

                    if(detatchTime < 0f)
                        return true;

                    switch (detatchType)
                    {
                        case DetatchType.BEFORE:
                            state = State.WAIT_WITH_DETATCH_BEFORE;
                            break;
                        case DetatchType.AFTER:
                            state = State.WAIT_WITH_DETATCH_AFTER;
                            startTime = TaleUtil.Props.cinematic.video.player.time;
                            break;
                        case DetatchType.FIXED:
                            state = State.WAIT_WITH_DETATCH_FIXED;
                            break;
                    }

                    break;
                }
                case State.WAIT_WITH_DETATCH_BEFORE:
                {
                    // Less than (or equal to) detatchTime seconds left, or the video stopped by reaching the end.
                    if (TaleUtil.Props.cinematic.video.player.time >= TaleUtil.Props.cinematic.video.player.length - detatchTime * TaleUtil.Props.cinematic.video.player.playbackSpeed || (!TaleUtil.Props.cinematic.video.player.isPlaying))
                    {
                        return true;
                    }

                    break;
                }
                case State.WAIT_WITH_DETATCH_AFTER:
                {
                    // At least detatchTime seconds passed, or the video stopped by reaching the end.
                    if (TaleUtil.Props.cinematic.video.player.time - startTime >= detatchTime * TaleUtil.Props.cinematic.video.player.playbackSpeed || (!TaleUtil.Props.cinematic.video.player.isPlaying))
                    {
                        return true;
                    }

                    break;
                }
                case State.WAIT_WITH_DETATCH_FIXED:
                {
                    // Time reached, or the video stopped by reaching the end.
                    if(TaleUtil.Props.cinematic.video.player.time >= detatchTime || (!TaleUtil.Props.cinematic.video.player.isPlaying))
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}