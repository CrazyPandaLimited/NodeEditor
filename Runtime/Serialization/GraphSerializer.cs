using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public static class GraphSerializer
    {
        internal static DefaultTypeResolver StaticResolver { get; } = new DefaultTypeResolver();

        public static string Serialize( GraphModel graph )
        {
            if( graph == null )
                throw new ArgumentNullException( nameof( graph ) );

            var sgraph = new SGraph { Type = StaticResolver.GetTypeName( graph.Type ) };

            foreach( var nodeModel in graph.Nodes )
            {
                sgraph.AddNode( nodeModel );
            }

            foreach( var connectionModel in graph.Connections )
            {
                sgraph.AddConnection( connectionModel );
            }

            sgraph.SetSettings( graph.GraphSettings );

            return Serialize( sgraph );
        }
        
        public static string Serialize( SGraph graph )
        {
            if( graph == null )
                throw new ArgumentNullException( nameof( graph ) );

            foreach( var node in graph.Nodes )
            {
                var typeResolver = graph.GraphType as IGraphTypeResolver ?? StaticResolver;
                node.Type = typeResolver.GetTypeName( node.NodeType ) ?? throw new InvalidOperationException( $"Cannot resolve name for node {node}" );
                node.Properties = JsonConvert.SerializeObject( node.PropertyBlock );
            }

            return JsonConvert.SerializeObject( graph, Formatting.Indented );
        }

        public static SGraph DeserializeSGraph( string data )
        {
            var graph = JsonConvert.DeserializeObject< SGraph >( data );
            var nodeResolver = graph.GraphType as IGraphTypeResolver ?? StaticResolver;

            foreach( var n in graph.Nodes )
            {
                n.NodeType = nodeResolver.GetInstance< INodeType >( n.Type ) ?? throw new InvalidOperationException( $"Node type {n.Type ?? "<null>"} not found!" );
                n.NodeType.Init( n );
                JsonConvert.PopulateObject( n.Properties, n.PropertyBlock );
                n.NodeType.PostLoad( n );
                foreach( var port in n.Ports )
                {
                    port.Connections.AddRange( graph.GetConnections( port ) );
                }
            }
            
            return graph;
        }

        public static GraphModel Deserialize( string data, List<SConnection> brokenConnections = null )
        {
            if( data == null )
                throw new ArgumentNullException( nameof( data ) );

            SGraph sgraph = DeserializeSGraph( data );
            var ret = new GraphModel( sgraph.GraphType );

            sgraph.AddContentsToGraph( ret, brokenConnections );
            
            ret.Type.PostLoad( ret );

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

        public static SGraph DeserializeSGraphFromGuid( string assetGuid )
        {
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath( assetGuid );
            var graphText = File.ReadAllText( path, Encoding.UTF8 );

            return DeserializeSGraph( graphText );
#else
            throw new InvalidOperationException( $"{nameof(DeserializeFromGuid)} may be called only from editor" );
#endif
        }

        public static void AddContentsToGraph( this SGraph graph, GraphModel graphModel, List<SConnection> brokenConnections = null )
        {
            graphModel.AddNodes( graph.Nodes );
            graphModel.AddConnections( graph.Connections, brokenConnections );
        }
    }
}
