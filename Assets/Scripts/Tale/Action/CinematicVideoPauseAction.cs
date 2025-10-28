namespace TaleUtil
{
    public class CinematicVideoPauseAction : Action
    {

        public CinematicVideoPauseAction Init() {
            Assert.Condition(Props.cinematic.video.player != null, "CinematicVideoPauseAction requires a video player object; did you forget to register it in TaleMaster?");

            return this;
        }

        public override bool Run()
        {
            Props.cinematic.video.player.Pause();
            return true;
        }

        public override string ToString()
        {
            return "CinematicVideoPauseAction";
        }
    }
}