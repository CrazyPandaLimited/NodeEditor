using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    sealed class NodesCopyPasteHelper
    {
        private Vector2 _currentMousePosition;
        private readonly BaseGraphView _graphView;

        public NodesCopyPasteHelper( BaseGraphView graphView )
        {
            _graphView = graphView ?? throw new ArgumentNullException( nameof(graphView) );
            
            _graphView.serializeGraphElements = SerializeGraphElements;
            _graphView.canPasteSerializedData = CanPasteSerializedData;
            _graphView.unserializeAndPaste = UnserializeAndPaste;

            _graphView.RegisterCallback<MouseMoveEvent>( evt => _currentMousePosition = evt.mousePosition);
            _graphView.RegisterCallback<MouseEnterEvent>( evt => _currentMousePosition = evt.mousePosition);
            _graphView.RegisterCallback<MouseLeaveEvent>( evt => _currentMousePosition = evt.mousePosition);
        }

        private bool CanPasteSerializedData( string data )
        {
            try
            {
                return GraphSerializer.DeserializeToSGraph( data ) != null;
            }
            catch
            {
                return false;
            }
        }

        private string SerializeGraphElements( IEnumerable< GraphElement > elements )
        {
            var nodes = elements.OfType< BaseNodeView >().ToDictionary( node=> node.Node.Id, node => node );
            var connections = elements.OfType< BaseConnectionView >().Where( view => nodes.ContainsKey( view.Connection.From.Node.Id ) && 
                                                                                     nodes.ContainsKey( view.Connection.To.Node.Id ) );

            return GraphSerializer.SerializeSGraph( CreateGraphToSerialize( nodes.Values, connections ) );
        }

        private void UnserializeAndPaste( string operationName, string data )
        {
            PasteNewNodes( data );
        }

        private void PasteNewNodes( string rawData )
        {
            GraphSerializer.SGraph graph = GraphSerializer.DeserializeToSGraph( rawData );

            GenerateNewNodeIds( graph );
            UpdateNodesPosition( graph );
            
            graph.AddContentsToGraph( _graphView.Graph );
        }
        
        private void GenerateNewNodeIds( GraphSerializer.SGraph graph )
        {
            foreach( var node in graph.Nodes )
            {
                var newNodeId = Guid.NewGuid().ToString();
                var fromConnections = graph.Connections.Where( connection => connection.FromNodeId == node.Id );
                var toConnections = graph.Connections.Where( connection => connection.ToNodeId == node.Id );

                foreach( var fromConnection in fromConnections )
                {
                    fromConnection.FromNodeId = newNodeId;
                }
                
                foreach( var toConnection in toConnections )
                {
                    toConnection.ToNodeId = newNodeId;
                }
                
                node.Id = newNodeId;
            }
        }
        
        private void UpdateNodesPosition(GraphSerializer.SGraph graph)
        {
            var mouseLocalPosition = _graphView.contentViewContainer.WorldToLocal(_currentMousePosition);

            var nodePositionOffset = mouseLocalPosition - FindSelectedNodesTopLeftCorner( graph.Nodes );

            foreach( var node in graph.Nodes )
            {
                node.Position += nodePositionOffset;
            }
        }
        
        private Vector2 FindSelectedNodesTopLeftCorner( IEnumerable< GraphSerializer.SNode > nodes )
        {
            var x = nodes.Min( node => node.Position.x );
            var y = nodes.Min( node => node.Position.y );
            return new Vector2( x, y );
        }

        private GraphSerializer.SGraph CreateGraphToSerialize( IEnumerable< BaseNodeView > nodes, IEnumerable< BaseConnectionView > connections )
        {
            var sgraph = new GraphSerializer.SGraph { Type = _graphView.Graph.Type.GetType().FullName };
           
            foreach( var nodeModel in nodes )
            {
                GraphSerializer.SNode sNode = nodeModel.Node;
                sNode.Position = nodeModel.localBound.min;
                sgraph.Nodes.Add( sNode );
            }

            foreach( var connectionModel in connections )
            {
                sgraph.Connections.Add( connectionModel.Connection );
            }

            return sgraph;
        }
    }
}