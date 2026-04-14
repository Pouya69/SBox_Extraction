using System.Collections.Generic;
using Sandbox;
using Sandbox.Diagnostics;

namespace NPBehave
{

    public class Clock
    {
        private readonly List<System.Action> _updateObservers = new List<System.Action>();
        private readonly Dictionary<System.Action, Timer> _timers = new Dictionary<System.Action, Timer>();
        private readonly HashSet<System.Action> _removeObservers = new HashSet<System.Action>();
        private readonly HashSet<System.Action> _addObservers = new HashSet<System.Action>();
        private readonly HashSet<System.Action> _removeTimers = new HashSet<System.Action>();
        private readonly Dictionary<System.Action, Timer> _addTimers = new Dictionary<System.Action, Timer>();
        private bool _isInUpdate = false;

        class Timer
        {
            public double ScheduledTime = 0f;
            public int Repeat = 0;
            public bool Used = false;
			public double Delay = 0f;
			public float RandomVariance = 0.0f;

			public void ScheduleAbsoluteTime(double elapsedTime)
			{
				ScheduledTime = elapsedTime + Delay - RandomVariance * 0.5f + RandomVariance * Game.Random.NextDouble();
			}
        }

        private TimeSince _elapsedTime = 0f;

        private readonly List<Timer> _timerPool = new List<Timer>();
        private int _currentTimerPoolIndex = 0;

        /// <summary>Register a timer function</summary>
        /// <param name="time">time in milliseconds</param>
        /// <param name="repeat">number of times to repeat, set to -1 to repeat until unregistered.</param>
        /// <param name="action">method to invoke</param>
        public void AddTimer(float time, int repeat, System.Action action)
        {
            AddTimer(time, 0f, repeat, action);
        }

        /// <summary>Register a timer function with random variance</summary>
        /// <param name="delay">time in milliseconds</param>
        /// <param name="randomVariance">deviate from time on a random basis</param>
        /// <param name="repeat">number of times to repeat, set to -1 to repeat until unregistered.</param>
        /// <param name="action">method to invoke</param>
        public void AddTimer(float delay, float randomVariance, int repeat, System.Action action)
        {
			Timer timer = null;

            if (!_isInUpdate)
            {
                if (!_timers.ContainsKey(action))
                {
					_timers[action] = GetTimerFromPool();
                }
				timer = _timers[action];
            }
            else
            {
                if (!_addTimers.TryGetValue( action, out Timer value ) )
                {
					value = GetTimerFromPool();
					_addTimers[action] = value;
                }
				timer = value;

                if (_removeTimers.Contains(action))
                {
                    _removeTimers.Remove(action);
                }
            }

			Assert.True(timer.Used);
			timer.Delay = delay;
			timer.RandomVariance = randomVariance;
			timer.Repeat = repeat;
			timer.ScheduleAbsoluteTime(_elapsedTime);
			
        }

        public void RemoveTimer(System.Action action)
        {
            if (!_isInUpdate)
            {
                if (_timers.TryGetValue( action, out Timer value ) )
                {
					value.Used = false;
                    _timers.Remove(action);
                }
            }
            else
            {
                if (_timers.ContainsKey(action))
                {
                    _removeTimers.Add(action);
                }
                if (_addTimers.TryGetValue( action, out Timer value ) )
                {
                    Assert.True( value.Used);
					value.Used = false;
                    _addTimers.Remove(action);
                }
            }
        }

        public bool HasTimer(System.Action action)
        {
            if (!_isInUpdate)
            {
                return _timers.ContainsKey(action);
            }
            else
            {
                if (_removeTimers.Contains(action))
                {
                    return false;
                }
                else if (_addTimers.ContainsKey(action))
                {
                    return true;
                }
                else
                {
                    return _timers.ContainsKey(action);
                }
            }
        }

        /// <summary>Register a function that is called every frame</summary>
        /// <param name="action">function to invoke</param>
        public void AddUpdateObserver(System.Action action)
        {
            if (!_isInUpdate)
            {
                _updateObservers.Add(action);
            }
            else
            {
                if (!_updateObservers.Contains(action))
                {
                    _addObservers.Add(action);
                }
                if (_removeObservers.Contains(action))
                {
                    _removeObservers.Remove(action);
                }
            }
        }

        public void RemoveUpdateObserver(System.Action action)
        {
            if (!_isInUpdate)
            {
                _updateObservers.Remove(action);
            }
            else
            {
                if (_updateObservers.Contains(action))
                {
                    _removeObservers.Add(action);
                }
                if (_addObservers.Contains(action))
                {
                    _addObservers.Remove(action);
                }
            }
        }

        public bool HasUpdateObserver(System.Action action)
        {
            if (!_isInUpdate)
            {
                return _updateObservers.Contains(action);
            }
            else
            {
                if (_removeObservers.Contains(action))
                {
                    return false;
                }
                else if (_addObservers.Contains(action))
                {
                    return true;
                }
                else
                {
                    return _updateObservers.Contains(action);
                }
            }
        }

        public void Update(float deltaTime)
        {
           // _elapsedTime += deltaTime;

            _isInUpdate = true;

            foreach (System.Action action in _updateObservers)
            {
                if (!_removeObservers.Contains(action))
                {
                    action.Invoke();
                }
            }

            Dictionary<System.Action, Timer>.KeyCollection keys = _timers.Keys;
			foreach (System.Action callback in keys)
            {
                if (_removeTimers.Contains(callback))
                {
                    continue;
                }

				Timer timer = _timers[callback];
                if (timer.ScheduledTime <= _elapsedTime)
                {
                    if (timer.Repeat == 0)
                    {
                        RemoveTimer(callback);
                    }
                    else if (timer.Repeat >= 0)
                    {
                        timer.Repeat--;
                    }
                    callback.Invoke();
					timer.ScheduleAbsoluteTime(_elapsedTime);
                }
            }

            foreach (System.Action action in _addObservers)
            {
                _updateObservers.Add(action);
            }
            foreach (System.Action action in _removeObservers)
            {
                _updateObservers.Remove(action);
            }
            foreach (System.Action action in _addTimers.Keys)
            {
                if (_timers.TryGetValue( action, out Timer value ) )
                {
                    Assert.AreNotEqual( value, _addTimers[action]);
					value.Used = false;
                }
                Assert.True(_addTimers[action].Used);
                _timers[action] = _addTimers[action];
            }
            foreach (System.Action action in _removeTimers)
            {
                Assert.True(_timers[action].Used);
                _timers[action].Used = false;
                _timers.Remove(action);
            }
            _addObservers.Clear();
            _removeObservers.Clear();
            _addTimers.Clear();
            _removeTimers.Clear();

            _isInUpdate = false;
        }

        public int NumUpdateObservers
        {
            get
            {
                return _updateObservers.Count;
            }
        }

        public int NumTimers
        {
            get
            {
                return _timers.Count;
            }
        }

        public double ElapsedTime
        {
            get
            {
                return _elapsedTime;
            }
        }

        private Timer GetTimerFromPool()
        {
            int i = 0;
            int l = _timerPool.Count;
            Timer timer = null;
            while (i < l)
            {
                int timerIndex = (i + _currentTimerPoolIndex) % l;
                if (!_timerPool[timerIndex].Used)
                {
                    _currentTimerPoolIndex = timerIndex;
                    timer = _timerPool[timerIndex];
                    break;
                }
                i++;
            }

            if (timer == null)
            {
                timer = new Timer();
                _currentTimerPoolIndex = 0;
                _timerPool.Add(timer);
            }

            timer.Used = true;
            return timer;
        }

        public int DebugPoolSize
        {
            get
            {
                return _timerPool.Count;
            }
        }
    }
}
