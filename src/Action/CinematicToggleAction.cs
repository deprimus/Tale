using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class CinematicToggleAction : TaleUtil.Action
    {

        public CinematicToggleAction() {
            TaleUtil.Assert.NotNull(TaleUtil.Props.cinematic.canvas, "CinematicToggleAction requires a canvas object; did you forget to register it in TaleMaster?");
        }

        public override TaleUtil.Action Clone()
        {
            CinematicToggleAction clone = new CinematicToggleAction();

            return clone;
        }

        public override bool Run()
        {
            if(TaleUtil.Props.cinematic.canvas.activeSelf)
            {
                // Make the background image black.
                if(TaleUtil.Props.cinematic.background.GetActiveImage() != null) 
                    TaleUtil.Props.cinematic.background.GetActiveImage().color = new Color32(0, 0, 0, 255);

                // Stop the video and forget the current clip.
                if(TaleUtil.Props.cinematic.video.player != null && TaleUtil.Props.cinematic.video.group != null)
                {
                    TaleUtil.Props.cinematic.video.player.Stop();
                    TaleUtil.Props.cinematic.video.player.clip = null;
                    TaleUtil.Props.cinematic.video.group.SetActive(false);
                }

                TaleUtil.Props.cinematic.canvas.SetActive(false);
            }
            else
            {
                TaleUtil.Props.cinematic.canvas.SetActive(true);
            }

            return true;
        }
    }
}