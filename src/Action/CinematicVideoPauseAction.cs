using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class CinematicVideoPauseAction : Action
    {

        public CinematicVideoPauseAction() {
            Assert.Condition(Props.cinematic.video.player != null, "CinematicVideoPauseAction requires a video player object; did you forget to register it in TaleMaster?");
        }

        public override Action Clone()
        {
            CinematicVideoPauseAction clone = new CinematicVideoPauseAction();

            return clone;
        }

        public override bool Run()
        {
            Props.cinematic.video.player.Pause();
            return true;
        }
    }
}