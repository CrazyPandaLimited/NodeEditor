using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public static class SgraphExtensions
    {
        /// <summary>
        /// Returns all input ports of a node
        /// </summary>
        /// <param name="node">Node to look for ports</param>
        public static IEnumerable<SPort> InputPorts( this SNode node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Input );
        }
        
        /// <summary>
        /// Creates a node of given type
        /// </summary>
        /// <param name="nodeType">Type of node to create</param>
        public static SNode CreateSNode( this INodeType nodeType )
        {
            var ret = new SNode
            {
                NodeType = nodeType
            };
            
            nodeType.Init( ret );
            nodeType.PostLoad( ret );
            
            return ret;
        }

        public static IEnumerable< SConnection > GetConnections( this SGraph graph, SPort port )
        {
            return graph.Connections.Where( connection => ( connection.FromPortId == port.Id && connection.FromNodeId == port.NodeId ) ||
                                                          ( connection.ToPortId == port.Id && connection.ToNodeId == port.NodeId ) );
        }
    }
}