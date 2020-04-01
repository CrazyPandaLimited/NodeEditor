using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class NodeModel
    {
        private string _id;
        private List<PortModel> _ports = new List<PortModel>();
        private PropertyBlock _propertyBlock;
        private GraphModel _graph;

        public string Id
        {
            get => _id;
            set => this.SetOnce( ref _id, value );
        }

        public INodeType Type { get; }

        public GraphModel Graph
        {
            get => _graph;
            set => this.SetOnceOrNull( ref _graph, value );
        }

        public IReadOnlyList<PortModel> Ports
        {
            get => _ports;
        }

        public Vector2 Position { get; set; }

        public PropertyBlock PropertyBlock
        {
            get => _propertyBlock;
            set => this.SetOnce( ref _propertyBlock, value );
        }

        public event Action<NodePortsChangedArgs> PortsChanged;

        public NodeModel( INodeType type )
        {
            Type = type ?? throw new ArgumentNullException( nameof( type ) );
            Type.InitModel( this );
        }

        public void AddPort( PortModel port )
        {
            if( port == null )
                throw new ArgumentNullException( nameof( port ) );

            if( _ports.Find( p => p.Id == port.Id ) != null )
                throw new ArgumentException( $"Port with id {port.Id} already added to node {this}", nameof( port ) );

            _ports.Add( port );
            port.Node = this;

            PortsChanged?.Invoke( new NodePortsChangedArgs( this, true, port ) );
        }

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

        public struct NodePortsChangedArgs
        {
            public NodeModel Node { get; }
            public bool IsAdded { get; }
            public PortModel Port { get; }

            public NodePortsChangedArgs( NodeModel node, bool isAdded, PortModel port )
            {
                Node = node;
                IsAdded = isAdded;
                Port = port;
            }
        }
    }
}