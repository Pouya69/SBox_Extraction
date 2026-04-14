namespace NPBehave
{
    public class Repeater : Decorator
    {
        private int _loopCount = -1;
        private int _currentLoop;

        /// <param name="loopCount">number of times to execute the decoratee. Set to -1 to repeat forever, be careful with endless loops!</param>
        /// <param name="decoratee">Decorated Node</param>
        public Repeater(int loopCount, Node decoratee) : base("Repeater", decoratee)
        {
            _loopCount = loopCount;
        }

        /// <param name="decoratee">Decorated Node, repeated forever</param>
        public Repeater(Node decoratee) : base("Repeater", decoratee)
        {
        }

        protected override void DoStart()
        {
            if (_loopCount != 0)
            {
                _currentLoop = 0;
                Decoratee.Start();
            }
            else
            {
                Stopped(true);
            }
        }

        protected override void DoStop()
        {
            Clock.RemoveTimer(RestartDecoratee);
            
            if (Decoratee.IsActive)
            {
                Decoratee.Stop();
            }
            else
            {
                Stopped(false);
            }
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            if (result)
            {
                if (IsStopRequested || (_loopCount > 0 && ++_currentLoop >= _loopCount))
                {
                    Stopped(true);
                }
                else
                {
                    Clock.AddTimer(0, 0, RestartDecoratee);
                }
            }
            else
            {
                Stopped(false);
            }
        }

        protected void RestartDecoratee()
        {
            Decoratee.Start();
        }
    }
}