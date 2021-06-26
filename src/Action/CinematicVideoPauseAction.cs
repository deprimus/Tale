using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class CinematicVideoPauseAction : TaleUtil.Action
    {

        public CinematicVideoPauseAction() {
            TaleUtil.Assert.NotNull(TaleUtil.Props.cinematic.video.player, "CinematicVideoPauseAction requires a video player object; did you forget to register it in TaleMaster?");
        }

        public override TaleUtil.Action Clone()
        {
            CinematicVideoPauseAction clone = new CinematicVideoPauseAction();

            return clone;
        }

        public override bool Run()
        {
            TaleUtil.Props.cinematic.video.player.Pause();
            return true;
        }
    }
}