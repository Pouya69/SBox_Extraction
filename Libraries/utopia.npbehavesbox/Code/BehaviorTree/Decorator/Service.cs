namespace NPBehave
{
    public class Service : Decorator
    {
        private System.Action _serviceMethod;

        private float _interval = -1.0f;
        private float _randomVariation;

        public Service(float interval, float randomVariation, System.Action service, Node decoratee) : base("Service", decoratee)
        {
            _serviceMethod = service;
            _interval = interval;
            _randomVariation = randomVariation;

            Label = $"{(interval - randomVariation)}...{(interval + randomVariation)}s";
        }

        public Service(float interval, System.Action service, Node decoratee) : base("Service", decoratee)
        {
            _serviceMethod = service;
            _interval = interval;
            _randomVariation = interval * 0.05f;
            Label = $"{(interval - _randomVariation)}...{(interval + _randomVariation)}s";
        }

        public Service(System.Action service, Node decoratee) : base("Service", decoratee)
        {
            _serviceMethod = service;
            Label = "every tick";
        }

        protected override void DoStart()
        {
            if (_interval <= 0f)
            {
                Clock.AddUpdateObserver(_serviceMethod);
                _serviceMethod();
            }
            else if (_randomVariation <= 0f)
            {
                Clock.AddTimer(_interval, -1, _serviceMethod);
                _serviceMethod();
            }
            else
            {
                InvokeServiceMethodWithRandomVariation();
            }
            Decoratee.Start();
        }

        protected override void DoStop()
        {
            Decoratee.Stop();
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            if (_interval <= 0f)
            {
                Clock.RemoveUpdateObserver(_serviceMethod);
            }
            else if (_randomVariation <= 0f)
            {
                Clock.RemoveTimer(_serviceMethod);
            }
            else
            {
                Clock.RemoveTimer(InvokeServiceMethodWithRandomVariation);
            }
            Stopped(result);
        }

        private void InvokeServiceMethodWithRandomVariation()
        {
            _serviceMethod();
            Clock.AddTimer(_interval, _randomVariation, 0, InvokeServiceMethodWithRandomVariation);
        }
    }
}
