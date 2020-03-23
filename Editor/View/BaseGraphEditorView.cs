using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseGraphEditorView : VisualElement, IGraphEditorViewFactory
    {
        private BaseGraphView _graphView;

        private IEdgeConnectorListener _edgeConnector;
        private GraphModel _graph;

        public GraphModel Graph
        {
            get { return _graph; }
            set
            {
                _graph = value;
                _graphView.LoadGraph( _graph );

                OnGraphLoaded();
            }
        }

        public BaseGraphView GraphView => _graphView;

        public EditorWindow Window { get; set; }

        public Toolbar Toolbar { get; }

        public event Action SaveRequested;
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
                content.Add( _graphView );
            }
            Add( content );

            _graphView.MarkDirtyRepaint();
        }

        public virtual BaseNodeView CreateNodeView( NodeModel node, IEdgeConnectorListener edgeConnectorListener )
        {
            return new BaseNodeView( node, edgeConnectorListener );
        }

        public virtual BaseConnectionView CreateConnectionView( ConnectionModel connection )
        {
            return new BaseConnectionView( connection );
        }

        protected virtual BaseGraphView CreateGraphView()
        {
            return new BaseGraphView( this );
        }

        protected virtual void OnGraphLoaded()
        {
        }

        private void NodeSelectRequested( NodeCreationContext obj )
        {
            var port = ((obj.target as BaseConnectionView)?.output as BasePortView)?.Port;

            var searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Init( Graph.Type, NodeCreationRequested, port );

            SearchWindow.Open( new SearchWindowContext( obj.screenMousePosition ), searchWindowProvider );
        }

        private bool NodeCreationRequested( SearchWindowResult result )
        {
            var newNode = new NodeModel( result.NodeType );
            result.NodeType.PostLoad( newNode );

            var windowRoot = Window.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo( windowRoot.parent, result.ScreenPosition - Window.position.position );
            windowMousePosition.y -= windowRoot.worldBound.y; // compensate for window header
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal( windowMousePosition );

            newNode.Position = graphMousePosition;

            using( _graph.BeginChangeSet() )
            {
                _graph.AddNode( newNode );

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
