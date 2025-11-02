namespace TaleUtil {
    public class Parallel : Collections.FastUnorderedList<Action> {
        public Parallel(int baseCapacity) : base(baseCapacity) { }

        public void Run() {
            for (int i = 0; i < Count;) {
                if (this[i].Run()) {
                    Remove(i);
                } else {
                    ++i;
                }
            }
        }
    }
}