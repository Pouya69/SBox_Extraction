using Sandbox.Diagnostics;

namespace NPBehave
{
    public class Action : Task
    {
        public enum Result
        {
            Success,
            Failed,
            Blocked,
            Progress
        }

        public enum Request
        {
            Start,
            Update,
            Cancel,
        }

        private System.Func<bool> _singleFrameFunc = null;
        private System.Func<bool, Result> _multiFrameFunc = null;
        private System.Func<Request, Result> _multiFrameFunc2 = null;
        private System.Action _action = null;
        private bool _bWasBlocked = false;

        public Action(System.Action action) : base("Action")
        {
            _action = action;
        }

        public Action(System.Func<bool, Result> multiframeFunc) : base("Action")
        {
            _multiFrameFunc = multiframeFunc;
        }

        public Action(System.Func<Request, Result> multiframeFunc2) : base("Action")
        {
            _multiFrameFunc2 = multiframeFunc2;
        }


        public Action(System.Func<bool> singleFrameFunc) : base("Action")
        {
            _singleFrameFunc = singleFrameFunc;
        }

        protected override void DoStart()
        {
            if (_action != null)
            {
                _action.Invoke();
                Stopped(true);
            }
            else if (_multiFrameFunc != null)
            {
                Result result = _multiFrameFunc.Invoke(false);
                if ( result == Result.Progress )
                {
                    RootNode.Clock.AddUpdateObserver( OnUpdateFunc );
                }
                else if ( result == Result.Blocked )
                {
                    _bWasBlocked = true;
                    RootNode.Clock.AddUpdateObserver( OnUpdateFunc );
                }
                else
                {
                    Stopped(result == Result.Success);
                }
            }
            else if (_multiFrameFunc2 != null)
            {
                Result result = _multiFrameFunc2.Invoke(Request.Start);
                if (result == Result.Progress)
                {
                    RootNode.Clock.AddUpdateObserver(OnUpdateFunc2);
                }
                else if ( result == Result.Blocked )
                {
                    _bWasBlocked = true;
                    RootNode.Clock.AddUpdateObserver( OnUpdateFunc2 );
                }
                else
                {
                    Stopped(result == Result.Success);
                }
            }
            else if (_singleFrameFunc != null)
            {
                Stopped(_singleFrameFunc.Invoke());
            }
        }

        private void OnUpdateFunc()
        {
            Result result = _multiFrameFunc.Invoke(false);
            if (result != Result.Progress && result != Result.Blocked)
            {
                RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc);
                Stopped(result == Result.Success);
            }
        }

        private void OnUpdateFunc2()
        {
            Result result = _multiFrameFunc2.Invoke( _bWasBlocked ? Request.Start : Request.Update);

            if ( result == Result.Blocked )
            {
                _bWasBlocked = true;
            }
            else if ( result == Result.Progress )
            {
                _bWasBlocked = false;
            }
            else
            {
                RootNode.Clock.RemoveUpdateObserver( OnUpdateFunc2 );
                Stopped( result == Result.Success );
            }
        }

        protected override void DoStop()
        {
            if (_multiFrameFunc != null)
            {
                Result result = _multiFrameFunc.Invoke(true);
                Assert.AreNotEqual(result, Result.Progress, "The Task has to return Result.SUCCESS, Result.FAILED/BLOCKED after beeing cancelled!");
                RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc);
                Stopped(result == Result.Success);
            }
            else if (_multiFrameFunc2 != null)
            {
                Result result = _multiFrameFunc2.Invoke(Request.Cancel);
                Assert.AreNotEqual(result, Result.Progress, "The Task has to return Result.SUCCESS or Result.FAILED/BLOCKED after beeing cancelled!");
                RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc2);
                Stopped(result == Result.Success);
            }
            else
            {
                Assert.True(false, $"DoStop called for a single frame action on {this}" );
            }
        }
    }
}
