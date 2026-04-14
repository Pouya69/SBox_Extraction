namespace NPBehave
{

    public abstract class Decorator : Container
    {
        protected Node Decoratee;

        public Decorator(string name, Node decoratee) : base(name)
        {
            Decoratee = decoratee;
            Decoratee.SetParent(this);
        }

        public override void SetRoot(Root rootNode)
        {
            base.SetRoot(rootNode);
            Decoratee.SetRoot(rootNode);
        }


#if DEBUG

	    public override string DebugIcon => "brush";
	    public override Node[] DebugChildren
        {
            get
            {
                return new Node[] { Decoratee };
            }
        }
#endif

        public override void ParentCompositeStopped(Composite composite)
        {
            base.ParentCompositeStopped(composite);
            Decoratee.ParentCompositeStopped(composite);
        }
    }
}
