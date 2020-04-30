using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    class GraphModelTests
    {
        public static IGraphType DefaultType => GraphTypeWithoutConnections;
        public static IGraphType GraphTypeWithConnections { get; } = CreateGraphType( new BaseConnectionType() );
        public static IGraphType GraphTypeWithoutConnections { get; } = CreateGraphType( null );

        [Test]
        public void Ctor_Should_Succeed()
        {
            var graph = new GraphModel( DefaultType );

            Assert.That( graph.Type, Is.EqualTo( DefaultType ) );
        }

        [Test]
        public void Ctor_Should_Throw_WhenNullGraphType()
        {
            Assert.That( () => new GraphModel( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void AddNode_Should_Succeed()
        {
            var graph = new GraphModel( DefaultType );
            var node = new NodeModel( Substitute.For<INodeType>() );

            IReadOnlyList<NodeModel> list = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { list = an; };
            graph.AddNode( node );

            Assert.That( graph.Nodes, Does.Contain( node ) );
            Assert.That( node.Graph, Is.EqualTo( graph ) );

            Assert.That( list, Is.Not.Null );
            Assert.That( list.Count, Is.EqualTo( 1 ) );
            Assert.That( list, Does.Contain( node ) );
        }

        [Test]
        public void AddNode_Should_Throw_WhenNullNode()
        {
            var graph = new GraphModel( DefaultType );

            Assert.That( () => graph.AddNode( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void AddNode_Should_Throw_WhenNodeIsInGraph()
        {
            var graph = new GraphModel( DefaultType );
            var node = new NodeModel( Substitute.For<INodeType>() );

            graph.AddNode( node );

            Assert.That( () => graph.AddNode( node ), Throws.ArgumentException );
        }

        [Test]
        public void RemoveNode_Should_Succeed()
        {
            var graph = new GraphModel( DefaultType );
            var node = new NodeModel( Substitute.For<INodeType>() );

            graph.AddNode( node );

            IReadOnlyList<NodeModel> list = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { list = rn; };
            graph.RemoveNode( node );

            Assert.That( graph.Nodes, Does.Not.Contains( node ) );
            Assert.That( node.Graph, Is.Null );

            Assert.That( list, Is.Not.Null );
            Assert.That( list.Count, Is.EqualTo( 1 ) );
            Assert.That( list, Does.Contain( node ) );
        }

        [Test]
        public void RemoveNode_Should_Throw_WhenNullNode()
        {
            var graph = new GraphModel( DefaultType );

            Assert.That( () => graph.RemoveNode( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void RemoveNode_Should_Throw_WhenNodeIsNotInGraph()
        {
            var graph = new GraphModel( DefaultType );
            var node = new NodeModel( Substitute.For<INodeType>() );

            Assert.That( () => graph.RemoveNode( node ), Throws.ArgumentException );
        }

        [Test]
        public void RemoveNode_Should_Remove_Connections()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var node2 = new NodeModel( Substitute.For<INodeType>() );
            var node3 = new NodeModel( Substitute.For<INodeType>() );

            graph.AddNode( node1 );
            graph.AddNode( node2 );
            graph.AddNode( node3 );

            var port1 = PortModel.Create<string>( "Out", PortDirection.Output );
            var port2 = PortModel.Create<string>( "In", PortDirection.Input );
            var port3 = PortModel.Create<string>( "Out", PortDirection.Output );
            var port4 = PortModel.Create<string>( "In", PortDirection.Input );

            node1.AddPort( port1 );
            node2.AddPort( port2 );
            node2.AddPort( port3 );
            node3.AddPort( port4 );

            var connection1 = graph.Connect( port1, port2 );
            var connection2 = graph.Connect( port3, port4 );

            IReadOnlyList<ConnectionModel> removedConnections = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => removedConnections = rc;

            graph.RemoveNode( node2 );

            Assert.That( graph.Connections, Does.Not.Contains( connection1 ) );
            Assert.That( graph.Connections, Does.Not.Contains( connection2 ) );

            Assert.That( removedConnections, Is.EquivalentTo( new[] { connection1, connection2 } ) );
        }

        [Test]
        public void Connect_Should_Succeed()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var nodeType = Substitute.For<INodeType>();
            var node1 = new NodeModel( nodeType );
            var node2 = new NodeModel( nodeType );

            var port1 = PortModel.Create<string>( "Out", PortDirection.Output );
            var port2 = PortModel.Create<string>( "In", PortDirection.Input );

            node1.AddPort( port1 );
            node2.AddPort( port2 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            IReadOnlyList<ConnectionModel> list = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { list = ac; };

            var connection = graph.Connect( port1, port2 );

            Assert.That( connection, Is.Not.Null );

            Assert.That( graph.Connections, Does.Contain( connection ) );

            Assert.That( port1.Connections, Has.Count.EqualTo( 1 ) );
            Assert.That( port2.Connections, Has.Count.EqualTo( 1 ) );

            Assert.That( port1.Connections, Is.EquivalentTo( port2.Connections ) );

            Assert.That( connection.From, Is.EqualTo( port1 ) );
            Assert.That( connection.To, Is.EqualTo( port2 ) );

            Assert.That( list, Is.Not.Null );
            Assert.That( list.Count, Is.EqualTo( 1 ) );
            Assert.That( list, Does.Contain( connection ) );
        }

        [Test]
        public void Connect_Should_Remove_OtherConnections()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var nodeType = Substitute.For<INodeType>();
            var node1 = new NodeModel( nodeType );
            var node2 = new NodeModel( nodeType );
            var node3 = new NodeModel( nodeType );
            var node4 = new NodeModel( nodeType );

            var port1 = PortModel.Create<string>( "Out", PortDirection.Output, PortCapacity.Single );
            var port2 = PortModel.Create<string>( "In", PortDirection.Input, PortCapacity.Single );
            var port3 = PortModel.Create<string>( "Out", PortDirection.Output, PortCapacity.Single );
            var port4 = PortModel.Create<string>( "In", PortDirection.Input, PortCapacity.Single );

            node1.AddPort( port1 );
            node2.AddPort( port2 );
            node3.AddPort( port3 );
            node4.AddPort( port4 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );
            graph.AddNode( node3 );
            graph.AddNode( node4 );

            var connection1 = graph.Connect( port1, port2 );

            IReadOnlyList<ConnectionModel> removedConnections = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { removedConnections = rc; };

            // ports may accept only one connection, that's why connection1 will be removed
            var connection2 = graph.Connect( port3, port2 );

            Assert.That( graph.Connections, Does.Not.Contains( connection1 ) );
            Assert.That( removedConnections, Is.EquivalentTo( new[] { connection1 } ) );

            // here, connection2 will be removed
            var connection3 = graph.Connect( port3, port4 );

            Assert.That( graph.Connections, Does.Not.Contains( connection2 ) );
            Assert.That( removedConnections, Is.EquivalentTo( new[] { connection2 } ) );
        }

        [Test]
        public void Connect_Should_Throw_WhenNullPorts()
        {
            var graph = new GraphModel( GraphTypeWithConnections );
            var port = PortModel.Create<string>( "", PortDirection.Input );

            Assert.That( () => graph.Connect( null, null ), Throws.ArgumentNullException );
            Assert.That( () => graph.Connect( port, null ), Throws.ArgumentNullException );
            Assert.That( () => graph.Connect( null, port ), Throws.ArgumentNullException );
        }

        [Test]
        public void Connect_Should_Throw_WhenCanNotConnect()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var nodeType = Substitute.For<INodeType>();
            var node1 = new NodeModel( nodeType );
            var node2 = new NodeModel( nodeType );

            var port1 = PortModel.Create<string>( "Out", PortDirection.Input );
            var port2 = PortModel.Create<string>( "In", PortDirection.Input );

            node1.AddPort( port1 );
            node2.AddPort( port2 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            Assert.That( () => graph.Connect( port1, port2 ), Throws.InvalidOperationException );
        }

        [Test]
        public void CanConnect_Should_ReturnFalse_WhenDirectionsIncorrect()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var portIn1 = PortModel.Create<int>( "In", PortDirection.Input );
            var portOut1 = PortModel.Create<int>( "Out", PortDirection.Output );
            node1.AddPort( portIn1 );
            node1.AddPort( portOut1 );

            var node2 = new NodeModel( Substitute.For<INodeType>() );
            var portIn2 = PortModel.Create<int>( "In", PortDirection.Input );
            var portOut2 = PortModel.Create<int>( "Out", PortDirection.Output );
            node2.AddPort( portIn2 );
            node2.AddPort( portOut2 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            Assert.That( graph.CanConnect( portIn1, portIn2 ), Is.False ); // input input
            Assert.That( graph.CanConnect( portOut1, portOut2 ), Is.False ); // output output
            Assert.That( graph.CanConnect( portIn1, portOut2 ), Is.False ); // input output
        }

        [Test]
        public void CanConnect_Should_ReturnFalse_WhenSameNode()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var port1 = PortModel.Create<string>( "1", PortDirection.Output );
            var port2 = PortModel.Create<string>( "2", PortDirection.Input );
            node1.AddPort( port1 );
            node1.AddPort( port2 );

            graph.AddNode( node1 );

            Assert.That( graph.CanConnect( port1, port2 ), Is.False );
        }

        [Test]
        public void CanConnect_Should_ReturnFalse_WhenNoConnectionType()
        {
            var graph = new GraphModel( GraphTypeWithoutConnections );

            var node1 = new NodeModel( Substitute.For<INodeType>() );
            var port1 = PortModel.Create<string>( "", PortDirection.Output );
            node1.AddPort( port1 );

            var node2 = new NodeModel( Substitute.For<INodeType>() );
            var port2 = PortModel.Create<string>( "", PortDirection.Input );
            node2.AddPort( port2 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            Assert.That( graph.CanConnect( port1, port2 ), Is.False );
        }

        [Test]
        public void CanConnect_Should_ReturnFalse_WhenPortsAlreadyConnected()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var nodeType = Substitute.For<INodeType>();
            var node1 = new NodeModel( nodeType );
            var node2 = new NodeModel( nodeType );

            var port1 = PortModel.Create<string>( "Out", PortDirection.Output );
            var port2 = PortModel.Create<string>( "In", PortDirection.Input );

            node1.AddPort( port1 );
            node2.AddPort( port2 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            graph.Connect( port1, port2 );

            Assert.That( graph.CanConnect( port1, port2 ), Is.False );
        }

        [Test]
        public void CanConnect_Should_ReturnFalse_WhenPortsNotInGraph()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var nodeType = Substitute.For<INodeType>();
            var node1 = new NodeModel( nodeType );
            var node2 = new NodeModel( nodeType );

            var port1 = PortModel.Create<string>( "Out", PortDirection.Output );
            var port2 = PortModel.Create<string>( "In", PortDirection.Input );

            Assert.That( graph.CanConnect( port1, port2 ), Is.False );

            node1.AddPort( port1 );
            node2.AddPort( port2 );

            Assert.That( graph.CanConnect( port1, port2 ), Is.False );
        }

        [Test]
        public void Disconnect_Should_Succeed()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            var nodeType = Substitute.For<INodeType>();
            var node1 = new NodeModel( nodeType );
            var node2 = new NodeModel( nodeType );

            var port1 = PortModel.Create<string>( "Out", PortDirection.Output );
            var port2 = PortModel.Create<string>( "In", PortDirection.Input );

            node1.AddPort( port1 );
            node2.AddPort( port2 );

            graph.AddNode( node1 );
            graph.AddNode( node2 );

            var connection = graph.Connect( port1, port2 );

            IReadOnlyList<ConnectionModel> list = null;
            graph.GraphChanged += ( an, rn, ac, rc ) => { list = rc; };

            graph.Disconnect( connection );

            Assert.That( graph.Connections, Does.Not.Contains( connection ) );

            Assert.That( port1.Connections, Does.Not.Contains( connection ) );
            Assert.That( port2.Connections, Does.Not.Contains( connection ) );

            Assert.That( connection.From, Is.Null );
            Assert.That( connection.To, Is.Null );

            Assert.That( list, Is.Not.Null );
            Assert.That( list.Count, Is.EqualTo( 1 ) );
            Assert.That( list, Does.Contain( connection ) );
        }

        [Test]
        public void Disconnect_Should_Throw_WhenNullConnection()
        {
            var graph = new GraphModel( GraphTypeWithConnections );

            Assert.That( () => graph.Disconnect( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void Disconnect_Should_Throw_WhenConnectionNotInGraph()
        {
            var graph = new GraphModel( GraphTypeWithConnections );
            var connection = new ConnectionModel( Substitute.For<IConnectionType>() );

            Assert.That( () => graph.Disconnect( connection ), Throws.InvalidOperationException );
        }

        public static IGraphType CreateGraphType( IConnectionType connectionType )
        {
            var graphType = Substitute.For<IGraphType>();
            graphType.FindConnectionType( Arg.Any<Type>(), Arg.Any<Type>() ).ReturnsForAnyArgs( connectionType );
            return graphType;
        }
    }
}