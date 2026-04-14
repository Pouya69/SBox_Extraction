using System.Collections;
using Sandbox.Diagnostics;

namespace NPBehave
{
    public class Selector : Composite
    {
        private int _currentIndex = -1;

        public Selector(params Node[] children) : base("Selector", children)
        {
        }

		#if DEBUG
	    public override string DebugIcon => "rule";
		#endif
        protected override void DoStart()
        {
            foreach (Node child in Children)
            {
                Assert.AreEqual(child.CurrentState, State.Inactive);
            }

            _currentIndex = -1;

            ProcessChildren();
        }

        protected override void DoStop()
        {
            Children[_currentIndex].Stop();
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            if (result)
            {
                Stopped(true);
            }
            else
            {
                ProcessChildren();
            }
        }

        private void ProcessChildren()
        {
            if (++_currentIndex < Children.Length)
            {
                if (IsStopRequested)
                {
                    Stopped(false);
                }
                else
                {
                    Children[_currentIndex].Start();
                }
            }
            else
            {
                Stopped(false);
            }
        }

        public override void StopLowerPriorityChildrenForChild(Node abortForChild, bool immediateRestart)
        {
            int indexForChild = 0;
            bool found = false;
            foreach (Node currentChild in Children)
            {
                if (currentChild == abortForChild)
                {
                    found = true;
                }
                else if (!found)
                {
                    indexForChild++;
                }
                else if (found && currentChild.IsActive)
                {
                    if (immediateRestart)
                    {
                        _currentIndex = indexForChild - 1;
                    }
                    else
                    {
                        _currentIndex = Children.Length;
                    }
                    currentChild.Stop();
                    break;
                }
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}[{_currentIndex}]";
        }
    }
}
