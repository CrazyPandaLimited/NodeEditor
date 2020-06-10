using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public static class GraphExtensions
    {
        public static IEnumerable<PortModel> ConnectedPorts( this PortModel port )
        {
            return port.Connections.Select( c => c.OtherSide( port ) );
        }

        public static PortModel OtherSide( this ConnectionModel connection, PortModel startingPort )
        {
            if( connection.From == startingPort )
                return connection.To;
            else if( connection.To == startingPort )
                return connection.From;
            else
                return null;
        }

        public static PortModel Port( this NodeModel node, string id )
        {
            return node.Ports.FirstOrDefault( p => p.Id == id );
        }

        public static IEnumerable<PortModel> InputPorts( this NodeModel node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Input );
        }

        public static IEnumerable<PortModel> OutputPorts( this NodeModel node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Output );
        }

        public static IEnumerable<ConnectionModel> InputConnections( this NodeModel node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Input ).SelectMany( p => p.Connections );
        }

        public static IEnumerable<ConnectionModel> OutputConnections( this NodeModel node )
        {
            return node.Ports.Where( p => p.Direction == PortDirection.Output ).SelectMany( p => p.Connections );
        }

        public static ConnectionModel InputConnection( this NodeModel node, string inputPortId )
        {
            return node.InputPorts().FirstOrDefault( p => p.Id == inputPortId )?.Connections?.SingleOrDefault();
        }

        public static NodeModel CreateNode( this INodeType nodeType )
        {
            var ret = new NodeModel( nodeType );
            nodeType.PostLoad( ret );
            return ret;
        }
    }
}