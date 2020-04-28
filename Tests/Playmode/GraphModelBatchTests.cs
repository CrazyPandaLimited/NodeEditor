using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    class GraphModelBatchTests
    {
        [Test]
        public void AddNode_Should_Batch()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );

            IReadOnlyList<NodeModel> addedNodes = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { addedNodes = an; };

            using( graph.BeginChangeSet() )
            {
                graph.AddNode( node1 );
                graph.AddNode( node2 );

                Assert.That( addedNodes, Is.Null );
            }

            Assert.That( addedNodes, Is.Not.Null );
            Assert.That( addedNodes, Is.EquivalentTo( new[] { node1, node2 } ) );
        }

        [Test]
        public void AddNode_Should_Batch_WithRemoveNode()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );

            IReadOnlyList<NodeModel> addedNodes = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { addedNodes = an; };

            using( graph.BeginChangeSet() )
            {
                graph.AddNode( node1 );
                graph.AddNode( node2 );

                graph.RemoveNode( node1 );

                Assert.That( addedNodes, Is.Null );
            }

            Assert.That( addedNodes, Is.Not.Null );
            Assert.That( addedNodes, Is.EquivalentTo( new[] { node2 } ) );
        }

        [Test]
        public void RemoveNode_Should_Batch()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            IReadOnlyList<NodeModel> removedNodes = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { removedNodes = rn; };

            using( graph.BeginChangeSet() )
            {
                graph.RemoveNode( node1 );
                graph.RemoveNode( node2 );

                Assert.That( removedNodes, Is.Null );
            }

            Assert.That( removedNodes, Is.Not.Null );
            Assert.That( removedNodes, Is.EquivalentTo( new[] { node1, node2 } ) );
        }

        [Test]
        public void RemoveNode_Should_Batch_WithAddNode()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            IReadOnlyList<NodeModel> removedNodes = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { removedNodes = rn; };

            using( graph.BeginChangeSet() )
            {
                graph.RemoveNode( node1 );
                graph.RemoveNode( node2 );

                graph.AddNode( node1 );

                Assert.That( removedNodes, Is.Null );
            }

            Assert.That( removedNodes, Is.Not.Null );
            Assert.That( removedNodes, Is.EquivalentTo( new[] { node2 } ) );
        }

        [Test]
        public void Connect_Should_Batch()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );

            var port1 = PortModel.Create<string>( "Out1", PortDirection.Output );
            var port2 = PortModel.Create<string>( "Out2", PortDirection.Output );
            var port3 = PortModel.Create<string>( "In1", PortDirection.Input );
            var port4 = PortModel.Create<string>( "In2", PortDirection.Input );

            node1.AddPort( port1 );
            node1.AddPort( port2 );
            node2.AddPort( port3 );
            node2.AddPort( port4 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            IReadOnlyList<ConnectionModel> addedConnections = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { addedConnections = ac; };

            ConnectionModel connection1, connection2;
            using( graph.BeginChangeSet() )
            {
                connection1 = graph.Connect( port1, port3 );
                connection2 = graph.Connect( port2, port4 );

                Assert.That( addedConnections, Is.Null );
            }

            Assert.That( addedConnections, Is.Not.Null );
            Assert.That( addedConnections, Is.EquivalentTo( new[] { connection1, connection2 } ) );
        }

        [Test]
        public void Connect_Should_Batch_WithDisconnet()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );

            var port1 = PortModel.Create<string>( "Out1", PortDirection.Output );
            var port2 = PortModel.Create<string>( "Out2", PortDirection.Output );
            var port3 = PortModel.Create<string>( "In1", PortDirection.Input );
            var port4 = PortModel.Create<string>( "In2", PortDirection.Input );

            node1.AddPort( port1 );
            node1.AddPort( port2 );
            node2.AddPort( port3 );
            node2.AddPort( port4 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            IReadOnlyList<ConnectionModel> addedConnections = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { addedConnections = ac; };

            ConnectionModel connection1, connection2;
            using( graph.BeginChangeSet() )
            {
                connection1 = graph.Connect( port1, port3 );
                connection2 = graph.Connect( port2, port4 );

                graph.Disconnect( connection1 );

                Assert.That( addedConnections, Is.Null );
            }

            Assert.That( addedConnections, Is.Not.Null );
            Assert.That( addedConnections, Is.EquivalentTo( new[] { connection2 } ) );
        }

        [Test]
        public void Disconnect_Should_Batch()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );

            var port1 = PortModel.Create<string>( "Out1", PortDirection.Output );
            var port2 = PortModel.Create<string>( "Out2", PortDirection.Output );
            var port3 = PortModel.Create<string>( "In1", PortDirection.Input );
            var port4 = PortModel.Create<string>( "In2", PortDirection.Input );

            node1.AddPort( port1 );
            node1.AddPort( port2 );
            node2.AddPort( port3 );
            node2.AddPort( port4 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            var connection1 = graph.Connect( port1, port3 );
            var connection2 = graph.Connect( port2, port4 );

            IReadOnlyList<ConnectionModel> removedConnections = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { removedConnections = rc; };

            using( graph.BeginChangeSet() )
            {
                graph.Disconnect( connection1 );
                graph.Disconnect( connection2 );

                Assert.That( removedConnections, Is.Null );
            }

            Assert.That( removedConnections, Is.Not.Null );
            Assert.That( removedConnections, Is.EquivalentTo( new[] { connection1, connection2 } ) );
        }

        [Test]
        public void Disconnect_Should_Batch_WithConnect()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );

            var port1 = PortModel.Create<string>( "Out1", PortDirection.Output );
            var port2 = PortModel.Create<string>( "Out2", PortDirection.Output );
            var port3 = PortModel.Create<string>( "In1", PortDirection.Input );
            var port4 = PortModel.Create<string>( "In2", PortDirection.Input );

            node1.AddPort( port1 );
            node1.AddPort( port2 );
            node2.AddPort( port3 );
            node2.AddPort( port4 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            var connection1 = graph.Connect( port1, port3 );
            var connection2 = graph.Connect( port2, port4 );

            IReadOnlyList<ConnectionModel> removedConnections = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { removedConnections = rc; };

            using( graph.BeginChangeSet() )
            {
                graph.Disconnect( connection1 );
                graph.Disconnect( connection2 );

                var connection3 = graph.Connect( port1, port3 );

                Assert.That( removedConnections, Is.Null );
                Assert.That( connection3, Is.SameAs( connection1 ) );
            }

            Assert.That( removedConnections, Is.Not.Null );
            Assert.That( removedConnections, Is.EquivalentTo( new[] { connection2 } ) );
        }

        [Test]
        public void BeginChange_Should_Throw_WhenChangeSetInProgress()
        {
            var graph = new GraphModel( Substitute.For<IGraphType>() );
            var changeSet = graph.BeginChangeSet();

            Assert.That( () => graph.BeginChangeSet(), Throws.InvalidOperationException );
        }
    }
}