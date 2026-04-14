using Sandbox.Diagnostics;

namespace NPBehave
{
    public abstract class Container : Node
    {
        private bool _collapse = false;
        public bool Collapse
        {
            get
            {
                return _collapse;
            }
            set
            {
                _collapse = value;
            }
        }

        public Container(string name) : base(name)
        {
        }

        public void ChildStopped(Node child, bool succeeded)
        {
            // Assert.AreNotEqual(this.currentState, State.INACTIVE, "The Child " + child.Name + " of Container " + this.Name + " was stopped while the container was inactive. PATH: " + GetPath());
            Assert.AreNotEqual(currentState, State.Inactive, "A Child of a Container was stopped while the container was inactive.");
            DoChildStopped(child, succeeded);
        }

        protected abstract void DoChildStopped(Node child, bool succeeded);

#if DEBUG

	    public override string DebugIcon => "list_alt";
	    public abstract Node[] DebugChildren
        {
            get;
        }
#endif
    }
}
