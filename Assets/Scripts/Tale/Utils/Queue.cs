namespace TaleUtil {
    public class Queue : Collections.IRDeque<Action> {
        public Queue(int baseCapacity) : base(baseCapacity) { }

        public bool Run() {
            if (Count > 0 && Fetch().Execute()) {
                Dequeue();
            }

            return Count == 0;
        }
    }
}