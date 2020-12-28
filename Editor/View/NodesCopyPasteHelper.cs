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
                return GraphSerializer.DeserializeSGraph( data ) != null;
            }
            catch
            {
                return false;
            }
        }

        private string SerializeGraphElements( IEnumerable< GraphElement > elements )
        {
            var nodes = elements.OfType< BaseNodeView >().ToDictionary( node=> node.Node.Id, node => node );
            var connections = elements.OfType< BaseConnectionView >().Where( view => nodes.ContainsKey( view.Connection.FromNodeId ) && 
                                                                                     nodes.ContainsKey( view.Connection.ToNodeId ) );

            return GraphSerializer.Serialize( CreateGraphToSerialize( nodes.Values, connections ) );
        }

        private void UnserializeAndPaste( string operationName, string data )
        {
            PasteNewNodes( data );
        }

        private void PasteNewNodes( string rawData )
        {
            SGraph graph = GraphSerializer.DeserializeSGraph( rawData );

            GenerateNewNodeIds( graph );
            UpdateNodesPosition( graph );
            
            foreach( var node in graph.Nodes )
            {
                _graphView.Graph.AddNode( node );
            }
            
            foreach( var connection in graph.Connections )
            {
                _graphView.Graph.AddConnection( connection );
            }
        }
        
        private void GenerateNewNodeIds( SGraph graph )
        {
            foreach( var node in graph.Nodes )
            {
                var newNodeId = Guid.NewGuid().ToString();
                var fromConnections = graph.Connections.Where( connection => connection.FromNodeId == node.Id ).ToArray();
                var toConnections = graph.Connections.Where( connection => connection.ToNodeId == node.Id ).ToArray();

                node.Id = newNodeId;
                
                foreach( var fromConnection in fromConnections )
                {
                    fromConnection.FromNodeId = newNodeId;
                }
                
                foreach( var toConnection in toConnections )
                {
                    toConnection.ToNodeId = newNodeId;
                }

                foreach( var port in node.Ports )
                {
                    port.NodeId = newNodeId;
                    port.Connections.Clear();
                    port.Connections.AddRange( graph.GetConnections( port ) );
                }
            }
        }
        
        private void UpdateNodesPosition(SGraph graph)
        {
            var mouseLocalPosition = _graphView.contentViewContainer.WorldToLocal(_currentMousePosition);

            var nodePositionOffset = mouseLocalPosition - FindSelectedNodesTopLeftCorner( graph.Nodes );

            foreach( var node in graph.Nodes )
            {
                node.Position += nodePositionOffset;
            }
        }
        
        private Vector2 FindSelectedNodesTopLeftCorner( IEnumerable< SNode > nodes )
        {
            var x = nodes.Min( node => node.Position.x );
            var y = nodes.Min( node => node.Position.y );
            return new Vector2( x, y );
        }

        private SGraph CreateGraphToSerialize( IEnumerable< BaseNodeView > nodes, IEnumerable< BaseConnectionView > connections )
        {
            var sgraph = new SGraph { Type = _graphView.Graph.Type };
           
            foreach( var nodeModel in nodes )
            {
                SNode sNode = nodeModel.Node;
                sNode.Position = nodeModel.localBound.min;
                sgraph.AddNode( sNode );
            }

            foreach( var connectionModel in connections )
            {
                sgraph.AddConnection( connectionModel.Connection );
            }

            return sgraph;
        }
    }
}