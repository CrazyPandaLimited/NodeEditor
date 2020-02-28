using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    static class SerializationHelper
    {
        [Serializable]
        public class SGraph
        {
            public string Type;

            public List<SNode> Nodes = new List<SNode>();
            public List<SConnection> Connections = new List<SConnection>();
        }

        [Serializable]
        public class SNode
        {
            public string Type;

            public string Id;
            public Vector2 Position = default;

            public string Properties;
        }

        [Serializable]
        public class SConnection
        {
            public string FromNodeId;
            public string FromPortId;

            public string ToNodeId;
            public string ToPortId;
        }

        public static string SerializeGraph( GraphModel graph )
        {
            var sgraph = new SGraph();
            sgraph.Type = graph.Type.GetType().FullName;

            foreach( NodeModel node in graph.Nodes )
            {
                sgraph.Nodes.Add( new SNode
                {
                    Id = node.Id,
                    Type = node.Type.GetType().FullName,
                    Position = node.Position,
                    Properties = JsonUtility.ToJson( node.PropertyBlock ),
                } );
            }

            foreach( ConnectionModel connection in graph.Connections )
            {
                sgraph.Connections.Add( new SConnection
                {
                    FromNodeId = (connection.From.Node as NodeModel).Id,
                    FromPortId = (connection.From as PortModel).Id,

                    ToNodeId = (connection.To.Node as NodeModel).Id,
                    ToPortId = (connection.To as PortModel).Id,
                } );
            }

            return JsonUtility.ToJson( sgraph, true );
        }

        public static GraphModel DeserializeGraph( string data, List<SConnection> brokenConnections = null )
        {
            var resolver = new DefaultTypeResolver();

            var sgraph = JsonUtility.FromJson<SGraph>( data );
            var gtype = FindType( sgraph.Type ) ?? throw new InvalidOperationException( $"Graph type {sgraph.Type ?? "<null>"} not found!" );
            var graphType = resolver.GetInstance<IGraphType>( gtype );

            var ret = new GraphModel( graphType );

            foreach( var n in sgraph.Nodes )
            {
                var ntype = FindType( n.Type ) ?? throw new InvalidOperationException( $"Node type {n.Type ?? "<null>"} not found!" );
                var nodeType = resolver.GetInstance<INodeType>( ntype );

                var node = new NodeModel( nodeType )
                {
                    Id = n.Id,
                    Position = n.Position
                };

                JsonUtility.FromJsonOverwrite( n.Properties, node.PropertyBlock );

                nodeType.PostLoad( node );

                ret.AddNode( node );
            }

            foreach( var c in sgraph.Connections )
            {
                var fromNode = ret.Nodes.FirstOrDefault( n => n.Id == c.FromNodeId );
                var toNode = ret.Nodes.FirstOrDefault( n => n.Id == c.ToNodeId );

                var fromPort = fromNode?.Ports?.FirstOrDefault( p => p.Id == c.FromPortId );
                var toPort = toNode?.Ports?.FirstOrDefault( p => p.Id == c.ToPortId );

                if( fromPort != null && toPort != null && fromPort.Direction != toPort.Direction )
                {
                    if( fromPort.Direction == PortDirection.Output )
                        ret.Connect( fromPort, toPort );
                    else
                        ret.Connect( toPort, fromPort );
                }
                else
                {
                    brokenConnections?.Add( c );
                }
            }

            graphType.PostLoad( ret );

            return ret;
        }

        private static Type FindType( string typeName )
        {
            if( string.IsNullOrEmpty( typeName ) )
                return null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach( var assembly in assemblies )
            {
                var type = assembly.GetType( typeName );
                if( type != null )
                    return type;
            }

            return null;
        }
    }
}
