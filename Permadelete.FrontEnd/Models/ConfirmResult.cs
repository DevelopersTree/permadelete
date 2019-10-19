namespace Permadelete.Models
{
    public class ConfirmResult
    {
        public ConfirmResult(bool? result, int passes)
        {
            GoAhead = result;
            Passes = passes;
        }

        public bool? GoAhead { get; }
        public int Passes { get; }
    }
}
