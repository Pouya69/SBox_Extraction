using Sandbox.Diagnostics;

namespace NPBehave
{
    public class TimeMax : Decorator
    {
        private float _limit = 0.0f;
        private float _randomVariation;
        private bool _waitForChildButFailOnLimitReached = false;
        private bool _isLimitReached = false;

        public TimeMax(float limit, bool waitForChildButFailOnLimitReached, Node decoratee) : base("TimeMax", decoratee)
        {
            _limit = limit;
            _randomVariation = limit * 0.05f;
            _waitForChildButFailOnLimitReached = waitForChildButFailOnLimitReached;
            Assert.True(limit > 0f, "limit has to be set");
        }

        public TimeMax(float limit, float randomVariation, bool waitForChildButFailOnLimitReached, Node decoratee) : base("TimeMax", decoratee)
        {
            _limit = limit;
            _randomVariation = randomVariation;
            _waitForChildButFailOnLimitReached = waitForChildButFailOnLimitReached;
            Assert.True(limit > 0f, "limit has to be set");
        }

        protected override void DoStart()
        {
            _isLimitReached = false;
            Clock.AddTimer(_limit, _randomVariation, 0, TimeoutReached);
            Decoratee.Start();
        }

        protected override void DoStop()
        {
            Clock.RemoveTimer(TimeoutReached);
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
            Clock.RemoveTimer(TimeoutReached);
            if (_isLimitReached)
            {
                Stopped(false);
            }
            else
            {
                Stopped(result);
            }
        }

        private void TimeoutReached()
        {
            if (!_waitForChildButFailOnLimitReached)
            {
                Decoratee.Stop();
            }
            else
            {
                _isLimitReached = true;
                Assert.True(Decoratee.IsActive);
            }
        }
    }
}
