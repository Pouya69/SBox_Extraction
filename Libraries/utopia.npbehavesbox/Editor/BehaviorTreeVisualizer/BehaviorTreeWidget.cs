#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor;
using NPBehave;
using Sandbox.UI;
using Checkbox = Editor.Checkbox;
using ControlSheet = Editor.ControlSheet;
using Label = Editor.Label;
using Option = Sandbox.UI.Option;

namespace Sandbox.BehaviorTreeVisualizer;


[Dock( "Editor", "Behavior Tree", "list" )]
public class BehaviorTreeWidget : Widget
{
	
	public DropDown? SelectBehaviorTree { get; set; }
	public ScrollArea? scroller { get; set; }
	
	private Checkbox ShowBlackboard { get; set; }
	public Widget? blackboardWidget { get; set; }
	
	public Widget? treeWidget { get; set; }
	
	protected TreeView? TreeView { get; set; }
	
	GameObject? _lastSelected = null;
	
	Dictionary<PropertyInfo, Root> _behaviorTrees = new();
	Root? _selected = null;


	public BehaviorTreeWidget(Widget parent) : base( parent )
	{
		Layout = Layout.Column();
		BuildUI();
	}

	[EditorEvent.Hotload]
	private void BuildUI()
	{
		Layout.Clear( true) ;

		if ( _behaviorTrees.Count > 1 )
		{
			var selection = new ComboBox( this );
			foreach (var behaviorTree in _behaviorTrees)
			{
		
				selection.AddItem( $"{behaviorTree.Key.DeclaringType} ({behaviorTree.Key.Name}) - {behaviorTree.Value.Name} ({behaviorTree.Value.Label})", onSelected: () =>
				{
					if ( _selected != behaviorTree.Value )
					{
						_selected = behaviorTree.Value;
						BuildUI();
					}
				} );
			}

			if ( _selected != null )
			{
				var pair = _behaviorTrees.FirstOrDefault( e => e.Value == _selected );
				if ( pair.Key != null && pair.Value != null )
					selection.TrySelectNamed(
						$"{pair.Key.DeclaringType} ({pair.Key.Name}) - {pair.Value.Name} ({pair.Value.Label})" );
			}
			else
			{
				_selected = _behaviorTrees.First(e=>e.Value.IsActive).Value;
			}
			Layout.Add( selection );
		}
		else
		{
			if ( _lastSelected.IsValid() && !TryGetBehaviorTree( _lastSelected, out _selected ) )
			{
				_selected = null;
			}
		}
		
		if ( _lastSelected.IsValid() && _selected != null)
		{
			if(_selected != null)
			{
				
				scroller = new ScrollArea( this ) { 
					Canvas = new Widget
					{
						Layout = Layout.Column(), 
						VerticalSizeMode = SizeMode.CanGrow, 
						HorizontalSizeMode = SizeMode.Flexible
					}
				};

				ShowBlackboard = new Checkbox( "Show Blackboard", this ) { 
					StateChanged = (state =>
						{
							if ( state == CheckState.On )
							{
								blackboardWidget = BuildBlackboardUI( _selected.Blackboard );
								scroller.Canvas.Layout.Add( blackboardWidget );
							}
							else if ( state == CheckState.Off )
							{
								blackboardWidget?.Destroy();
							}
						} )
				};
				scroller.Canvas.Layout.Add( ShowBlackboard );
				
				treeWidget = BuildBehaviorTree( _selected );
				scroller.Canvas.Layout.Add( treeWidget );
				
				Layout.Add( scroller );
				
			}
			else
			{
				Layout.Add(new Label( "Behavior tree found but null - is the game started?", this ));
			}
		}
		else
		{
			Layout.Add(new Label( "No behavior tree on selected gameobject", this ));
		}
	}

	private Widget BuildBlackboardUI( Blackboard behaviorBlackboard )
	{
		Widget widget = new Widget(  );
		widget.Layout = Layout.Column();
		
		Label blackboardLabel = new Label.Subtitle( "Blackboard values:" );
		var ps = new ControlSheet();
		
		behaviorBlackboard.Keys.ForEach( key =>
		{
			var property = new BlackboardProperty( key, behaviorBlackboard );
			ps.AddRow( property );
		});
		
		//ps.AddRow( behaviorBlackboard.GetSerialized().GetProperty( "_data" ));

		widget.Layout.Add( blackboardLabel );
		widget.Layout.Add( ps );
		widget.Layout.AddSeparator( true );
		return widget;
	}

	private Widget BuildBehaviorTree( Root behavior )
	{
		Widget widget = new Widget(  );
		widget.Layout = Layout.Column();
		
		Label subtitle = new Label.Subtitle( "Tree:" );
		widget.Layout.Add( subtitle );
		
		TreeView treeView = new TreeView
		{
			ExpandForSelection = true
		};
		treeView.AddItem( new BehaviorTreeNode( behavior ) );
		widget.Layout.Add( treeView );
		
		TreeView = treeView;
		return widget;
	}

	bool TryGetBehaviorTree( GameObject go, out Root? behavior)
	{
		behavior = null;
		bool propertyFound = false;
		foreach (Component component in go.Components.GetAll())
		{
			// Using reflection to check if the component have a public property of type Root
			var type = component.GetType();
			var property = type.GetProperties().FirstOrDefault( e => e.PropertyType == typeof(Root) );
			if ( property != null )
			{
				propertyFound = true;
				behavior = property.GetValue( component ) as Root;
				if( behavior != null )
					return true;
			}
		}
		return propertyFound;
	}
	
	Dictionary<PropertyInfo, Root> GetBehaviorTrees( GameObject? go )
	{
		Dictionary<PropertyInfo, Root> behaviors = new();
		if(go == null)
			return behaviors;
		
		foreach (Component component in go.Components.GetAll())
		{
			// Using reflection to check if the component have a public property of type Root
			var type = component.GetType();
			var property = type.GetProperties().FirstOrDefault( e => e.PropertyType == typeof(Root) );
			if ( property != null )
			{
				if( property.GetValue( component ) is Root behavior )
					behaviors.Add( property, behavior );
			}
		}
		return behaviors;
	}
	

	[EditorEvent.Frame]
	public void CheckForChanges()
	{		
		var activeScene = SceneEditorSession.Active;
		if ( activeScene != null )
		{
			var selected = activeScene.Selection.FirstOrDefault() as GameObject;
			if ( selected != _lastSelected)
			{
				_lastSelected = selected;
				_behaviorTrees = GetBehaviorTrees( _lastSelected );
				BuildUI();
			}
		}
	}
}
