using Sandbox.Diagnostics;

namespace NPBehave
{
    public class Wait : Task
    {
        private System.Func<float> _function = null;
        private string _blackboardKey = null;
        private float _seconds = -1f;
        private float _randomVariance;

        public float RandomVariance
        {
            get
            {
                return _randomVariance;
            }
            set
            {
                _randomVariance = value;
            }
        }

        public Wait(float seconds, float randomVariance) : base("Wait")
        {
            Assert.True(seconds >= 0);
            _seconds = seconds;
            _randomVariance = randomVariance;
        }

        public Wait(float seconds) : base("Wait")
        {
            _seconds = seconds;
            _randomVariance = _seconds * 0.05f;
        }

        public Wait(string blackboardKey, float randomVariance = 0f) : base("Wait")
        {
            _blackboardKey = blackboardKey;
            _randomVariance = randomVariance;
        }

        public Wait(System.Func<float> function, float randomVariance = 0f) : base("Wait")
        {
            _function = function;
            _randomVariance = randomVariance;
        }

        protected override void DoStart()
        {
            float seconds = _seconds;
            if (seconds < 0)
            {
                if (_blackboardKey != null)
                {
                    seconds = Blackboard.Get<float>(_blackboardKey);
                }
                else if (_function != null)
                {
                    seconds = _function();
                }
            }
//            UnityEngine.Assertions.Assert.True(seconds >= 0);
            if (seconds < 0)
            {
                seconds = 0;
            }

            if (_randomVariance >= 0f)
            {
                Clock.AddTimer(seconds, _randomVariance, 0, OnTimer);
            }
            else
            {
                Clock.AddTimer(seconds, 0, OnTimer);
            }
        }

        protected override void DoStop()
        {
            Clock.RemoveTimer(OnTimer);
            Stopped(false);
        }

        private void OnTimer()
        {
            Clock.RemoveTimer(OnTimer);
            Stopped(true);
        }
    }
}
