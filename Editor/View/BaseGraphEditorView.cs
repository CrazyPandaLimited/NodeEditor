using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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

        protected IEnumerable<(string Title, Action Action)> CustomButtons { get; set; } = new (string, Action)[ 0 ];

        public event Action SaveRequested;
        public event Action ShowInProjectRequested;

        public BaseGraphEditorView()
        {
            styleSheets.Add( Resources.Load<StyleSheet>( $"Styles/BaseGraphEditorView" ) );

            var toolbar = new IMGUIContainer( () =>
            {
                GUILayout.BeginHorizontal( EditorStyles.toolbar );
                if( GUILayout.Button( "Save Asset", EditorStyles.toolbarButton ) )
                {
                    SaveRequested?.Invoke();
                }
                GUILayout.Space( 6 );
                if( GUILayout.Button( "Show In Project", EditorStyles.toolbarButton ) )
                {
                    ShowInProjectRequested?.Invoke();
                }

                GUILayout.FlexibleSpace();

                foreach( var button in CustomButtons )
                {
                    if( GUILayout.Button( button.Title, EditorStyles.toolbarButton ) )
                    {
                        button.Action?.Invoke();
                    }
                }
                
                GUILayout.EndHorizontal();
            } );
            Add( toolbar );

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
            var searchWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            searchWindowProvider.Init( Graph.Type, NodeCreationRequested );

            SearchWindow.Open( new SearchWindowContext( obj.screenMousePosition ), searchWindowProvider );
        }

        private bool NodeCreationRequested( INodeType nodeType, Vector2 screenMousePosition )
        {
            var newNode = new NodeModel( nodeType );
            nodeType.PostLoad( newNode );
            
            var windowRoot = Window.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo( windowRoot.parent, screenMousePosition - Window.position.position );
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal( windowMousePosition );

            newNode.Position = graphMousePosition;
            _graph.AddNode( newNode );

            return true;
        }
    }
}
