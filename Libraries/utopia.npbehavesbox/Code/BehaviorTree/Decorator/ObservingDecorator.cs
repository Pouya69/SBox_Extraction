using System.Collections;
using Sandbox.Diagnostics;

namespace NPBehave
{
    public abstract class ObservingDecorator : Decorator
    {
        protected Stops StopsOnChange;
        private bool _isObserving;

        public ObservingDecorator(string name, Stops stopsOnChange, Node decoratee) : base(name, decoratee)
        {
            StopsOnChange = stopsOnChange;
            _isObserving = false;
        }

        protected override void DoStart()
        {
            if (StopsOnChange != Stops.None)
            {
                if (!_isObserving)
                {
                    _isObserving = true;
                    StartObserving();
                }
            }

            if (!IsConditionMet())
            {
                Stopped(false);
            }
            else
            {
                Decoratee.Start();
            }
        }

        protected override void DoStop()
        {
            Decoratee.Stop();
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            Assert.AreNotEqual(((Node)this).CurrentState, State.Inactive);
            if (StopsOnChange is Stops.None or Stops.Self)
            {
                if (_isObserving)
                {
                    _isObserving = false;
                    StopObserving();
                }
            }
            Stopped(result);
        }

        protected override void DoParentCompositeStopped(Composite parentComposite)
        {
            if (_isObserving)
            {
                _isObserving = false;
                StopObserving();
            }
        }

        protected void Evaluate()
        {
            if (IsActive && !IsConditionMet())
            {
                if (StopsOnChange is Stops.Self or Stops.Both or Stops.ImmediateRestart)
                {
                    // Debug.Log( this.key + " stopped self ");
                    Stop();
                }
            }
            else if (!IsActive && IsConditionMet())
            {
                if (StopsOnChange == Stops.LowerPriority || StopsOnChange == Stops.Both || StopsOnChange == Stops.ImmediateRestart || StopsOnChange == Stops.LowerPriorityImmediateRestart)
                {
                    // Debug.Log( this.key + " stopped other ");
                    Container parentNode = ParentNode;
                    Node childNode = this;
                    while (parentNode != null && !(parentNode is Composite))
                    {
                        childNode = parentNode;
                        parentNode = parentNode.ParentNode;
                    }
                    Assert.NotNull(parentNode, "NTBtrStops is only valid when attached to a parent composite");
                    Assert.NotNull(childNode);
                    if (parentNode is Parallel)
                    {
                        Assert.True(StopsOnChange == Stops.ImmediateRestart, "On Parallel Nodes all children have the same priority, thus Stops.LOWER_PRIORITY or Stops.BOTH are unsupported in this context!");
                    }

                    if (StopsOnChange == Stops.ImmediateRestart || StopsOnChange == Stops.LowerPriorityImmediateRestart)
                    {
                        if (_isObserving)
                        {
                            _isObserving = false;
                            StopObserving();
                        }
                    }

                    ((Composite)parentNode)?.StopLowerPriorityChildrenForChild(childNode, StopsOnChange is Stops.ImmediateRestart or Stops.LowerPriorityImmediateRestart);
                }
            }
        }

        protected abstract void StartObserving();

        protected abstract void StopObserving();

        protected abstract bool IsConditionMet();

    }
}
