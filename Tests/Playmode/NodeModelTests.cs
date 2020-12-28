using NSubstitute;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    class NodeModelTests
    {
        [Test]
        public void Ctor_Should_Succeed()
        {
            var nodeType = Substitute.For<INodeType>();
            var node = new NodeModel( nodeType );

            Assert.That( node.Type, Is.EqualTo( nodeType ) );
            nodeType.Received().Init( node );
        }

        [Test]
        public void Ctor_Should_Throw_WhenNullNodeType()
        {
            Assert.That( () => new NodeModel( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void Properties_Should_Throw_OnSecondSet()
        {
            var graphType = Substitute.For<IGraphType>();
            var nodeType = Substitute.For<INodeType>();
            var node = new NodeModel( nodeType )
            {
                Id = "id",
                PropertyBlock = new PropertyBlock(),
                Graph = new GraphModel( graphType )
            };

            Assert.That( () => node.Id = "new id", Throws.InvalidOperationException );
            Assert.That( () => node.PropertyBlock = new PropertyBlock(), Throws.InvalidOperationException );
            Assert.That( () => node.Graph = new GraphModel( graphType ), Throws.InvalidOperationException );
        }

        [Test]
        public void AddPort_Should_Succeed()
        {
            var node = new NodeModel( Substitute.For<INodeType>() );

            var port = PortModel.Create<string>( "Id", PortDirection.Input );

            NodeModel.NodePortsChangedArgs args = default;
            node.PortsChanged += e => { args = e; };
            node.AddPort( port );

            Assert.That( node.Ports, Does.Contain( port ) );
            Assert.That( port.Node, Is.EqualTo( node ) );
            Assert.That( args.Node, Is.EqualTo( node ) );
            Assert.That( args.Port, Is.EqualTo( port ) );
            Assert.That( args.IsAdded, Is.True );
        }

        [Test]
        public void AddPort_Should_Throw_WhenNullPort()
        {
            var node = new NodeModel( Substitute.For<INodeType>() );

            Assert.That( () => node.AddPort( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void AddPort_Should_Throw_WhenIdIsOccupied()
        {
            var node = new NodeModel( Substitute.For<INodeType>() );
            var port1 = PortModel.Create<string>( "Id", PortDirection.Input );
            var port2 = PortModel.Create<string>( "Id", PortDirection.Input );

            node.AddPort( port1 );

            Assert.That( () => node.AddPort( port2 ), Throws.ArgumentException );
        }

        [Test]
        public void RemovePort_Should_Succeed_WithPort()
        {
            var node = new NodeModel( Substitute.For<INodeType>() );

            var port = PortModel.Create<string>( "Id", PortDirection.Input );
            node.AddPort( port );

            NodeModel.NodePortsChangedArgs args = default;
            node.PortsChanged += e => { args = e; };
            node.RemovePort( port );

            Assert.That( node.Ports, Does.Not.Contains( port ) );
            Assert.That( port.Node, Is.Null );
            Assert.That( args.Node, Is.EqualTo( node ) );
            Assert.That( args.Port, Is.EqualTo( port ) );
            Assert.That( args.IsAdded, Is.False );
        }

        [Test]
        public void RemovePort_Should_Succeed_WithPortId()
        {
            var node = new NodeModel( Substitute.For<INodeType>() );

            var port = PortModel.Create<string>( "Id", PortDirection.Input );
            node.AddPort( port );

            NodeModel.NodePortsChangedArgs args = default;
            node.PortsChanged += e => { args = e; };
            node.RemovePort( port.Id );

            Assert.That( node.Ports, Does.Not.Contains( port ) );
            Assert.That( port.Node, Is.Null );
            Assert.That( args.Node, Is.EqualTo( node ) );
            Assert.That( args.Port, Is.EqualTo( port ) );
            Assert.That( args.IsAdded, Is.False );
        }

        [Test]
        public void RemovePort_Should_Throw_WhenNullPort()
        {
            var node = new NodeModel( Substitute.For<INodeType>() );

            Assert.That( () => node.RemovePort( null as PortModel ), Throws.ArgumentNullException );
        }

        [Test]
        public void RemovePort_Should_Throw_WhenNullPortId()
        {
            var node = new NodeModel( Substitute.For<INodeType>() );

            Assert.That( () => node.RemovePort( null as string ), Throws.ArgumentNullException );
        }

        [Test]
        public void RemovePort_Should_Throw_WhenPortNotFound()
        {
            var node = new NodeModel( Substitute.For<INodeType>() );

            Assert.That( () => node.RemovePort( PortModel.Create<string>( "Id", PortDirection.Input ) ), Throws.ArgumentException );
        }
    }
}