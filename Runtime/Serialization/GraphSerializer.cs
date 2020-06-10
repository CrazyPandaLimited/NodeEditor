using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    static class GraphSerializer
    {
        private static DefaultTypeResolver _staticResolver = new DefaultTypeResolver();

        public static string Serialize( GraphModel graph )
        {
            if( graph == null )
                throw new ArgumentNullException( nameof( graph ) );

            var sgraph = new SGraph { Type = graph.Type.GetType().FullName };

            foreach( var nodeModel in graph.Nodes )
            {
                sgraph.Nodes.Add( nodeModel );
            }

            foreach( var connectionModel in graph.Connections )
            {
                sgraph.Connections.Add( connectionModel );
            }

            return SerializeSGraph( sgraph );
        }
        
        public static string SerializeSGraph( SGraph graph )
        {
            if( graph == null )
                throw new ArgumentNullException( nameof( graph ) );

            return JsonConvert.SerializeObject( graph, Formatting.Indented );
        }

        public static SGraph DeserializeToSGraph( string data )
        {
            return JsonConvert.DeserializeObject< SGraph >( data );
        }

        public static GraphModel Deserialize( string data, List<SConnection> brokenConnections = null )
        {
            if( data == null )
                throw new ArgumentNullException( nameof( data ) );

            SGraph sgraph = DeserializeToSGraph( data );
            var graphType = _staticResolver.GetInstance< IGraphType >( sgraph.Type ) ?? throw new InvalidOperationException( $"Graph type {sgraph.Type ?? "<null>"} not found!" );
            
            var ret = new GraphModel( graphType );

            sgraph.AddContentsToGraph( ret, brokenConnections );
            
            graphType.PostLoad( ret );

            return ret;
        }

        public static GraphModel DeserializeFromGuid( string assetGuid, List<SConnection> brokenConnections = null )
        {
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath( assetGuid );
            var graphText = File.ReadAllText( path, Encoding.UTF8 );

            return Deserialize( graphText, brokenConnections );
#else
            throw new InvalidOperationException( $"{nameof(DeserializeFromGuid)} may be called only from editor" );
#endif
        }

        public static void AddContentsToGraph( this SGraph graph, GraphModel graphModel, List<SConnection> brokenConnections = null )
        {
            var graphType = _staticResolver.GetInstance< IGraphType >( graph.Type ) ?? throw new InvalidOperationException( $"Graph type {graph.Type ?? "<null>"} not found!" );
            var nodeResolver = graphType as IGraphTypeResolver ?? _staticResolver;

            foreach( var n in graph.Nodes )
            {
                var nodeType = nodeResolver.GetInstance< INodeType >( n.Type ) ?? throw new InvalidOperationException( $"Node type {n.Type ?? "<null>"} not found!" );

                var node = new NodeModel( nodeType )
                {
                    Id = n.Id,
                    Position = n.Position
                };

                JsonConvert.PopulateObject( n.Properties, node.PropertyBlock );

                nodeType.PostLoad( node );

                graphModel.AddNode( node );
            }
            
            foreach( var c in graph.Connections )
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

            public static implicit operator SNode( NodeModel node )
            {
                return new SNode
                {
                    Id = node.Id,
                    Type = node.Type.GetType().FullName,
                    Position = node.Position,
                    Properties = JsonConvert.SerializeObject( node.PropertyBlock ),
                };
            }
        }

        [Serializable]
        public class SConnection
        {
            public string FromNodeId;
            public string FromPortId;

            public string ToNodeId;
            public string ToPortId;

            public static implicit operator SConnection( ConnectionModel connection )
            {
                return new SConnection
                {
                    FromNodeId = connection.From.Node.Id, 
                    FromPortId = connection.From.Id, 
                    ToNodeId = connection.To.Node.Id, 
                    ToPortId = connection.To.Id,
                };
            }
        }
    }
}
