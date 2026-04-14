using Sandbox;
using Sandbox.Diagnostics;

namespace NPBehave
{
    public abstract class Node
    {
        public enum State
        {
            Inactive,
            Active,
            StopRequested,
        }

        protected State currentState = State.Inactive;

        public State CurrentState
        {
            get { return currentState; }
        }

        public Root RootNode;

        private Container _parentNode;
        public Container ParentNode
        {
            get
            {
                return _parentNode;
            }
        }

        private string _label;

        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
            }
        }

        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public virtual Blackboard Blackboard
        {
            get
            {
                return RootNode.Blackboard;
            }
        }

        public virtual Clock Clock
        {
            get
            {
                return RootNode.Clock;
            }
        }

        public bool IsStopRequested
        {
            get
            {
                return currentState == State.StopRequested;
            }
        }

        public bool IsActive
        {
            get
            {
                return currentState == State.Active;
            }
        }


        public Node(string name)
        {
            _name = name;
        }

        public virtual void SetRoot(Root rootNode)
        {
            RootNode = rootNode;
        }

        public void SetParent(Container parent)
        {
            _parentNode = parent;
        }

#if DEBUG
	    
	    public virtual string DebugIcon => "fiber_manual_record";
	    public virtual string ComputedLabel => string.Empty;
	    
        public TimeSince DebugLastStopRequestAt = 0.0f;
        public TimeSince DebugLastStoppedAt = 0.0f;
        public TimeSince DebugLastSuccessAt = 0.0f;
        public TimeSince DebugLastFailureAt = 0.0f;
        public int DebugNumStartCalls = 0;
        public int DebugNumStopCalls = 0;
        public int DebugNumStoppedCalls = 0;
        public bool DebugLastResult = false;
#endif

        public void Start()
        {
            // Assert.AreEqual(this.currentState, State.INACTIVE, "can only start inactive nodes, tried to start: " + this.Name + "! PATH: " + GetPath());
            Assert.AreEqual(currentState, State.Inactive, $"can only start inactive nodes - {Name} is {currentState}");

#if DEBUG
            RootNode.TotalNumStartCalls++;
            this.DebugNumStartCalls++;
#endif
            currentState = State.Active;
            DoStart();
        }

        /// <summary>
        /// TODO: Rename to "Cancel" in next API-Incompatible version
        /// </summary>
        public void Stop()
        {
            // Assert.AreEqual(this.currentState, State.ACTIVE, "can only stop active nodes, tried to stop " + this.Name + "! PATH: " + GetPath());
            Assert.AreEqual(currentState, State.Active, $"can only stop active nodes, tried to stop {Name} - {currentState}");
            currentState = State.StopRequested;
#if DEBUG
            RootNode.TotalNumStopCalls++;
            this.DebugLastStopRequestAt = 0;
            this.DebugNumStopCalls++;
#endif
            DoStop();
        }

        protected virtual void DoStart()
        {

        }

        protected virtual void DoStop()
        {

        }


        /// THIS ABSOLUTLY HAS TO BE THE LAST CALL IN YOUR FUNCTION, NEVER MODIFY
        /// ANY STATE AFTER CALLING Stopped !!!!
        protected virtual void Stopped(bool success)
        {
            // Assert.AreNotEqual(this.currentState, State.INACTIVE, "The Node " + this + " called 'Stopped' while in state INACTIVE, something is wrong! PATH: " + GetPath());
            Assert.AreNotEqual(currentState, State.Inactive, "Called 'Stopped' while in state INACTIVE, something is wrong!");
            currentState = State.Inactive;
#if DEBUG
            RootNode.TotalNumStoppedCalls++;
            this.DebugNumStoppedCalls++;
            this.DebugLastStoppedAt = 0;
            DebugLastResult = success;
            if(success)
	            DebugLastSuccessAt = 0;
            else
	            DebugLastFailureAt = 0;
            
#endif
	        ParentNode?.ChildStopped(this, success);
        }

        public virtual void ParentCompositeStopped(Composite composite)
        {
            DoParentCompositeStopped(composite);
        }

        /// THIS IS CALLED WHILE YOU ARE INACTIVE, IT's MEANT FOR DECORATORS TO REMOVE ANY PENDING
        /// OBSERVERS
        protected virtual void DoParentCompositeStopped(Composite composite)
        {
            /// be careful with this!
        }

        // public Composite ParentComposite
        // {
        //     get
        //     {
        //         if (ParentNode != null && !(ParentNode is Composite))
        //         {
        //             return ParentNode.ParentComposite;
        //         }
        //         else
        //         {
        //             return ParentNode as Composite;
        //         }
        //     }
        // }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Label) ? (Name + "{"+Label+"}") : Name;
        }

        protected string GetPath()
        {
	        return ParentNode != null ? $"{ParentNode.GetPath()}/{Name}" : Name;
        }
    }
}
