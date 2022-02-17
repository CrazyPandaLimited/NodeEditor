using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base class for Graph view
    /// </summary>
    public class BaseGraphView : GraphView
    {
        private readonly IGraphEditorViewFactory _editorViewFactory;
        private readonly IEdgeConnectorListener _edgeConnectorListener;
        private GridBackground _background;
        private Label _graphTypeLabel;

        private SGraph _graph;

        /// <summary>
        /// Current opened <see cref="GraphModel"/>
        /// </summary>
        public SGraph Graph => _graph;

        /// <summary>
        /// Called when elements selected or unselected
        /// </summary>
        public event Action<IReadOnlyList<ISelectable>> SelectionChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="editorViewFactory">Factory to use for creating additional views</param>
        public BaseGraphView( IGraphEditorViewFactory editorViewFactory )
        {
            styleSheets.Add( Resources.Load<StyleSheet>( "Styles/BaseGraphView" ) );

            _editorViewFactory = editorViewFactory;
            _edgeConnectorListener = new EdgeConnectorListener( this );

            _background = new GridBackground() { name = "GridBackground" };
            Add( _background );

            _graphTypeLabel = new Label() { name = "GraphTypeLabel" };
            Add( _graphTypeLabel );

            _graphTypeLabel.SendToBack();
            _background.SendToBack();

            graphViewChanged += OnGraphViewChanged;
        }

        /// <summary>
        /// Loads new <see cref="SGraph"/>
        /// </summary>
        /// <param name="graph">Graph to load</param>
        public void LoadGraph( SGraph graph )
        {
            if( _graph != null )
                _graph.GraphChanged -= OnGraphChanged;

            _graph = graph ?? throw new ArgumentNullException( nameof( graph ) );
            _graph.GraphChanged += OnGraphChanged;

            _graphTypeLabel.text = graph.GraphType.Name.ToUpperInvariant();

            DeleteElements( graphElements.ToList() );

            OnGraphChanged( _graph.Nodes, new SNode[ 0 ], _graph.Connections, new SConnection[ 0 ] );
        }

        public override List<Port> GetCompatiblePorts( Port startPort, NodeAdapter nodeAdapter )
        {
            var ret = new List<Port>();

            var baseStartPort = (startPort as BasePortView).Port;

            ports.ForEach( portView =>
             {
                 var baseEndPort = (portView as BasePortView).Port;

                 var fromPort = baseStartPort.Direction == PortDirection.Input ? baseEndPort : baseStartPort;
                 var toPort = baseStartPort.Direction == PortDirection.Input ? baseStartPort : baseEndPort;

                 if( _graph.CanConnect( fromPort, toPort ) )
                     ret.Add( portView );
             } );

            return ret;
        }

        public override void AddToSelection( ISelectable selectable )
        {
            var prev = selection.Count;
            base.AddToSelection( selectable );

            if( prev != selection.Count )
                SelectionChanged?.Invoke( selection );
        }

        public override void ClearSelection()
        {
            var prev = selection.Count;
            base.ClearSelection();

            if( prev != selection.Count )
                SelectionChanged?.Invoke( selection );
        }

        public override void RemoveFromSelection( ISelectable selectable )
        {
            var prev = selection.Count;
            base.RemoveFromSelection( selectable );

            if( prev != selection.Count )
                SelectionChanged?.Invoke( selection );
        }

        private void OnGraphChanged( IReadOnlyList< SNode > addedNodes, IReadOnlyList< SNode > removedNodes, IReadOnlyList< SConnection > addedConnections, IReadOnlyList< SConnection > removedConnections )
        {
            foreach( var node in addedNodes )
                AddElement( _editorViewFactory.CreateNodeView( node, _edgeConnectorListener ) );

            var elems = graphElements.ToList();

            foreach( var connection in removedConnections )
            {
                var graphConnection = elems.OfType< BaseConnectionView >().FirstOrDefault( c => c.Connection.Equals( connection ) );

                if( graphConnection != null )
                {
                    graphConnection.input.Disconnect( graphConnection );
                    graphConnection.output.Disconnect( graphConnection );

                    graphConnection.input = null;
                    graphConnection.output = null;

                    RemoveElement( graphConnection );
                }
            }

            foreach( var connection in addedConnections )
            {
                var connView = _editorViewFactory.CreateConnectionView( connection );

                var fromNode = elems.OfType<BaseNodeView>().FirstOrDefault( n => n.Node.Id == connection.FromNodeId );
                var toNode = elems.OfType<BaseNodeView>().FirstOrDefault( n => n.Node.Id == connection.ToNodeId );

                var outputPort = fromNode.Ports.FirstOrDefault( p => p.Port.Id == connection.FromPortId );
                var inputPort = toNode.Ports.FirstOrDefault( p => p.Port.Id == connection.ToPortId );

                outputPort.Connect( connView );
                inputPort.Connect( connView );

                connView.output = outputPort;
                connView.input = inputPort;

                AddElement( connView );
            }

            foreach( var node in removedNodes )
            {
                var graphNode = elems.OfType<BaseNodeView>().FirstOrDefault( n => n.Node.Id == node.Id );

                if( graphNode != null )
                {
                    RemoveElement( graphNode );
                }
            }
        }

        private GraphViewChange OnGraphViewChanged( GraphViewChange graphViewChange )
        {
            if( graphViewChange.elementsToRemove != null )
            {
                foreach( var elem in graphViewChange.elementsToRemove )
                {
                    if( elem is BaseNodeView nodeView )
                        _graph.RemoveNode( nodeView.Node );
                    if( elem is BaseConnectionView connectionView )
                        _graph.Disconnect( connectionView.Connection );
                }
            }

            return graphViewChange;
        }
    }
}
