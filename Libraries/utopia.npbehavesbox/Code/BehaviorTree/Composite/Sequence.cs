using System.Collections;
using Sandbox.Diagnostics;

namespace NPBehave
{
    public class Sequence : Composite
    {
        private int _currentIndex = -1;

        public Sequence(params Node[] children) : base("Sequence", children)
        {
        }

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
                ProcessChildren();
            }
            else
            {
                Stopped(false);
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
                Stopped(true);
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
