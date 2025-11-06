namespace TaleUtil {
    public class CinematicVideoPauseAction : Action {

        public CinematicVideoPauseAction Init() {
            Assert.Condition(master.Props.cinematic.video.player != null, "CinematicVideoPauseAction requires a video player object; did you forget to register it in TaleMaster?");

            return this;
        }

        protected override bool Run() {
            master.Props.cinematic.video.player.Pause();
            return true;
        }

        public override string ToString() {
            return "CinematicVideoPauseAction";
        }
    }
}