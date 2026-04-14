using Sandbox.Diagnostics;

namespace NPBehave
{
    public class Root : Decorator
    {
        private Node _mainNode;

        //private Node inProgressNode;

        private Blackboard _blackboard;
        public override Blackboard Blackboard
        {
            get
            {
                return _blackboard;
            }
        }


        private Clock _clock;
        public override Clock Clock
        {
            get
            {
                return _clock;
            }
        }

#if DEBUG
	    public override string DebugIcon => "home";
	    public int TotalNumStartCalls = 0;
        public int TotalNumStopCalls = 0;
        public int TotalNumStoppedCalls = 0;
#endif

        public Root(Node mainNode) : base("Root", mainNode)
        {
            _mainNode = mainNode;
            _clock = SandboxContext.GetClock();
            _blackboard = new Blackboard(_clock);
            SetRoot(this);
        }
        public Root(Blackboard blackboard, Node mainNode) : base("Root", mainNode)
        {
            _blackboard = blackboard;
            _mainNode = mainNode;
            _clock = SandboxContext.GetClock();
            SetRoot(this);
        }

        public Root(Blackboard blackboard, Clock clock, Node mainNode) : base("Root", mainNode)
        {
            _blackboard = blackboard;
            _mainNode = mainNode;
            _clock = clock;
            SetRoot(this);
        }

        public override void SetRoot(Root rootNode)
        {
            Assert.AreEqual(this, rootNode);
            base.SetRoot(rootNode);
            _mainNode.SetRoot(rootNode);
        }


        protected override void DoStart()
        {
            _blackboard.Enable();
            _mainNode.Start();
        }

        protected override void DoStop()
        {
            if (_mainNode.IsActive)
            {
                _mainNode.Stop();
            }
            else
            {
                _clock.RemoveTimer(_mainNode.Start);
            }
        }


        protected override void DoChildStopped(Node node, bool success)
        {
            if (!IsStopRequested)
            {
                // wait one tick, to prevent endless recursions
                _clock.AddTimer(0, 0, _mainNode.Start);
            }
            else
            {
                _blackboard.Disable();
                Stopped(success);
            }
        }
    }
}
