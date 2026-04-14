namespace NPBehave
{
    public class BlackboardQuery : ObservingDecorator
    {
        private string[] _keys;
        private System.Func<bool> _query;

        public BlackboardQuery(string[] keys, Stops stopsOnChange, System.Func<bool> query, Node decoratee) : base("BlackboardQuery", stopsOnChange, decoratee)
        {
            _keys = keys;
            _query = query;
        }

        protected override void StartObserving()
        {
            foreach (string key in _keys)
            {
                RootNode.Blackboard.AddObserver(key, OnValueChanged);
            }
        }

        protected override void StopObserving()
        {
            foreach (string key in _keys)
            {
                RootNode.Blackboard.RemoveObserver(key, OnValueChanged);
            }
        }

        private void OnValueChanged(Blackboard.Type type, object newValue)
        {
            Evaluate();
        }

        protected override bool IsConditionMet()
        {
            return _query();
        }

        public override string ToString()
        {
            string keys = "";
            foreach (string key in _keys)
            {
                keys += $" {key}";
            }
            return Name + keys;
        }
    }
}