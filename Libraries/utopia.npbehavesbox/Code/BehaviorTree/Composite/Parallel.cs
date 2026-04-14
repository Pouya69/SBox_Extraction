using System.Collections.Generic;
using Sandbox.Diagnostics;

namespace NPBehave
{
    public class Parallel : Composite
    {
        public enum Policy
        {
            One,
            All,
        }

        // public enum Wait
        // {
        //     NEVER,
        //     ON_FAILURE,
        //     ON_SUCCESS,
        //     BOTH
        // }

        // private Wait waitForPendingChildrenRule;
        private Policy _failurePolicy;
        private Policy _successPolicy;
        private int _childrenCount = 0;
        private int _runningCount = 0;
        private int _succeededCount = 0;
        private int _failedCount = 0;
        private Dictionary<Node, bool> _childrenResults;
        private bool _successState;
        private bool _childrenAborted;

        public Parallel(Policy successPolicy, Policy failurePolicy, /*Wait waitForPendingChildrenRule,*/ params Node[] children) : base("Parallel", children)
        {
            _successPolicy = successPolicy;
            _failurePolicy = failurePolicy;
            // this.waitForPendingChildrenRule = waitForPendingChildrenRule;
            _childrenCount = children.Length;
            _childrenResults = new Dictionary<Node, bool>();
        }

        protected override void DoStart()
        {
            foreach (Node child in Children)
            {
                Assert.AreEqual(child.CurrentState, State.Inactive);
            }

            _childrenAborted = false;
            _runningCount = 0;
            _succeededCount = 0;
            _failedCount = 0;
            foreach (Node child in Children)
            {
                _runningCount++;
                child.Start();
            }
        }

        protected override void DoStop()
        {
            Assert.True(_runningCount + _succeededCount + _failedCount == _childrenCount);

            foreach (Node child in Children)
            {
                if (child.IsActive)
                {
                    child.Stop();
                }
            }
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            _runningCount--;
            if (result)
            {
                _succeededCount++;
            }
            else
            {
                _failedCount++;
            }
            _childrenResults[child] = result;

            bool allChildrenStarted = _runningCount + _succeededCount + _failedCount == _childrenCount;
            if (allChildrenStarted)
            {
                if (_runningCount == 0)
                {
                    if (!_childrenAborted) // if children got aborted because rule was evaluated previously, we don't want to override the successState 
                    {
                        if (_failurePolicy == Policy.One && _failedCount > 0)
                        {
                            _successState = false;
                        }
                        else if (_successPolicy == Policy.One && _succeededCount > 0)
                        {
                            _successState = true;
                        }
                        else if (_successPolicy == Policy.All && _succeededCount == _childrenCount)
                        {
                            _successState = true;
                        }
                        else
                        {
                            _successState = false;
                        }
                    }
                    Stopped(_successState);
                }
                else if (!_childrenAborted)
                {
                    Assert.False(_succeededCount == _childrenCount);
                    Assert.False(_failedCount == _childrenCount);

                    if (_failurePolicy == Policy.One && _failedCount > 0/* && waitForPendingChildrenRule != Wait.ON_FAILURE && waitForPendingChildrenRule != Wait.BOTH*/)
                    {
                        _successState = false;
                        _childrenAborted = true;
                    }
                    else if (_successPolicy == Policy.One && _succeededCount > 0/* && waitForPendingChildrenRule != Wait.ON_SUCCESS && waitForPendingChildrenRule != Wait.BOTH*/)
                    {
                        _successState = true;
                        _childrenAborted = true;
                    }

                    if (_childrenAborted)
                    {
                        foreach (Node currentChild in Children)
                        {
                            if (currentChild.IsActive)
                            {
                                currentChild.Stop();
                            }
                        }
                    }
                }
            }
        }

        public override void StopLowerPriorityChildrenForChild(Node abortForChild, bool immediateRestart)
        {
            if (immediateRestart)
            {
                Assert.False(abortForChild.IsActive);
                if (_childrenResults[abortForChild])
                {
                    _succeededCount--;
                }
                else
                {
                    _failedCount--;
                }
                _runningCount++;
                abortForChild.Start();
            }
            else
            {
                throw new Exception("On Parallel Nodes all children have the same priority, thus the method does nothing if you pass false to 'immediateRestart'!");
            }
        }
    }
}
