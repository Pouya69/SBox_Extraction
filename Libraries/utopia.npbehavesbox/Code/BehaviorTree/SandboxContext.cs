using System.Collections.Generic;
using Sandbox;

namespace NPBehave
{
    public class SandboxContext : Component
    {
        private static SandboxContext _instance = null;

        private static SandboxContext GetInstance()
        {
            if (!_instance.IsValid())
            {
	            GameObject gameObject = Game.ActiveScene.CreateObject();
                gameObject.Name = "~Context";
                _instance = gameObject.Components.Create<SandboxContext>();
	            gameObject.Flags |= GameObjectFlags.NotSaved;
            }
            return _instance;
        }

        public static Clock GetClock()
        {
            return GetInstance()._clock;
        }

        public static Blackboard GetSharedBlackboard(string key)
        {
            SandboxContext context = GetInstance();
            if (!context._blackboards.ContainsKey(key))
            {
                context._blackboards.Add(key, new Blackboard(context._clock));
            }
            return context._blackboards[key];
        }

        private Dictionary<string, Blackboard> _blackboards = new Dictionary<string, Blackboard>();

        private Clock _clock = new Clock();


        protected override void OnUpdate()
        {
	        _clock.Update(Time.Delta);
	        base.OnUpdate();
        }
    }
}
