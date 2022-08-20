namespace TaleUtil
{
    public class ParallelStopAction : Action
    {
        Parallel.Pointer ptr;

        ParallelStopAction() { }

        public ParallelStopAction(Parallel.Pointer ptr)
        {
            this.ptr = ptr;
        }

        public override Action Clone()
        {
            ParallelStopAction clone = new ParallelStopAction();
            clone.ptr = new Parallel.Pointer(ptr.start, ptr.size);

            return clone;
        }

        public override bool Run()
        {
            ptr.Stop();
            return true;
        }
    }
}