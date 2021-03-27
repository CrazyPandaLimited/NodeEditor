using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseGraphEditorView < TGraphSettingsView > : BaseGraphEditorView where TGraphSettingsView : BaseGraphSettingsView, new()
    {
        protected TGraphSettingsView _graphSettingsView;

        public override SGraph Graph
        {
            get => base.Graph;
            set
            {
                if( _graph != null )
                {
                    _graph.OnCustomSettingsChanged -= OnGraphSettingsChanged;
                }

                if( value.CustomSettings == null )
                {
                    value.CustomSettings = CreateGraphSettings();
                }
                
                OnGraphSettingsChanged( value.CustomSettings );
                
                value.OnCustomSettingsChanged += OnGraphSettingsChanged;

                base.Graph = value;
            }
        }

        public BaseGraphEditorView() => CreateToolbarButton( "Graph Settings", OnToolbarSettingsButtonClick );
        
        protected virtual void OnToolbarSettingsButtonClick()
        {
            if( _graphSettingsView == null )
            {
                _graphSettingsView = new TGraphSettingsView { Model = Graph.CustomSettings };
                _overlayRoot.Add( _graphSettingsView );
            }
            else
            {
                _overlayRoot.Remove( _graphSettingsView );
                _graphSettingsView = null;
            }
        }
        
        private void OnGraphSettingsChanged( IGraphSettings graphSettings )
        {
            if( _graphSettingsView == null )
            {
                return;
            }
                    
            _graphSettingsView.Model = graphSettings;
        }
    }
    
    /// <summary>
    /// Base class for GraphEditor view
    /// </summary>
    public class BaseGraphEditorView : VisualElement, IGraphEditorViewFactory
    {
        private BaseGraphView _graphView;

        private readonly SGraphToGraphContentConverter _sGraphToGraphContentConverter = new SGraphToGraphContentConverter();
        protected VisualElement _overlayRoot;


        protected SGraph _graph { get; private set; }

        /// <summary>
        /// <see cref="GraphModel"/> that is displayed by this view
        /// </summary>
        public GraphModel GraphModel => _sGraphToGraphContentConverter.GraphModel;

        /// <summary>
        /// <see cref="SGraph"/> that is displayed by this view
        /// </summary>
        public virtual SGraph Graph
        {
            get { return _graph; }
            set
            {
                _graph = value;
                _graphView.LoadGraph( _graph );

                if( _graph.CustomSettings == null )
                {
                    _graph.CustomSettings = CreateGraphSettings();
                }

                _sGraphToGraphContentConverter.SetGraph( value );

                OnGraphLoaded();
            }
        }
        
        /// <summary>
        /// View used to display graph
        /// </summary>
        public BaseGraphView GraphView => _graphView;

        /// <summary>
        /// Owning window
        /// </summary>
        public EditorWindow Window { get; set; }

        /// <summary>
        /// Toolbar view
        /// </summary>
        public Toolbar Toolbar { get; }

        /// <summary>
        /// Called when user requests to save
        /// </summary>
        public event Action SaveRequested;

        /// <summary>
        /// Called when user requests to view asset in Project tab
        /// </summary>
        public event Action ShowInProjectRequested;

        public BaseGraphEditorView()
        {
            styleSheets.Add( Resources.Load<StyleSheet>( $"Styles/BaseGraphEditorView" ) );

            Toolbar = new Toolbar() { name = "main-toolbar" };
            Add( Toolbar );

            CreateToolbarButton( "Save Asset", () => SaveRequested?.Invoke() );
            Toolbar.Add( new ToolbarSpacer() );
            CreateToolbarButton( "Show In Project", () => ShowInProjectRequested?.Invoke() );
            
            var content = new VisualElement { name = "content" };
            {
                _graphView = CreateGraphView();
                _graphView.name = "GraphView";
                _graphView.viewDataKey = GetType().Name;
                _graphView.SetupZoom( 0.05f, ContentZoomer.DefaultMaxScale );
                _graphView.AddManipulator( new ContentDragger() );
                _graphView.AddManipulator( new SelectionDragger() );
                _graphView.AddManipulator( new RectangleSelector() );
                _graphView.AddManipulator( new ClickSelector() );

                _graphView.nodeCreationRequest += NodeSelectRequested;

                new NodesCopyPasteHelper( _graphView );
                
                content.Add( _graphView );
            }

            _overlayRoot = new VisualElement();
            {
                _overlayRoot.name = "overlay-root";
                _overlayRoot.style.position = Position.Absolute;
                _overlayRoot.style.left = 0;
                _overlayRoot.style.right = 0;
                _overlayRoot.style.top = 0;
                _overlayRoot.style.bottom = 0;
                _overlayRoot.pickingMode = PickingMode.Ignore;
            }
            content.Add( _overlayRoot );

            Add( content );

            _graphView.MarkDirtyRepaint();
        }

        protected void CreateToolbarButton( string name, Action clickEvent )
        {
            Toolbar.Add( new ToolbarButton( clickEvent )
            {
                text = name
            } );
        }

        /// <summary>
        /// Creates new <see cref="BaseNodeView"/> for a <see cref="NodeModel"/>.
        /// Override this to create custom view
        /// </summary>
        /// <param name="node">Node that needs a view</param>
        /// <param name="edgeConnectorListener">Edge connector listener needed to create a view</param>
        public virtual BaseNodeView CreateNodeView( SNode node, IEdgeConnectorListener edgeConnectorListener )
        {
            return new BaseNodeView( node, edgeConnectorListener );
        }
        
        /// <summary>
        /// Creates new <see cref="BaseConnectionView"/> for a <see cref="ConnectionModel"/>.
        /// Override this to create custom view
        /// </summary>
        /// <param name="connection">Connection that needs view</param>
        public virtual BaseConnectionView CreateConnectionView( SConnection connection )
        {
            return new BaseConnectionView( connection );
        }

        /// <summary>
        /// Creates new <see cref="BaseGraphView"/>.
        /// Override this to create custom view
        /// </summary>
        /// <returns></returns>
        protected virtual BaseGraphView CreateGraphView()
        {
            return new BaseGraphView( this );
        }

        /// <summary>
        /// Called after new graph was loaded
        /// </summary>
        protected virtual void OnGraphLoaded()
        {
        }

        protected virtual IGraphSettings CreateGraphSettings()
        {
            return default;
        }
        
        private void NodeSelectRequested( NodeCreationContext obj )
        {
            var port = ((obj.target as BaseConnectionView)?.output as BasePortView)?.Port;

            var searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Init( _graph.GraphType, port, NodeCreationRequested );

            SearchWindow.Open( new SearchWindowContext( obj.screenMousePosition ), searchWindowProvider );
        }

        private bool NodeCreationRequested( SearchWindowResult result )
        {
            var windowRoot = Window.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo( windowRoot.parent, result.ScreenPosition - Window.position.position );
            windowMousePosition.y -= windowRoot.worldBound.y; // compensate for window header
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal( windowMousePosition );

            var newNode = result.Node;
            newNode.Position = graphMousePosition;

            using( _graph.BeginChangeSet() )
            {
                _graph.AddNode( result.Node );

                if( result.FromPort != null )
                {
                    var toPort = newNode.InputPorts().FirstOrDefault( p => _graph.CanConnect( result.FromPort, p ) );

                    if( toPort != null )
                    {
                        _graph.Connect( result.FromPort, toPort );
                    }
                }
            }

            return true;
        }
    }
}
