using System;

namespace CrazyPanda.UnityCore.NodeEditor
{
    [ Serializable ]
    public class SConnection : IConnection, IEquatable< SConnection >
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

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ( FromNodeId != null ? FromNodeId.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( FromPortId != null ? FromPortId.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( ToNodeId != null ? ToNodeId.GetHashCode() : 0 );
                hashCode = ( hashCode * 397 ) ^ ( ToPortId != null ? ToPortId.GetHashCode() : 0 );
                return hashCode;
            }
        }

        public bool Equals( SConnection other )
        {
            return other!=null && FromNodeId == other.FromNodeId && FromPortId == other.FromPortId && ToNodeId == other.ToNodeId && ToPortId == other.ToPortId;
        }

        public bool Equals( ConnectionModel connectionModel )
        {
            return connectionModel != null && FromNodeId == connectionModel.From.Node.Id && FromPortId == connectionModel.From.Id && ToNodeId == connectionModel.To.Node.Id && ToPortId == connectionModel.To.Id;
        }
    }
}