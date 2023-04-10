using NSubstitute;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    class ConnectionModelTests
    {
        [Test]
        public void Ctor_Should_Succeed()
        {
            var connectionType = Substitute.For<IConnectionType>();
            var connection = new ConnectionModel( connectionType );

            Assert.That( connection.Type, Is.EqualTo( connectionType ) );
            connectionType.Received().InitModel( connection );
        }

        [Test]
        public void Properties_Should_Throw_OnSecondSet()
        {
            var connection = new ConnectionModel( Substitute.For<IConnectionType>() );

            connection.From = new PortModel( "", typeof( string ), PortDirection.Input );
            connection.To = new PortModel( "", typeof( string ), PortDirection.Input );

            Assert.That( () => connection.From = new PortModel( "", typeof( string ), PortDirection.Input ), Throws.InvalidOperationException );
            Assert.That( () => connection.To = new PortModel( "", typeof( string ), PortDirection.Input ), Throws.InvalidOperationException );
        }
    }
}