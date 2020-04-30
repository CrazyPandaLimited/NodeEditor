using System;
using System.Linq;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]

    class BaseNodeTypeTests
    {
        [TestCaseSource( nameof( _portsSource ) )]
        public void CreateNode_Should_Create_Port( string id, Type type, PortDirection direction, PortCapacity capacity )
        {
            var nodeType = new NodeWithPorts();
            var node = nodeType.CreateNode();

            var port = node.Ports.FirstOrDefault( p => p.Id == id );

            Assert.That( port, Is.Not.Null );
            Assert.That( port.Type, Is.EqualTo( type ) );
            Assert.That( port.Direction, Is.EqualTo( direction ) );
            Assert.That( port.Capacity, Is.EqualTo( capacity ) );
        }

        private static object[][] _portsSource = new[]
        {
            new object[] { "In", typeof(string), PortDirection.Input, PortCapacity.Single },
            new object[] { "In1", typeof(string), PortDirection.Input, PortCapacity.Single },
            new object[] { "InMulti", typeof(string), PortDirection.Input, PortCapacity.Multiple },
            new object[] { "InName", typeof(int), PortDirection.Input, PortCapacity.Single },
            new object[] { "InFloat", typeof(float), PortDirection.Input, PortCapacity.Single },

            new object[] { "Out", typeof(string), PortDirection.Output, PortCapacity.Multiple },
            new object[] { "Out1", typeof(string), PortDirection.Output, PortCapacity.Multiple },
            new object[] { "OutSingle", typeof(string), PortDirection.Output, PortCapacity.Single },
            new object[] { "OutName", typeof(int), PortDirection.Output, PortCapacity.Multiple },
            new object[] { "OutFloat", typeof(float), PortDirection.Output, PortCapacity.Multiple },
        };

        class NodeWithPorts : BaseNodeType
        {
            public InputPort<string> In { get; }
            public InputPort<string> In1 { get; }
            public InputPortMulti<string> InMulti { get; }
            public InputPort<int> InWithName { get; } = "InName";
            public InputPort<float> InFloat { get; }

            public OutputPort<string> Out { get; }
            public OutputPort<string> Out1 { get; }
            public OutputPortSingle<string> OutSingle { get; }
            public OutputPort<int> OutWithName { get; } = "OutName";
            public OutputPort<float> OutFloat { get; }
        }
    }
}