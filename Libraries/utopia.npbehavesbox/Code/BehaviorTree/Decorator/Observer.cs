using System;

namespace NPBehave
{
    public class Observer : Decorator
    {
        private System.Action _onStart;
        private Action<bool> _onStop;

        public Observer(System.Action onStart, Action<bool> onStop, Node decoratee) : base("Observer", decoratee)
        {
            _onStart = onStart;
            _onStop = onStop;
        }

        protected override void DoStart()
        {
            _onStart();
            Decoratee.Start();
        }

        protected override void DoStop()
        {
            Decoratee.Stop();
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            _onStop(result);
            Stopped(result);
        }
    }
}