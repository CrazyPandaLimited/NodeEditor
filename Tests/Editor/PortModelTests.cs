using NSubstitute;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    class PortModelTests
    {
        [Test]
        public void Ctor_Should_Succeed_InputWithoutCapacity()
        {
            var inputPort = new PortModel( "In", typeof( string ), PortDirection.Input );

            Assert.That( inputPort.Id, Is.EqualTo( "In" ) );
            Assert.That( inputPort.Type, Is.EqualTo( typeof( string ) ) );
            Assert.That( inputPort.Direction, Is.EqualTo( PortDirection.Input ) );
            Assert.That( inputPort.Capacity, Is.EqualTo( PortCapacity.Single ) );
        }

        [Test]
        public void Ctor_Should_Succeed_OutputWithoutCapacity()
        {
            var outputPort = new PortModel( "Out", typeof( string ), PortDirection.Output );

            Assert.That( outputPort.Id, Is.EqualTo( "Out" ) );
            Assert.That( outputPort.Type, Is.EqualTo( typeof( string ) ) );
            Assert.That( outputPort.Direction, Is.EqualTo( PortDirection.Output ) );
            Assert.That( outputPort.Capacity, Is.EqualTo( PortCapacity.Multiple ) );
        }

        [Test]
        public void Ctor_Should_Succeed_InputWithCapacity()
        {
            var inputPort = new PortModel( "In", typeof( string ), PortDirection.Input, PortCapacity.Multiple );

            Assert.That( inputPort.Id, Is.EqualTo( "In" ) );
            Assert.That( inputPort.Type, Is.EqualTo( typeof( string ) ) );
            Assert.That( inputPort.Direction, Is.EqualTo( PortDirection.Input ) );
            Assert.That( inputPort.Capacity, Is.EqualTo( PortCapacity.Multiple ) );
        }

        [Test]
        public void Ctor_Should_Succeed_OutputWithCapacity()
        {
            var outputPort = new PortModel( "Out", typeof( string ), PortDirection.Output, PortCapacity.Single );

            Assert.That( outputPort.Id, Is.EqualTo( "Out" ) );
            Assert.That( outputPort.Type, Is.EqualTo( typeof( string ) ) );
            Assert.That( outputPort.Direction, Is.EqualTo( PortDirection.Output ) );
            Assert.That( outputPort.Capacity, Is.EqualTo( PortCapacity.Single ) );
        }

        [Test]
        public void Ctor_Should_Throw_WhenNullId()
        {
            Assert.That( () => new PortModel( null, typeof( string ), PortDirection.Input ), Throws.ArgumentNullException );
        }

        [Test]
        public void Ctor_Should_Throw_WhenNullType()
        {
            Assert.That( () => new PortModel( "Id", null, PortDirection.Input ), Throws.ArgumentNullException );
        }

        [Test]
        public void Properties_Should_Throw_OnSecondSet()
        {
            var port = PortModel.Create<string>( "In", PortDirection.Input );

            port.Node = new NodeModel( Substitute.For<INodeType>() );

            Assert.That( () => port.Node = new NodeModel( Substitute.For<INodeType>() ), Throws.InvalidOperationException );
            Assert.That( () => port.Node = null, Throws.Nothing );
        }

        [Test]
        public void CreateT_Should_Succeed_InputWithoutCapacity()
        {
            var inputPort = PortModel.Create<string>( "In", PortDirection.Input );

            Assert.That( inputPort.Id, Is.EqualTo( "In" ) );
            Assert.That( inputPort.Type, Is.EqualTo( typeof( string ) ) );
            Assert.That( inputPort.Direction, Is.EqualTo( PortDirection.Input ) );
            Assert.That( inputPort.Capacity, Is.EqualTo( PortCapacity.Single ) );
        }

        [Test]
        public void CreateT_Should_Succeed_OutputWithoutCapacity()
        {
            var outputPort = PortModel.Create<string>( "Out", PortDirection.Output );

            Assert.That( outputPort.Id, Is.EqualTo( "Out" ) );
            Assert.That( outputPort.Type, Is.EqualTo( typeof( string ) ) );
            Assert.That( outputPort.Direction, Is.EqualTo( PortDirection.Output ) );
            Assert.That( outputPort.Capacity, Is.EqualTo( PortCapacity.Multiple ) );
        }

        [Test]
        public void CreateT_Should_Succeed_InputWithCapacity()
        {
            var inputPort = PortModel.Create<string>( "In", PortDirection.Input, PortCapacity.Multiple );

            Assert.That( inputPort.Id, Is.EqualTo( "In" ) );
            Assert.That( inputPort.Type, Is.EqualTo( typeof( string ) ) );
            Assert.That( inputPort.Direction, Is.EqualTo( PortDirection.Input ) );
            Assert.That( inputPort.Capacity, Is.EqualTo( PortCapacity.Multiple ) );
        }

        [Test]
        public void CreateT_Should_Succeed_OutputWithCapacity()
        {
            var outputPort = PortModel.Create<string>( "Out", PortDirection.Output, PortCapacity.Single );

            Assert.That( outputPort.Id, Is.EqualTo( "Out" ) );
            Assert.That( outputPort.Type, Is.EqualTo( typeof( string ) ) );
            Assert.That( outputPort.Direction, Is.EqualTo( PortDirection.Output ) );
            Assert.That( outputPort.Capacity, Is.EqualTo( PortCapacity.Single ) );
        }

        [Test]
        public void CreateT_Should_Throw_WhenNullId()
        {
            Assert.That( () => PortModel.Create<string>( null, PortDirection.Input ), Throws.ArgumentNullException );
        }
    }
}