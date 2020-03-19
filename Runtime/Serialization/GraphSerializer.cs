using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    static class GraphSerializer
    {
        private static DefaultTypeResolver _staticResolver = new DefaultTypeResolver();

        public static string Serialize( GraphModel graph )
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
                    Properties = JsonConvert.SerializeObject( node.PropertyBlock ),
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

            return JsonConvert.SerializeObject( sgraph, Formatting.Indented );
        }

        public static GraphModel Deserialize( string data, List<SConnection> brokenConnections = null )
        {
            var resolver = _staticResolver;

            var sgraph = JsonConvert.DeserializeObject<SGraph>( data );
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

                JsonConvert.PopulateObject( n.Properties, node.PropertyBlock );

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

        public static GraphModel DeserializeFromGuid( string assetGuid, List<SConnection> brokenConnections = null )
        {
            var path = AssetDatabase.GUIDToAssetPath( assetGuid );
            var graphText = File.ReadAllText( path, Encoding.UTF8 );

            return Deserialize( graphText, brokenConnections );
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
    }
}
