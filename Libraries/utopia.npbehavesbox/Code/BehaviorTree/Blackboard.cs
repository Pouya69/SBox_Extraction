using System.Collections.Generic;

namespace NPBehave
{
    public class Blackboard
    {
        public enum Type
        {
            Add,
            Remove,
            Change
        }
        private struct Notification
        {
            public string Key;
            public Type Type;
            public object Value;
            public Notification(string key, Type type, object value)
            {
                Key = key;
                Type = type;
                Value = value;
            }
        }

        private Clock _clock;
        private Dictionary<string, object> _data = new Dictionary<string, object>();
        private Dictionary<string, List<System.Action<Type, object>>> _observers = new Dictionary<string, List<System.Action<Type, object>>>();
        private bool _isNotifiyng = false;
        private Dictionary<string, List<System.Action<Type, object>>> _addObservers = new Dictionary<string, List<System.Action<Type, object>>>();
        private Dictionary<string, List<System.Action<Type, object>>> _removeObservers = new Dictionary<string, List<System.Action<Type, object>>>();
        private List<Notification> _notifications = new List<Notification>();
        private List<Notification> _notificationsDispatch = new List<Notification>();
        private Blackboard _parentBlackboard;
        private HashSet<Blackboard> _children = new HashSet<Blackboard>();

        public Blackboard(Blackboard parent, Clock clock)
        {
            _clock = clock;
            _parentBlackboard = parent;
        }
        public Blackboard(Clock clock)
        {
            _parentBlackboard = null;
            _clock = clock;
        }

        public void Enable()
        {
	        _parentBlackboard?._children.Add(this);
        }

        public void Disable()
        {
	        _parentBlackboard?._children.Remove(this);
            if (_clock != null)
            {
                _clock.RemoveTimer(NotifiyObservers);
            }
        }

        public object this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }

        public void Set(string key)
        {
            if (!IsSet(key))
            {
                Set(key, null);
            }
        }

        public void Set(string key, object value)
        {
            if (_parentBlackboard != null && _parentBlackboard.IsSet(key))
            {
                _parentBlackboard.Set(key, value);
            }
            else
            {
                if (_data.TryAdd(key, value))
                {
	                _notifications.Add(new Notification(key, Type.Add, value));
                    _clock.AddTimer(0f, 0, NotifiyObservers);
                }
                else
                {
                    if ((_data[key] == null && value != null) || (_data[key] != null && !_data[key].Equals(value)))
                    {
                        _data[key] = value;
                        _notifications.Add(new Notification(key, Type.Change, value));
                        _clock.AddTimer(0f, 0, NotifiyObservers);
                    }
                }
            }
        }

        public void Unset(string key)
        {
            if (_data.ContainsKey(key))
            {
                _data.Remove(key);
                _notifications.Add(new Notification(key, Type.Remove, null));
                _clock.AddTimer(0f, 0, NotifiyObservers);
            }
        }

        public T Get<T>(string key)
        {
            object result = Get(key);
            if (result == null)
            {
                return default(T);
            }
            return (T)result;
        }

        public object Get(string key)
        {
	        return _data.TryGetValue(key, out var value) ? value : _parentBlackboard?.Get(key);
        }

        public bool IsSet(string key)
        {
            return _data.ContainsKey(key) || (_parentBlackboard != null && _parentBlackboard.IsSet(key));
        }

        public void AddObserver(string key, System.Action<Type, object> observer)
        {
            List<System.Action<Type, object>> observers = GetObserverList(_observers, key);
            if (!_isNotifiyng)
            {
                if (!observers.Contains(observer))
                {
                    observers.Add(observer);
                }
            }
            else
            {
                if (!observers.Contains(observer))
                {
                    List<System.Action<Type, object>> addObservers = GetObserverList(_addObservers, key);
                    if (!addObservers.Contains(observer))
                    {
                        addObservers.Add(observer);
                    }
                }

                List<System.Action<Type, object>> removeObservers = GetObserverList(_removeObservers, key);
                if (removeObservers.Contains(observer))
                {
                    removeObservers.Remove(observer);
                }
            }
        }

        public void RemoveObserver(string key, System.Action<Type, object> observer)
        {
            List<System.Action<Type, object>> observers = GetObserverList(_observers, key);
            if (!_isNotifiyng)
            {
                if (observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
            else
            {
                List<System.Action<Type, object>> removeObservers = GetObserverList(_removeObservers, key);
                if (!removeObservers.Contains(observer))
                {
                    if (observers.Contains(observer))
                    {
                        removeObservers.Add(observer);
                    }
                }

                List<System.Action<Type, object>> addObservers = GetObserverList(_addObservers, key);
                if (addObservers.Contains(observer))
                {
                    addObservers.Remove(observer);
                }
            }
        }


#if DEBUG
        public List<string> Keys
        {
            get
            {
                if (_parentBlackboard != null)
                {
                    List<string> keys = this._parentBlackboard.Keys;
                    keys.AddRange(_data.Keys);
                    return keys;
                }
                else
                {
                    return new List<string>(_data.Keys);
                }
            }
        }

        public int NumObservers
        {
            get
            {
                int count = 0;
                foreach (var key in _observers.Keys)
                {
                    count += _observers[key].Count;
                }
                return count;
            }
        }
#endif


        private void NotifiyObservers()
        {
            if (_notifications.Count == 0)
            {
                return;
            }

            _notificationsDispatch.Clear();
            _notificationsDispatch.AddRange(_notifications);
            foreach (Blackboard child in _children)
            {
                child._notifications.AddRange(_notifications);
                child._clock.AddTimer(0f, 0, child.NotifiyObservers);
            }
            _notifications.Clear();

            _isNotifiyng = true;
            foreach (Notification notification in _notificationsDispatch)
            {
                if (!_observers.ContainsKey(notification.Key))
                {
                    //                Debug.Log("1 do not notify for key:" + notification.key + " value: " + notification.value);
                    continue;
                }

                List<System.Action<Type, object>> observers = GetObserverList(_observers, notification.Key);
                foreach (System.Action<Type, object> observer in observers)
                {
                    if (_removeObservers.TryGetValue( notification.Key, out List<System.Action<Type, object>> value ) && value.Contains(observer))
                    {
                        continue;
                    }
                    observer(notification.Type, notification.Value);
                }
            }

            foreach (string key in _addObservers.Keys)
            {
                GetObserverList(_observers, key).AddRange(_addObservers[key]);
            }
            foreach (string key in _removeObservers.Keys)
            {
                foreach (System.Action<Type, object> action in _removeObservers[key])
                {
                    GetObserverList(_observers, key).Remove(action);
                }
            }
            _addObservers.Clear();
            _removeObservers.Clear();

            _isNotifiyng = false;
        }

        private List<System.Action<Type, object>> GetObserverList(Dictionary<string, List<System.Action<Type, object>>> target, string key)
        {
            List<System.Action<Type, object>> observers;
            if (target.TryGetValue(key, out var value))
            {
                observers = value;
            }
            else
            {
                observers = new List<System.Action<Type, object>>();
                target[key] = observers;
            }
            return observers;
        }
    }
}
