using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public abstract class BaseNode< TNode, TPort > : INode where TNode : BaseNode< TNode, TPort > where TPort : IPort
    {
        [ JsonIgnore ]
        protected readonly List< TPort > _ports = new List< TPort >();
        [ JsonIgnore ]
        protected Action<NodePortsChangedArgs> _portsChanged;

        public abstract string Id { get; set; }

        [ JsonIgnore ]
        public abstract PropertyBlock PropertyBlock { get; set; }

        /// <summary>
        /// Collection of ports
        /// </summary>
        [ JsonIgnore ]
        public IReadOnlyList< TPort > Ports => _ports;

        /// <summary>
        /// Fired when <see cref="Ports"/> collection changes
        /// </summary>
        public event Action<NodePortsChangedArgs> PortsChanged
        {
            add => _portsChanged += value;
            remove => _portsChanged -= value;
        }

        IEnumerable< IPort > INode.Ports => _ports.Cast< IPort >();

        public abstract void AddPort( IPort port );

        public abstract void RemovePort( IPort port );
        
        /// <summary>
        /// Adds new port to a node
        /// </summary>
        /// <param name="port">New port</param>
        /// <exception cref="ArgumentNullException">When <paramref name="port"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="port"/> is already added to a node</exception>
        public void AddPort( TPort port )
        {
            if( port == null )
                throw new ArgumentNullException( nameof(port) );
            
            if( NeedToAddPort( port ) )
            {
                _ports.Add( port );
                _portsChanged?.Invoke( new NodePortsChangedArgs( ( TNode )this, true, port ) );
            }
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

            var portIdx = _ports.FindIndex( p => DoesPortExists( p, portId ) );

            if( portIdx == -1 )
                return;

            var port = _ports[ portIdx ];

            _ports.RemoveAt( portIdx );

            _portsChanged?.Invoke( new NodePortsChangedArgs( ( TNode )this, false, port ) );
        }
        
        /// <summary>
        /// Removes port from a node
        /// </summary>
        /// <param name="port">Port to remove</param>
        /// <exception cref="ArgumentNullException">When <paramref name="port"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="port"/> is not found in this node</exception>
        public void RemovePort( TPort port )
        {
            if( port == null )
                throw new ArgumentNullException( nameof(port) );

            if( !_ports.Remove( port ) )
                throw new ArgumentException( $"Port {port} not found in node {this}", nameof(port) );

            _portsChanged?.Invoke( new NodePortsChangedArgs( (TNode) this, false, port ) );
        }

        protected abstract bool NeedToAddPort( TPort port );
        protected abstract bool DoesPortExists( TPort port, string portId );

        /// <summary>
        /// Args of <see cref="PortsChanged"/> event
        /// </summary>
        public struct NodePortsChangedArgs
        {
            /// <summary>
            /// Node that was changed
            /// </summary>
            public TNode Node { get; }

            /// <summary>
            /// Whether the port was added or removed
            /// </summary>
            public bool IsAdded { get; }

            /// <summary>
            /// Port that was added or removed
            /// </summary>
            public TPort Port { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="node">Node that was changed</param>
            /// <param name="isAdded">Whether the port was added or removed</param>
            /// <param name="port">Port that was added or removed</param>
            public NodePortsChangedArgs( TNode node, bool isAdded, TPort port )
            {
                Node = node;
                IsAdded = isAdded;
                Port = port;
            }
        }
    }
}