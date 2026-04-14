namespace NPBehave
{
    public class WaitUntilStopped : Task
    {
        private bool _sucessWhenStopped;
        public WaitUntilStopped(bool sucessWhenStopped = false) : base("WaitUntilStopped")
        {
            _sucessWhenStopped = sucessWhenStopped;
        }

        protected override void DoStop()
        {
            Stopped(_sucessWhenStopped);
        }
    }
}