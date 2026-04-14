using System;

namespace NPBehave
{
    public class Condition : ObservingDecorator
    {
        private Func<bool> _condition;
        private float _checkInterval;
        private float _checkVariance;

        public Condition(Func<bool> condition, Node decoratee) : base("Condition", Stops.None, decoratee)
        {
            _condition = condition;
            _checkInterval = 0.0f;
            _checkVariance = 0.0f;
        }

        public Condition(Func<bool> condition, Stops stopsOnChange, Node decoratee) : base("Condition", stopsOnChange, decoratee)
        {
            _condition = condition;
            _checkInterval = 0.0f;
            _checkVariance = 0.0f;
        }

        public Condition(Func<bool> condition, Stops stopsOnChange, float checkInterval, float randomVariance, Node decoratee) : base("Condition", stopsOnChange, decoratee)
        {
            _condition = condition;
            _checkInterval = checkInterval;
            _checkVariance = randomVariance;
        }

        protected override void StartObserving()
        {
            RootNode.Clock.AddTimer(_checkInterval, _checkVariance, -1, Evaluate);
        }

        protected override void StopObserving()
        {
            RootNode.Clock.RemoveTimer(Evaluate);
        }

        protected override bool IsConditionMet()
        {
            return _condition();
        }
    }
}