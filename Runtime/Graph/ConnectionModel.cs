using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public sealed class ConnectionModel
    {
        private PortModel _from;
        private PortModel _to;

        public IConnectionType Type { get; }

        public PortModel From
        {
            get => _from;
            set => this.SetOnceOrNull( ref _from, value );
        }

        public PortModel To
        {
            get => _to;
            set => this.SetOnceOrNull( ref _to, value );
        }

        public ConnectionModel( IConnectionType type )
        {
            Type = type;
            Type.InitModel( this );
        }

        public override string ToString()
        {
            if( From == null || To == null )
                return "Unconnected";

            return $"{From.Node.Type.Name} {From.Id} -> {To.Node.Type.Name} {To.Id}";
        }
    }
}