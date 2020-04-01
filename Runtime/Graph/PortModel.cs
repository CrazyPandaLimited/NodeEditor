using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public enum PortDirection
    {
        Input, Output
    }

    public enum PortCapacity
    {
        NotSet, Single, Multiple
    }

    public sealed class PortModel
    {
        private NodeModel _node;

        public string Id { get; }

        public Type Type { get; }

        public PortDirection Direction { get; }

        public PortCapacity Capacity { get; }

        public NodeModel Node
        {
            get => _node;
            set => this.SetOnceOrNull( ref _node, value );
        }

        public IList<ConnectionModel> Connections { get; } = new List<ConnectionModel>();

        public PortModel( string id, Type type, PortDirection direction, PortCapacity capacity = PortCapacity.NotSet )
        {
            if( capacity == PortCapacity.NotSet )
            {
                // by default input ports are single, output ports are multiple
                capacity = direction == PortDirection.Input ? PortCapacity.Single : PortCapacity.Multiple;
            }

            Id = id ?? throw new ArgumentNullException( nameof( id ) );
            Type = type ?? throw new ArgumentNullException( nameof( type ) );
            Direction = direction;
            Capacity = capacity;
        }

        public static PortModel Create<T>( string id, PortDirection direction, PortCapacity capacity = PortCapacity.NotSet )
        {
            return new PortModel( id, typeof( T ), direction, capacity );
        }

        public override string ToString() => $"Port: {Id} ({Type.Name})";
    }
}