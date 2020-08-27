using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public static class GraphExtensions
    {
        /// <summary>
        /// Returns ports connected to given <see cref="PortModel"/>
        /// </summary>
        /// <param name="port">Starting port</param>
        public static IEnumerable<PortModel> ConnectedPorts( this PortModel port )
        {
            return port.Connections.Select( c => c.OtherSide( port ) );
        }

        /// <summary>
        /// Returns <see cref="PortModel"/> connected to other side of connection relative to <paramref name="startingPort"/>
        /// </summary>
        /// <param name="connection">Connection to process</param>
        /// <param name="startingPort">Port to process</param>
        public static PortModel OtherSide( this ConnectionModel connection, PortModel startingPort )
        {
            if( connection.From == startingPort )
                return connection.To;
            else if( connection.To == startingPort )
                return connection.From;
            else
                return null;
        }

        /// <summary>
        /// Returns port with given id
        /// </summary>
        /// <param name="node">Node to look for ports</param>
        /// <param name="id">Id of a port to search</param>
        public static PortModel Port( this NodeModel node, string id )
        {
            return node.Ports.FirstOrDefault( p => p.Id == id );
        }

        /// <summary>
        /// Returns all input ports of a node
        /// </summary>
        /// <param name="node">Node to look for ports</param>
        public static IEnumerable<PortModel> InputPorts( this NodeModel node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Input );
        }

        /// <summary>
        /// Returns all output ports of a node
        /// </summary>
        /// <param name="node">Node to look for ports</param>
        public static IEnumerable<PortModel> OutputPorts( this NodeModel node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Output );
        }

        /// <summary>
        /// Returns all input connections of a node
        /// </summary>
        /// <param name="node">Node to look for connections</param>
        public static IEnumerable<ConnectionModel> InputConnections( this NodeModel node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Input ).SelectMany( p => p.Connections );
        }

        /// <summary>
        /// Returns all output connections of a node
        /// </summary>
        /// <param name="node">Node to look for connections</param>
        public static IEnumerable<ConnectionModel> OutputConnections( this NodeModel node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Output ).SelectMany( p => p.Connections );
        }

        /// <summary>
        /// Returns connection for give port id
        /// </summary>
        /// <param name="node">Node to look for connection</param>
        /// <param name="inputPortId">Id of a port to search</param>
        public static ConnectionModel InputConnection( this NodeModel node, string inputPortId )
        {
            return node.InputPorts().FirstOrDefault( p => p.Id == inputPortId )?.Connections?.SingleOrDefault();
        }

        /// <summary>
        /// Creates a node of given type
        /// </summary>
        /// <param name="nodeType">Type of node to create</param>
        public static NodeModel CreateNode( this INodeType nodeType )
        {
            var ret = new NodeModel( nodeType );
            nodeType.PostLoad( ret );
            return ret;
        }
    }
}