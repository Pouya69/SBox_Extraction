namespace NPBehave
{
    public class BlackboardCondition : ObservingDecorator
    {
        private string _key;
        private object _value;
        private Operator _op;

        public string Key
        {
            get
            {
                return _key;
            }
        }

        public object Value
        {
            get
            {
                return _value;
            }
        }

        public Operator Operator
        {
            get
            {
                return _op;
            }
        }
        
        #if DEBUG
	    public override string DebugIcon => "quiz";
	    public override string ComputedLabel
	    {
		    get
		    {
			    return $"{Key} {OperatorToString(Operator)} {Value}";
		    }
	    }

	    public string OperatorToString( Operator _op )
	    {
		    return _op switch
		    {
			    Operator.IsSet => "?=",
			    Operator.IsNotSet => "?!=",
			    Operator.IsEqual => "==",
			    Operator.IsNotEqual => "!=",
			    Operator.IsGreaterOrEqual => ">=",
			    Operator.IsGreater => ">",
			    Operator.IsSmallerOrEqual => "<=",
			    Operator.IsSmaller => "<",
			    Operator.AlwaysTrue => "ALWAYS_TRUE",
			    _ => $"<{_op}>"
		    };
	    }


#endif

        public BlackboardCondition(string key, Operator op, object value, Stops stopsOnChange, Node decoratee) : base("BlackboardCondition", stopsOnChange, decoratee)
        {
            _op = op;
            _key = key;
            _value = value;
            StopsOnChange = stopsOnChange;
        }
        
        public BlackboardCondition(string key, Operator op, Stops stopsOnChange, Node decoratee) : base("BlackboardCondition", stopsOnChange, decoratee)
        {
            _op = op;
            _key = key;
            StopsOnChange = stopsOnChange;
        }


        protected override void StartObserving()
        {
            RootNode.Blackboard.AddObserver(_key, OnValueChanged);
        }

        protected override void StopObserving()
        {
            RootNode.Blackboard.RemoveObserver(_key, OnValueChanged);
        }

        private void OnValueChanged(Blackboard.Type type, object newValue)
        {
            Evaluate();
        }

        protected override bool IsConditionMet()
        {
            if (_op == Operator.AlwaysTrue)
            {
                return true;
            }

            if (!RootNode.Blackboard.IsSet(_key))
            {
                return _op == Operator.IsNotSet;
            }

            object o = RootNode.Blackboard.Get(_key);

            switch (_op)
            {
                case Operator.IsSet: return true;
                case Operator.IsEqual: return Equals(o, _value);
                case Operator.IsNotEqual: return !Equals(o, _value);

                case Operator.IsGreaterOrEqual:
                    if (o is float)
                    {
                        return (float)o >= (float)_value;
                    }
                    else if (o is int)
                    {
                        return (int)o >= (int)_value;
                    }
                    else
                    {
                        Log.Error( $"Type not compareable: {o.GetType()}" );
                        return false;
                    }

                case Operator.IsGreater:
                    if (o is float)
                    {
                        return (float)o > (float)_value;
                    }
                    else if (o is int)
                    {
                        return (int)o > (int)_value;
                    }
                    else
                    {
	                    Log.Error( $"Type not compareable: {o.GetType()}" );
                        return false;
                    }

                case Operator.IsSmallerOrEqual:
                    if (o is float)
                    {
                        return (float)o <= (float)_value;
                    }
                    else if (o is int)
                    {
                        return (int)o <= (int)_value;
                    }
                    else
                    {
	                    Log.Error( $"Type not compareable: {o.GetType()}" );
                        return false;
                    }

                case Operator.IsSmaller:
                    if (o is float)
                    {
                        return (float)o < (float)_value;
                    }
                    else if (o is int)
                    {
                        return (int)o < (int)_value;
                    }
                    else
                    {
	                    Log.Error( $"Type not compareable: {o.GetType()}" );
                        return false;
                    }

                default: return false;
            }
        }

        public override string ToString()
        {
            return $"({_op}) {_key} ? {_value}";
        }
    }
}
