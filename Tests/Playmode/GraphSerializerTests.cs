using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    class GraphSerializerTests
    {
        [Test]
        public void SerializeDeserialize_Should_Succeed_WithGraphType()
        {
            var sourceGraph = CreateGraph();
            var newGraph = SaveLoadGraph( sourceGraph );

            Assert.That( newGraph.Type.GetType(), Is.EqualTo( sourceGraph.Type.GetType() ) );
        }

        [Test]
        public void SerializeDeserialize_Should_Succeed_WithNodes()
        {
            var sourceGraph = CreateGraph();
            var newGraph = SaveLoadGraph( sourceGraph );

            Assert.That( newGraph.Nodes.Count, Is.EqualTo( sourceGraph.Nodes.Count ) );

            for( int i = 0; i < newGraph.Nodes.Count; ++i )
            {
                var sourceNode = sourceGraph.Nodes[ i ];
                var newNode = newGraph.Nodes[ i ];

                var sourceNodeType = sourceNode.Type.GetType();
                var newNodeType = newNode.Type.GetType();

                Assert.That( newNode.Id, Is.EqualTo( sourceNode.Id ) );
                Assert.That( newNodeType, Is.EqualTo( sourceNodeType ) );
            }
        }

        [Test]
        public void SerializeDeserialize_Should_Succeed_WithConnections()
        {
            var sourceGraph = CreateGraph();
            var newGraph = SaveLoadGraph( sourceGraph );

            Assert.That( newGraph.Connections.Count, Is.EqualTo( sourceGraph.Connections.Count ) );

            for( int i = 0; i < newGraph.Connections.Count; ++i )
            {
                var sourceConnection = sourceGraph.Connections[ i ];
                var newConnection = newGraph.Connections[ i ];

                Assert.That( newConnection.From.Id, Is.EqualTo( sourceConnection.From.Id ) );
                Assert.That( newConnection.To.Id, Is.EqualTo( sourceConnection.To.Id ) );
            }
        }

        [Test]
        public void SerializeDeserialize_Should_Succeed_WithProperties()
        {
            var sourceGraph = CreateGraph();
            var newGraph = SaveLoadGraph( sourceGraph );

            var sourceNode = sourceGraph.Nodes.FirstOrDefault( n => n.Type is NodeWithProperties );
            var newNode = sourceGraph.Nodes.FirstOrDefault( n => n.Type is NodeWithProperties );

            Assert.That( sourceNode, Is.Not.Null );
            Assert.That( newNode, Is.Not.Null );

            Assert.That( sourceNode.PropertyBlock, Is.Not.Null );
            Assert.That( newNode.PropertyBlock, Is.Not.Null );

            Assert.That( newNode.PropertyBlock.GetType(), Is.EqualTo( sourceNode.PropertyBlock.GetType() ) );

            var sourceProperties = (sourceNode.PropertyBlock as NodeWithProperties.Properties);
            var newProperties = (newNode.PropertyBlock as NodeWithProperties.Properties);

            Assert.That( newProperties.Value, Is.EqualTo( sourceProperties.Value ) );
        }

        [Test]
        public void Serialize_Should_Throw_WhenNullGraph()
        {
            Assert.That( () => GraphSerializer.Serialize( ( GraphModel ) null ), Throws.ArgumentNullException );
        }

        [Test]
        public void Deserialize_Should_Throw_WhenNullGraph()
        {
            Assert.That( () => GraphSerializer.Deserialize( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void Deserialize_Should_Throw_WhenGraphTypeNotFound()
        {
            var sgraph = new SGraph() { Type = "NonExistingType" };
            var str = JsonConvert.SerializeObject( sgraph );

            Assert.That( () => GraphSerializer.Deserialize( str ), Throws.InvalidOperationException );
        }

        [Test]
        public void Deserialize_Should_Throw_WhenNodeTypeNotFound()
        {
            var sgraph = new SGraph() { Type = typeof( GraphType ).FullName };
            var snode = new SNode() { Type = "NonExistingType" };
            sgraph.AddNode( snode );

            var str = JsonConvert.SerializeObject( sgraph );

            Assert.That( () => GraphSerializer.Deserialize( str ), Throws.InvalidOperationException );
        }

        // _ is for not existing node or port
        [TestCase( "_", "_", "1", "In" )]
        [TestCase( "_", "_", "1", "Out" )]
        [TestCase( "_", "_", "1", "_" )]
        [TestCase( "1", "Out", "2", "Out" )]
        [TestCase( "1", "In", "2", "In" )]
        [TestCase( "1", "Out", "2", "_" )]
        [TestCase( "1", "_", "2", "Out" )]
        [TestCase( "1", "_", "2", "In" )]
        [TestCase( "1", "Out", "_", "_" )]
        [TestCase( "1", "_", "_", "_" )]
        public void Deserialize_Should_Return_BrokenConnections( string fromNodeId, string fromPortId, string toNodeId, string toPortId )
        {
            var sgraph = new SGraph() { Type = typeof( GraphType ).FullName };

            var snode1 = new SNode() { Id = "1", Type = typeof( NodeWithProperties ).FullName, Properties = "{}" };
            var snode2 = new SNode() { Id = "2", Type = typeof( NodeWithProperties ).FullName, Properties = "{}" };

            sgraph.AddNode( snode1 );
            sgraph.AddNode( snode2 );

            var sconnection = new SConnection()
            {
                FromNodeId = fromNodeId,
                FromPortId = fromPortId,
                ToNodeId = toNodeId,
                ToPortId = toPortId,
            };

            sgraph.AddConnection( sconnection );

            var brokenConnections = new List<SConnection>();

            var str = JsonConvert.SerializeObject( sgraph );
            GraphSerializer.Deserialize( str, brokenConnections );

            Assert.That( brokenConnections, Has.Count.EqualTo( 1 ) );

            var c = brokenConnections[ 0 ];
            Assert.That( c.FromNodeId, Is.EqualTo( sconnection.FromNodeId ) );
            Assert.That( c.FromPortId, Is.EqualTo( sconnection.FromPortId ) );
            Assert.That( c.ToNodeId, Is.EqualTo( sconnection.ToNodeId ) );
            Assert.That( c.ToPortId, Is.EqualTo( sconnection.ToPortId ) );
        }

        [Test]
        public void Deserialize_Should_Fix_ConnectionDirection()
        {
            var sgraph = new SGraph() { Type = typeof( GraphType ).FullName };

            var snode1 = new SNode() { Id = "1", Type = typeof( NodeWithProperties ).FullName, Properties = "{}" };
            var snode2 = new SNode() { Id = "2", Type = typeof( NodeWithProperties ).FullName, Properties = "{}" };

            sgraph.AddNode( snode1 );
            sgraph.AddNode( snode2 );

            // this connection has inverted direction: In -> Out
            var sconnection = new SConnection()
            {
                FromNodeId = "2",
                FromPortId = "In",
                ToNodeId = "1",
                ToPortId = "Out",
            };

            sgraph.AddConnection( sconnection );

            var str = JsonConvert.SerializeObject( sgraph );
            var graph = GraphSerializer.Deserialize( str );

            Assert.That( graph.Connections.Count, Is.EqualTo( 1 ) );

            var c = graph.Connections[ 0 ];
            Assert.That( c.From.Node.Id, Is.EqualTo( "1" ) );
            Assert.That( c.From.Id, Is.EqualTo( "Out" ) );
            Assert.That( c.To.Node.Id, Is.EqualTo( "2" ) );
            Assert.That( c.To.Id, Is.EqualTo( "In" ) );
        }

        [Test]
        public void SerializeDeserialize_Should_Succeed_WithCustomTypeResolver()
        {
            var graphType = new GraphTypeWithResolver();
            var graph = new GraphModel( graphType );

            var nodeType = new NodeWithProperties();
            var node = nodeType.CreateNode();

            graph.AddNode( node );

            var newGraph = SaveLoadGraph( graph );

            Assert.That( newGraph.Nodes.Count, Is.EqualTo( graph.Nodes.Count ) );
        }

        [Test]
        public void Serialize_Should_Throw_WhenNodeTypeNameNotSupported_ByCustomResolver()
        {
            var graphType = new GraphTypeWithResolver();
            var graph = new GraphModel( graphType );

            var nodeType = new SourceNode();
            var node = nodeType.CreateNode();

            graph.AddNode( node );

            Assert.That( () => GraphSerializer.Serialize( graph ), Throws.InvalidOperationException );
        }

        [Test]
        public void Deserialize_Should_Throw_WhenNodeTypeNotFound_ByCustomResolver()
        {
            var sgraph = new SGraph() { Type = typeof( GraphTypeWithResolver ).FullName };
            var snode = new SNode() { Type = "NonExistingType" };
            sgraph.AddNode( snode );

            var str = JsonConvert.SerializeObject( sgraph );

            Assert.That( () => GraphSerializer.Deserialize( str ), Throws.InvalidOperationException );
        }

        private GraphModel CreateGraph()
        {
            // graph sceme:
            //   node1 -------------> node2 -------------> node3
            //          connection1          connection2

            var graph = new GraphModel( new GraphType() );
            var sourceType = new SourceNode( "test" );
            var transferType = new NodeWithProperties();
            var destType = new DestinationNode();

            var node1 = sourceType.CreateNode();
            graph.AddNode( node1 );

            var node2 = transferType.CreateNode();
            graph.AddNode( node2 );

            var node3 = destType.CreateNode();
            graph.AddNode( node3 );

            var connection1 = graph.Connect( sourceType.Out.FromNode( node1 ), transferType.In.FromNode( node2 ) );
            var connection2 = graph.Connect( transferType.Out.FromNode( node2 ), destType.In.FromNode( node3 ) );

            (node2.PropertyBlock as NodeWithProperties.Properties).Value = "TestValue";

            return graph;
        }

        private GraphModel SaveLoadGraph( GraphModel graph )
        {
            var str = GraphSerializer.Serialize( graph );
            return GraphSerializer.Deserialize( str );
        }

        // we need actual type for serialization to work
        public class GraphType : IGraphType
        {
            public string Name => "";

            public IReadOnlyList<INodeType> AvailableNodes => new INodeType[ 0 ];

            public IConnectionType FindConnectionType( Type from, Type to )
            {
                return new BaseConnectionType();
            }

            public void PostLoad( GraphModel graph )
            {
            }
        }

        class NodeWithProperties : BaseNodeType
        {
            public InputPort<string> In { get; }
            public OutputPort<string> Out { get; }

            protected override PropertyBlock CreatePropertyBlock( INode node )
            {
                return new Properties();
            }

            public class Properties : PropertyBlock
            {
                public string Value;
            }
        }

        public class GraphTypeWithResolver : GraphType, IGraphTypeResolver
        {
            private NodeWithProperties _nodeType = new NodeWithProperties();
            private const string _nodeTypeName = "MyNodeType";

            public T GetInstance< T >( string typeName )
                where T : class
            {
                if( typeName == _nodeTypeName )
                    return _nodeType as T;

                return null;
            }

            public string GetTypeName< T >( T instance )
                where T : class
            {
                if( instance is NodeWithProperties )
                    return _nodeTypeName;

                return null;
            }
        }
    }
}