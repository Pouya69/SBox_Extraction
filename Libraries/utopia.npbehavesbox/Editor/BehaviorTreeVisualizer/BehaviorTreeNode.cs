using System;
using System.Linq;
using System.Text;
using Editor;
using NPBehave;
using Sandbox.Utils;

namespace Sandbox.BehaviorTreeVisualizer;

public class BehaviorTreeNode : TreeNode<Node>
{
	private Color _nodeColor;
	
	Color NodeColor
	{
		get => _nodeColor;
		set
		{
			if( _nodeColor == value )
				return;
			_nodeColor = value;
			Dirty();
		}
	}

	public BehaviorTreeNode( Node dir ) : base( dir )
	{
		Height = 40;
	}

	protected override void BuildChildren()
	{
		Clear();
		
		if ( Value is Container container )
		{
			foreach ( var child in container.DebugChildren )
			{
				AddItem( CreateChildFor( child ) );
			}
		}
	}

	protected virtual TreeNode CreateChildFor( Node child ) => new BehaviorTreeNode( child );


	public override void OnSelectionChanged( bool state )
	{
		if ( state )
		{
			TreeView?.Toggle( this );
		}
		base.OnSelectionChanged( state );
	}

	public override bool OnContextMenu()
	{
		var m = new Editor.Menu( TreeView );
		if ( Value.CurrentState == Node.State.Active )
		{
			m.AddOption( "Stop", action: () =>
			{
				Value.Stop();
			} );
		}
		else if(Value is Root root && Value.CurrentState == Node.State.Inactive )
		{
			m.AddOption( "Start", action: () => root.Start() );
		}
		
		m.OpenAtCursor( false );
		return true;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Value.GetHashCode(), Value.CurrentState, Value.IsActive, Value.Name, Value.ComputedLabel) ;
	}

	public override int ValueHash => GetHashCode();

	protected override void Think()
	{
		Color newColor = Theme.TextDark;

		if ( Value.IsActive )
		{
			newColor = Theme.Green;
		}
		
		else if ( Value.DebugLastSuccessAt < 0.5f )
		{
			newColor = Theme.Green;
		}
		else if ( Value.DebugLastFailureAt < 0.5f )
		{
			newColor = Theme.Red;
		}
		
		NodeColor = newColor;
			
		
		base.Think();
	}

	protected override void RebuildOnDirty()
	{
		TreeView?.Update();
		if(Value is Container container && container.DebugChildren.Length != Children.Count() )
		{
			BuildChildren();
		}
		//base.RebuildOnDirty();
	}

	protected override void OnHashChanged()
	{
		Dirty();
		if(Value is Container container && container.DebugChildren.Length != Children.Count() )
		{
			BuildChildren();
		}
	}

	public override void OnPaint( VirtualWidget item )
	{
		var open = item.IsOpen;
		
		var backgroundRect = item.Rect;
		backgroundRect.Bottom -= 1;
		
		if ( item.Selected )
		{
			Paint.SetPen( Theme.Primary.WithAlpha( 0.9f ) );
			Paint.SetBrush( NodeColor.WithAlpha( 0.6f ) );
			Paint.DrawRect( backgroundRect.Shrink( 2 ) );
		}
		else if ( item.Hovered )
		{
			Paint.SetPen( Theme.Primary.WithAlpha( 0.9f ) );
			Paint.SetBrush( NodeColor.WithAlpha( 0.7f ) );
			Paint.DrawRect( backgroundRect.Shrink( 1 ) );
		}
		else
		{
			Paint.ClearPen();
			Paint.SetBrush( NodeColor.WithAlpha( 0.7f ) );
			Paint.DrawRect( backgroundRect );
		}
		
		var rect = backgroundRect.Shrink( 8 );

		if ( !string.IsNullOrWhiteSpace( Value.DebugIcon ) )
		{
			Paint.SetPen( Theme.Yellow.WithAlphaMultiplied( 1.0f) );
			var i = Paint.DrawIcon( rect, Value.DebugIcon, 22, TextFlag.LeftCenter );
			rect.Left = i.Right + 8;
		}


		Paint.SetPen( Theme.Base.WithAlpha(1.0f));
		Paint.SetFont( "Poppins", 12, 450 );

		StringBuilder text = new StringBuilder();

		if ( !string.IsNullOrWhiteSpace( Value.Label ) )
		{
			text.Append( $" {Value.Label}" );
		}
		if( !string.IsNullOrWhiteSpace( Value.ComputedLabel ) )
		{
			text.Append( $" {Value.ComputedLabel}" );
		}
		if ( text.Length == 0 )
		{
			text.Append( Value.Name.AddSpacesToSentence( ) );
		}
		else
		{
			text.Append( $" ({Value.Name.AddSpacesToSentence( )})" );
		}
		
		
		var textRect = Paint.DrawText( rect, text.ToString(), TextFlag.LeftCenter );
	}
}
