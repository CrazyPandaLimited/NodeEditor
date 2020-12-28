using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        
        public static void AddNodes( this GraphModel graphModel, IEnumerable< SNode > nodes )
        {
            foreach( var node in nodes )
            {
                graphModel.AddNode( node );
            }
        }

        public static void AddConnections( this GraphModel graphModel, IEnumerable< SConnection > connections, List<SConnection> brokenConnections = null )
        {
            foreach( var c in connections )
            {
                var fromNode = graphModel.Nodes.FirstOrDefault( n => n.Id == c.FromNodeId );
                var toNode = graphModel.Nodes.FirstOrDefault( n => n.Id == c.ToNodeId );

                var fromPort = fromNode?.Ports?.FirstOrDefault( p => p.Id == c.FromPortId );
                var toPort = toNode?.Ports?.FirstOrDefault( p => p.Id == c.ToPortId );

                if( fromPort != null && toPort != null && fromPort.Direction != toPort.Direction )
                {
                    if( fromPort.Direction == PortDirection.Output )
                        graphModel.Connect( fromPort, toPort );
                    else
                        graphModel.Connect( toPort, fromPort );
                }
                else
                {
                    brokenConnections?.Add( c );
                }
            }
        }

        public static void RemoveNodes( this GraphModel graph, IEnumerable< SNode > nodes )
        {
            foreach( var sNode in nodes )
            {
                var nodeToRemove = graph.Nodes.FirstOrDefault( node => node.Id == sNode.Id );

                if( nodeToRemove != null )
                {
                    graph.RemoveNode( nodeToRemove );
                }
            }
        }

        public static void RemoveConnections( this GraphModel graph, IEnumerable< SConnection > connections )
        {
            foreach( var sConnection in connections )
            {
                var connectionToRemove = graph.Connections.FirstOrDefault( connection => sConnection.Equals( connection ));

                if( connectionToRemove != null )
                {
                    graph.Disconnect( connectionToRemove );
                }
            }
        }
    }
}