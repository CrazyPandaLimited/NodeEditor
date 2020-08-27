using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Node in a graph
    /// </summary>
    public class NodeModel
    {
        private string _id;
        private List<PortModel> _ports = new List<PortModel>();
        private PropertyBlock _propertyBlock;
        private GraphModel _graph;

        /// <summary>
        /// Unique id of a node
        /// </summary>
        public string Id
        {
            get => _id;
            set => this.SetOnce( ref _id, value );
        }

        /// <summary>
        /// Type of a node
        /// </summary>
        public INodeType Type { get; }

        /// <summary>
        /// Owning graph or null
        /// </summary>
        public GraphModel Graph
        {
            get => _graph;
            set => this.SetOnceOrNull( ref _graph, value );
        }

        /// <summary>
        /// Collection of ports
        /// </summary>
        public IReadOnlyList<PortModel> Ports
        {
            get => _ports;
        }

        /// <summary>
        /// Position of a node view inside graph
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Custom properties associated with this node. Initialized by <see cref="INodeType"/>
        /// </summary>
        public PropertyBlock PropertyBlock
        {
            get => _propertyBlock;
            set => this.SetOnce( ref _propertyBlock, value );
        }

        /// <summary>
        /// Fired when <see cref="Ports"/> collection changes
        /// </summary>
        public event Action<NodePortsChangedArgs> PortsChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of a node</param>
        public NodeModel( INodeType type )
        {
            Type = type ?? throw new ArgumentNullException( nameof( type ) );
            Type.InitModel( this );
        }

        /// <summary>
        /// Adds new port to a node
        /// </summary>
        /// <param name="port">New port</param>
        /// <exception cref="ArgumentNullException">When <paramref name="port"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="port"/> is already added to a node</exception>
        public void AddPort( PortModel port )
        {
            if( port == null )
                throw new ArgumentNullException( nameof( port ) );

            if( port.Node != null || _ports.Find( p => p.Id == port.Id ) != null )
                throw new ArgumentException( $"Port with id {port.Id} already added to node {port.Node ?? this}", nameof( port ) );

            _ports.Add( port );
            port.Node = this;

            PortsChanged?.Invoke( new NodePortsChangedArgs( this, true, port ) );
        }

        /// <summary>
        /// Removes port from a node
        /// </summary>
        /// <param name="portId">Id of port to remove</param>
        /// <exception cref="ArgumentNullException">When <paramref name="portId"/> is null</exception>
        public void RemovePort( string portId )
        {
            if( portId == null )
                throw new ArgumentNullException( nameof( portId ) );

            var portIdx = _ports.FindIndex( p => p.Id == portId );

            if( portIdx == -1 )
                return;

            var port = _ports[ portIdx ];

            foreach( var c in port.Connections.ToArray() )
            {
                Graph.Disconnect( c );
            }

            _ports.RemoveAt( portIdx );
            port.Node = null;

            PortsChanged?.Invoke( new NodePortsChangedArgs( this, false, port ) );
        }

        /// <summary>
        /// Removes port from a node
        /// </summary>
        /// <param name="port">Port to remove</param>
        /// <exception cref="ArgumentNullException">When <paramref name="port"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="port"/> is not found in this node</exception>
        public void RemovePort( PortModel port )
        {
            if( port == null )
                throw new ArgumentNullException( nameof( port ) );

            if( !_ports.Remove( port ) )
                throw new ArgumentException( $"Port {port} not found in node {this}", nameof( port ) );

            foreach( var c in port.Connections.ToArray() )
            {
                Graph.Disconnect( c );
            }

            port.Node = null;

            PortsChanged?.Invoke( new NodePortsChangedArgs( this, false, port ) );
        }

        public override string ToString() => $"{Type.Name}. Id: {Id}";

        /// <summary>
        /// Args of <see cref="PortsChanged"/> event
        /// </summary>
        public struct NodePortsChangedArgs
        {
            /// <summary>
            /// Node that was changed
            /// </summary>
            public NodeModel Node { get; }

            /// <summary>
            /// Whether the port was added or removed
            /// </summary>
            public bool IsAdded { get; }

            /// <summary>
            /// Port that was added or removed
            /// </summary>
            public PortModel Port { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="node">Node that was changed</param>
            /// <param name="isAdded">Whether the port was added or removed</param>
            /// <param name="port">Port that was added or removed</param>
            public NodePortsChangedArgs( NodeModel node, bool isAdded, PortModel port )
            {
                Node = node;
                IsAdded = isAdded;
                Port = port;
            }
        }
    }
}