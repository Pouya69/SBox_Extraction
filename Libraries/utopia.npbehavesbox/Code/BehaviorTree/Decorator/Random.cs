using Sandbox;

namespace NPBehave
{
    public class Random : Decorator
    {
        private float _probability;

        public Random(float probability, Node decoratee) : base("Random", decoratee)
        {
            _probability = probability;
        }

        protected override void DoStart()
        {
            if (Game.Random.Float() <= _probability)
            {
                Decoratee.Start();
            }
            else
            {
                Stopped(false);
            }
        }

        protected override void DoStop()
        {
            Decoratee.Stop();
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            Stopped(result);
        }
    }
}
