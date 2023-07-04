using UnityEngine;

namespace TaleUtil
{
    public class CinematicToggleAction : Action
    {

        public CinematicToggleAction() {
            Assert.Condition(Props.cinematic.canvas != null, "CinematicToggleAction requires a canvas object; did you forget to register it in TaleMaster?");
        }

        public override Action Clone()
        {
            CinematicToggleAction clone = new CinematicToggleAction();
            clone.delta = delta;

            return clone;
        }

        public override bool Run()
        {
            if(Props.cinematic.canvas.activeSelf)
            {
                // Make the background image black.
                if(Props.cinematic.background.GetActiveImage() != null) 
                    Props.cinematic.background.GetActiveImage().color = new Color32(0, 0, 0, 255);

                // Stop the video and forget the current clip.
                if(Props.cinematic.video.player != null && Props.cinematic.video.group != null)
                {
                    Props.cinematic.video.player.Stop();
                    Props.cinematic.video.player.clip = null;
                    Props.cinematic.video.group.SetActive(false);
                }

                Props.cinematic.canvas.SetActive(false);
            }
            else
            {
                Props.cinematic.canvas.SetActive(true);
            }

            return true;
        }

        public override string ToString()
        {
            return "CinematicToggleAction";
        }
    }
}