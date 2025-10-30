using UnityEngine;

namespace TaleUtil
{
    public class CinematicToggleAction : Action
    {
        public CinematicToggleAction Init() {
            Assert.Condition(master.Props.cinematic.canvas != null, "CinematicToggleAction requires a canvas object; did you forget to register it in TaleMaster?");

            return this;
        }

        public override bool Run()
        {
            if(master.Props.cinematic.canvas.activeSelf)
            {
                // Make the background image black.
                if(master.Props.cinematic.background.GetActiveImage() != null) 
                    master.Props.cinematic.background.GetActiveImage().color = new Color32(0, 0, 0, 255);

                // Stop the video and forget the current clip.
                if(master.Props.cinematic.video.player != null && master.Props.cinematic.video.group != null)
                {
                    master.Props.cinematic.video.player.Stop();
                    master.Props.cinematic.video.player.clip = null;
                    master.Props.cinematic.video.group.SetActive(false);
                }

                master.Props.cinematic.canvas.SetActive(false);
            }
            else
            {
                master.Props.cinematic.canvas.SetActive(true);
            }

            return true;
        }

        public override string ToString()
        {
            return "CinematicToggleAction";
        }
    }
}