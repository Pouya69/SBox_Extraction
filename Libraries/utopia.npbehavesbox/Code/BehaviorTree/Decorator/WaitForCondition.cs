using System;
using Sandbox.Diagnostics;

namespace NPBehave
{
    public class WaitForCondition : Decorator
    {
        private Func<bool> _condition;
        private float _checkInterval;
        private float _checkVariance;

        public WaitForCondition(Func<bool> condition, float checkInterval, float randomVariance, Node decoratee) : base("WaitForCondition", decoratee)
        {
            _condition = condition;

            _checkInterval = checkInterval;
            _checkVariance = randomVariance;

            Label = $"{(checkInterval - randomVariance)}...{(checkInterval + randomVariance)}s";
        }

        public WaitForCondition(Func<bool> condition, Node decoratee) : base("WaitForCondition", decoratee)
        {
            _condition = condition;
            _checkInterval = 0.0f;
            _checkVariance = 0.0f;
            Label = "every tick";
        }

        protected override void DoStart()
        {
            if (!_condition.Invoke())
            {
                Clock.AddTimer(_checkInterval, _checkVariance, -1, CheckCondition);
            }
            else
            {
                Decoratee.Start();
            }
        }

        private void CheckCondition()
        {
            if (_condition.Invoke())
            {
                Clock.RemoveTimer(CheckCondition);
                Decoratee.Start();
            }
        }

        protected override void DoStop()
        {
            Clock.RemoveTimer(CheckCondition);
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
            Assert.AreNotEqual(((Node)this).CurrentState, State.Inactive);
            Stopped(result);
        }
    }
}
