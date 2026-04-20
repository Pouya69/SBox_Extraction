using Sandbox.Diagnostics;

namespace NPBehave
{
    public abstract class Composite : Container
    {
        protected Node[] Children;

        public Composite(string name, Node[] children) : base(name)
        {
            Children = children;

			if ( children.Length == 0 )
				Log.Warning( "\"Composite nodes (Selector, Sequence, Parallel) need at least one child!\"" );

			foreach (Node node in Children)
            {
                node.SetParent(this);
            }
        }

		public Composite( string name ) : base( name )
		{
		}

		/// <summary>
		/// ONLY when initializing the composite AFTER construction.
		/// </summary>
		public void InitComposite( Node[] children )
		{
			Children = children;

			if ( children.Length == 0 )
				Log.Warning( "\"Composite nodes (Selector, Sequence, Parallel) need at least one child!\"" );

			foreach ( Node node in Children )
			{
				node.SetParent( this );
			}
		}

		public override void SetRoot(Root rootNode)
        {
            base.SetRoot(rootNode);

            foreach (Node node in Children)
            {
                node.SetRoot(rootNode);
            }
        }


#if DEBUG
        public override Node[] DebugChildren
        {
            get
            {
                return this.Children;
            }
        }

        public Node DebugGetActiveChild()
        {
            foreach( Node node in DebugChildren )
            {
                if(node.CurrentState == Node.State.Active )
                {
                    return node;
                }
            }

            return null;
        }
#endif

        protected override void Stopped(bool success)
        {
            foreach (Node child in Children)
            {
                child.ParentCompositeStopped(this);
            }
            base.Stopped(success);
        }

        public abstract void StopLowerPriorityChildrenForChild(Node child, bool immediateRestart);
    }
}
