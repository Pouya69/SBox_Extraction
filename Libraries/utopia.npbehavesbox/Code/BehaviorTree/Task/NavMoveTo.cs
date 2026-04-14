using System;
using Sandbox;

namespace NPBehave
{
    public class NavMoveTo : Task
    {
        private const float DestinationChangeThreshold = 0.0001f;
        private const uint DestinationChangeMaxChecks = 100;
        
        private NavMeshAgent _agent;
        private string _blackboardKey;
        private float _tolerance;
        private bool _stopOnTolerance;
        private float _updateFrequency;
        private float _updateVariance;

        private Vector3 _lastDestination;
        private float _lastDistance;
        private uint _failedChecks;

        /// CAUTION: EXPERIMENTAL !!!!
        /// <param name="agent">target to move</param>
        /// <param name="blackboardKey">blackboard key containing either a Transform or a Vector.</param>
        /// <param name="tolerance">acceptable tolerance</param>
        /// <param name="stopOnTolerance">should stop when in tolerance</param>
        /// <param name="updateFrequency">frequency to check for changes of reaching the destination or a Transform's location</param>
        /// <param name="updateVariance">random variance for updateFrequency</param>

#if UNITY_5_3 || UNITY_5_4
        public NavMoveTo(NavMeshAgent agent, string blackboardKey, float tolerance = 1.0f, bool stopOnTolerance = false, float updateFrequency = 0.1f, float updateVariance = 0.025f) : base("NavMoveTo")
#else
        public NavMoveTo(NavMeshAgent agent, string blackboardKey, float tolerance = 1.0f, bool stopOnTolerance = false, float updateFrequency = 0.1f, float updateVariance = 0.025f) : base("NavMoveTo")
#endif
        {
            _agent = agent;
            _blackboardKey = blackboardKey;
            _tolerance = tolerance;
            _stopOnTolerance = stopOnTolerance;
            _updateFrequency = updateFrequency;
            _updateVariance = updateVariance;
        }

        protected override void DoStart()
        {
            _lastDestination = Vector3.Zero;
            _lastDistance = 99999999.0f;
            _failedChecks = 0;

            Blackboard.AddObserver(_blackboardKey, OnBlackboardValueChanged);
            Clock.AddTimer(_updateFrequency, _updateVariance, -1, OnUpdateTimer);

            MoveToBlackboardKey();
        }

        protected override void DoStop()
        {
            StopAndCleanUp(false);
        }

        private void OnBlackboardValueChanged(Blackboard.Type type, object newValue)
        {
            MoveToBlackboardKey();
        }

        private void OnUpdateTimer()
        {
            MoveToBlackboardKey();
        }

        private void MoveToBlackboardKey()
        {
            object target = Blackboard.Get(_blackboardKey);
            if (target == null)
            {
                StopAndCleanUp(false);
                return;
            }

            // get target location
            Vector3 destination = Vector3.Zero;
            if (target is Transform transform)
            {
                if (_updateFrequency >= 0.0f)
                {
                    destination = transform.Position;
                }
            }
            else if (target is Vector3 vector3)
            {
                destination = vector3;
            }
            else if (target is GameObject gameObject)
            {
	            destination = gameObject.Transform.Position;
            }
            else
            {
                Log.Warning(
	                $"NavMoveTo: Blackboard Key '{_blackboardKey}' contained unsupported type '{target.GetType()}" );
                StopAndCleanUp(false);
                return;
            }

            // set new destination
            if(_agent.TargetPosition == null || !_agent.TargetPosition.Value.AlmostEqual( destination ) )
	            _agent.MoveTo(destination);

            float sqrDistLeft = (destination - _agent.AgentPosition).LengthSquared;
            
            bool destinationChanged = (_agent.TargetPosition ?? Vector3.Zero - _lastDestination).LengthSquared > (DestinationChangeThreshold * DestinationChangeThreshold); //(destination - agent.destination).sqrMagnitude > (DESTINATION_CHANGE_THRESHOLD * DESTINATION_CHANGE_THRESHOLD);
            bool distanceChanged = MathF.Abs(sqrDistLeft) > DestinationChangeThreshold;

            // check if we are already at our goal and stop the task
            if (_lastDistance < _tolerance)
            {
                if (_stopOnTolerance || (!destinationChanged && !distanceChanged))
                {
                    // reached the goal
                    StopAndCleanUp(true);
                    return;
                }
            }
            else if (!destinationChanged && !distanceChanged)
            {
                if (_failedChecks++ > DestinationChangeMaxChecks)
                {
                    // could not reach the goal for whatever reason
                    StopAndCleanUp(false);
                    return;
                }
            }
            else
            {
                _failedChecks = 0;
            }

            _lastDestination = _agent.TargetPosition ?? Vector3.Zero; 

            // Workaround for lastDistance set to 0 https://github.com/meniku/NPBehave/issues/33
            if (_agent.TargetPosition == null)
            {
                _lastDistance = 99999999.0f;
            }
            else
            {
                _lastDistance = sqrDistLeft;
            }
        }

        private void StopAndCleanUp(bool result)
        {
            _agent.Stop();
            Blackboard.RemoveObserver(_blackboardKey, OnBlackboardValueChanged);
            Clock.RemoveTimer(OnUpdateTimer);
            Stopped(result);
        }
    }
}
