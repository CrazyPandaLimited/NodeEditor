using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Direction of a port
    /// </summary>
    public enum PortDirection
    {
        /// <summary>
        /// Input of a node
        /// </summary>
        Input,

        /// <summary>
        /// Output of a node
        /// </summary>
        Output
    }

    /// <summary>
    /// How many connections a port can support
    /// </summary>
    public enum PortCapacity
    {
        /// <summary>
        /// Use default capacity for given port direction.
        /// Input ports use <see cref="Single"/> and Output ports use <see cref="Multiple"/>
        /// </summary>
        NotSet,
        
        /// <summary>
        /// Only one connection may be connected
        /// </summary>
        Single,
        
        /// <summary>
        /// Multiple connections may be connected
        /// </summary>
        Multiple
    }

    /// <summary>
    /// Port in a node that may be connected to other ports
    /// </summary>
    public sealed class PortModel
    {
        private NodeModel _node;

        /// <summary>
        /// Unique id of a port within a node
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Type of value that this port have
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Direction of port
        /// </summary>
        public PortDirection Direction { get; }

        /// <summary>
        /// Capacity of a port
        /// </summary>
        public PortCapacity Capacity { get; }

        /// <summary>
        /// Owning node
        /// </summary>
        public NodeModel Node
        {
            get => _node;
            set => this.SetOnceOrNull( ref _node, value );
        }

        /// <summary>
        /// Collection of <see cref="ConnectionModel"/> connected to this port
        /// </summary>
        public IList<ConnectionModel> Connections { get; } = new List<ConnectionModel>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Unique id of a port within a node</param>
        /// <param name="type">Type of value that this port have</param>
        /// <param name="direction">Direction of port</param>
        /// <param name="capacity">Capacity of a port</param>
        /// <exception cref="ArgumentNullException">When <paramref name="id"/> is null</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="type"/> is null</exception>
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

        /// <summary>
        /// Creates a port with given type
        /// </summary>
        /// <typeparam name="T">Type of value that this port have</typeparam>
        /// <param name="id">Unique id of a port within a node</param>
        /// <param name="direction">Direction of port</param>
        /// <param name="capacity">Capacity of a port</param>
        /// <exception cref="ArgumentNullException">When <paramref name="id"/> is null</exception>
        public static PortModel Create<T>( string id, PortDirection direction, PortCapacity capacity = PortCapacity.NotSet )
        {
            return new PortModel( id, typeof( T ), direction, capacity );
        }

        public override string ToString() => $"Port: {Id} ({Type.Name})";
    }
}