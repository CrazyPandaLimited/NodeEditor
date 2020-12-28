using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base class for GraphEditor view
    /// </summary>
    public class BaseGraphEditorView : VisualElement, IGraphEditorViewFactory
    {
        private BaseGraphView _graphView;
        private readonly SGraphToGraphContentConverter _sGraphToGraphContentConverter = new SGraphToGraphContentConverter();

        private SGraph _graph;

        /// <summary>
        /// <see cref="GraphModel"/> that is displayed by this view
        /// </summary>
        public GraphModel GraphModel => _sGraphToGraphContentConverter.GraphModel;

        /// <summary>
        /// <see cref="SGraph"/> that is displayed by this view
        /// </summary>
        public SGraph Graph
        {
            get { return _graph; }
            set
            {
                _graph = value;
                _graphView.LoadGraph( _graph );

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
            {
                Toolbar.Add( new ToolbarButton( () => SaveRequested?.Invoke() ) { text = "Save Asset" } );
                Toolbar.Add( new ToolbarSpacer() );
                Toolbar.Add( new ToolbarButton( () => ShowInProjectRequested?.Invoke() ) { text = "Show In Project" } );
            }
            Add( Toolbar );

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
            Add( content );

            _graphView.MarkDirtyRepaint();
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
