using System.Collections;
using Sandbox.Diagnostics;

namespace NPBehave
{
    public class RandomSequence : Composite
    {
        static System.Random _rng = new System.Random();


#if DEBUG
        static public void DebugSetSeed( int seed )
        {
            _rng = new System.Random( seed );
        }
#endif

        private int _currentIndex = -1;
        private int[] _randomizedOrder;

        public RandomSequence(params Node[] children) : base("Random Sequence", children)
        {
            _randomizedOrder = new int[children.Length];
            for (int i = 0; i < Children.Length; i++)
            {
                _randomizedOrder[i] = i;
            }
        }

        protected override void DoStart()
        {
            foreach (Node child in Children)
            {
                Assert.AreEqual(child.CurrentState, State.Inactive);
            }

            _currentIndex = -1;

            // Shuffling
            int n = _randomizedOrder.Length;
            while (n > 1)
            {
                int k = _rng.Next(n--);
                (_randomizedOrder[n], _randomizedOrder[k]) = (_randomizedOrder[k], _randomizedOrder[n]);
            }

            ProcessChildren();
        }

        protected override void DoStop()
        {
            Children[_randomizedOrder[_currentIndex]].Stop();
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
                    Children[_randomizedOrder[_currentIndex]].Start();
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
