using Sandbox.Diagnostics;

namespace NPBehave
{
    public class TimeMin : Decorator
    {
        private float _limit = 0.0f;
        private float _randomVariation;
        private bool _waitOnFailure = false;
        private bool _isLimitReached = false;
        private bool _isDecorateeDone = false;
        private bool _isDecorateeSuccess = false;

        public TimeMin(float limit, Node decoratee) : base("TimeMin", decoratee)
        {
            _limit = limit;
            _randomVariation = _limit * 0.05f;
            _waitOnFailure = false;
            Assert.True(limit > 0f, "limit has to be set");
        }

        public TimeMin(float limit, bool waitOnFailure, Node decoratee) : base("TimeMin", decoratee)
        {
            _limit = limit;
            _randomVariation = _limit * 0.05f;
            _waitOnFailure = waitOnFailure;
            Assert.True(limit > 0f, "limit has to be set");
        }

        public TimeMin(float limit, float randomVariation, bool waitOnFailure, Node decoratee) : base("TimeMin", decoratee)
        {
            _limit = limit;
            _randomVariation = randomVariation;
            _waitOnFailure = waitOnFailure;
            Assert.True(limit > 0f, "limit has to be set");
        }

        protected override void DoStart()
        {
            _isDecorateeDone = false;
            _isDecorateeSuccess = false;
            _isLimitReached = false;
            Clock.AddTimer(_limit, _randomVariation, 0, TimeoutReached);
            Decoratee.Start();
        }

        protected override void DoStop()
        {
            if (Decoratee.IsActive)
            {
                Clock.RemoveTimer(TimeoutReached);
                _isLimitReached = true;
                Decoratee.Stop();
            }
            else
            {
                Clock.RemoveTimer(TimeoutReached);
                Stopped(false);
            }
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            _isDecorateeDone = true;
            _isDecorateeSuccess = result;
            if (_isLimitReached || (!result && !_waitOnFailure))
            {
                Clock.RemoveTimer(TimeoutReached);
                Stopped(_isDecorateeSuccess);
            }
            else
            {
                Assert.True(Clock.HasTimer(TimeoutReached));
            }
        }

        private void TimeoutReached()
        {
            _isLimitReached = true;
            if (_isDecorateeDone)
            {
                Stopped(_isDecorateeSuccess);
            }
            else
            {
                Assert.True(Decoratee.IsActive);
            }
        }
    }
}
