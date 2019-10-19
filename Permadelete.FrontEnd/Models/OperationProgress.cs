namespace Permadelete.Models
{
    public class OperationProgress
    {
        public OperationProgress(long newBytes, int currentPass)
        {
            NewBytes = newBytes;
            CurrentPass = currentPass;
        }

        public long NewBytes { get; }
        public int CurrentPass { get; }
    }
}
