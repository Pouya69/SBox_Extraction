using System.Collections.Generic;
using Sandbox;

namespace NPBehave
{
    public class Debugger : Component
    {
        public Root BehaviorTree;

        private static Blackboard _customGlobalStats = null;
        public static Blackboard CustomGlobalStats
        {
            get 
            {
                if (_customGlobalStats == null)
                {
                    _customGlobalStats = SandboxContext.GetSharedBlackboard("_GlobalStats");;
                }
                return _customGlobalStats;
            }
        }

        private Blackboard _customStats = null;
        public Blackboard CustomStats
        {
            get 
            {
                if (_customStats == null)
                {
                    _customStats = new Blackboard(CustomGlobalStats, SandboxContext.GetClock());
                }
                return _customStats;
            }
        }

        public void DebugCounterInc(string key)
        {
            if (!CustomStats.IsSet(key))
            {
                CustomStats[key] = 0;
            }
            CustomStats[key] = CustomStats.Get<int>(key) + 1;
        }

        public void DebugCounterDec(string key)
        {
            if (!CustomStats.IsSet(key))
            {
                CustomStats[key] = 0;
            }
            CustomStats[key] = CustomStats.Get<int>(key) - 1;
        }

        public static void GlobalDebugCounterInc(string key)
        {
            if (!CustomGlobalStats.IsSet(key))
            {
                CustomGlobalStats[key] = 0;
            }
            CustomGlobalStats[key] = CustomGlobalStats.Get<int>(key) + 1;
        }

        public static void GlobalDebugCounterDec(string key)
        {
            if (!CustomGlobalStats.IsSet(key))
            {
                CustomGlobalStats[key] = 0;
            }
            CustomGlobalStats[key] = CustomGlobalStats.Get<int>(key) - 1;
        }

    }
}
