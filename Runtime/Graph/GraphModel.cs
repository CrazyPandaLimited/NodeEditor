using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Calback that is fired when changes happen in a graph
    /// </summary>
    /// <param name="addedNodes">Collection of nodes that were added to graph</param>
    /// <param name="removedNodes">Collection of nodes that were removed from graph</param>
    /// <param name="addedConnections">Collection of connections that were added to graph</param>
    /// <param name="removedConnections">Collection of connections that were removed from graph</param>
    public delegate void GraphChangedCalback( IReadOnlyList<NodeModel> addedNodes, IReadOnlyList<NodeModel> removedNodes, IReadOnlyList<ConnectionModel> addedConnections, IReadOnlyList<ConnectionModel> removedConnections );

    /// <summary>
    /// Model of a graph. Contains nodes and connections between them
    /// </summary>
    public sealed class GraphModel
    {
        private static readonly IReadOnlyList<NodeModel> _emptyNodes = new NodeModel[ 0 ];
        private static readonly IReadOnlyList<ConnectionModel> _emptyConnections = new ConnectionModel[ 0 ];

        private List<NodeModel> _nodes = new List<NodeModel>();
        private List<ConnectionModel> _connections = new List<ConnectionModel>();
        private ChangeSet _changeSet;

        /// <summary>
        /// Type of graph
        /// </summary>
        public IGraphType Type { get; }

        /// <summary>
        /// Collection of nodes
        /// </summary>
        public IReadOnlyList<NodeModel> Nodes => _nodes;

        /// <summary>
        /// Collection of connections
        /// </summary>
        public IReadOnlyList<ConnectionModel> Connections => _connections;

        /// <summary>
        /// Event that is fired when changes happen in a graph
        /// </summary>
        public event GraphChangedCalback GraphChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of graph</param>
        public GraphModel( IGraphType type )
        {
            Type = type ?? throw new ArgumentNullException( nameof( type ) );
        }

        /// <summary>
        /// Adds a node to a graph
        /// </summary>
        /// <param name="node">Node to add</param>
        /// <exception cref="ArgumentNullException">When <paramref name="node"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="node"/> is already added to a graph</exception>
        public void AddNode( NodeModel node )
        {
            if( node == null )
                throw new ArgumentNullException( nameof( node ) );

            if( node.Graph != null || _nodes.Contains( node ) )
                throw new ArgumentException( $"Node '{node}' already added to graph" );

            DoAddNode( node );
        }

        /// <summary>
        /// Removes a node from a graph. Also removes all of its connections
        /// </summary>
        /// <param name="node">Node to remove</param>
        /// <exception cref="ArgumentNullException">When <paramref name="node"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="node"/> is not found in this graph</exception>
        public void RemoveNode( NodeModel node )
        {
            if( node == null )
                throw new ArgumentNullException( nameof( node ) );

            // this operation may remove some connections. start a batch if not already
            ChangeSet scopedChangeSet = null;
            if( _changeSet == null )
                _changeSet = scopedChangeSet = new ChangeSet( this );

            try
            {
                if( !DoRemoveNode( node ) )
                    throw new ArgumentException( $"Node '{node}' not found in graph" );

                foreach( var connection in node.InputConnections().ToArray() )
                    Disconnect( connection );

                foreach( var connection in node.OutputConnections().ToArray() )
                    Disconnect( connection );
            }
            finally
            {
                if( scopedChangeSet != null )
                {
                    scopedChangeSet?.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks whether two given ports can be connected
        /// </summary>
        /// <param name="from">Port where connection starts</param>
        /// <param name="to">Port where connection ends/param>
        /// <returns>True if these ports can be connected</returns>
        /// <exception cref="ArgumentNullException">When any of ports is null</exception>
        public bool CanConnect( PortModel from, PortModel to )
        {
            if( from == null )
                throw new ArgumentNullException( nameof( from ) );

            if( to == null )
                throw new ArgumentNullException( nameof( to ) );

            // check direction
            if( from.Direction == PortDirection.Input || to.Direction == PortDirection.Output )
                return false;

            // check same node
            if( from.Node == to.Node )
                return false;

            // check if ports are in this graph
            if( from.Node?.Graph != this || to.Node?.Graph != this )
                return false;

            // check types
            if( Type.FindConnectionType( from.Type, to.Type ) == null )
                return false;

            // check same connection exists
            if( from.Connections.Any( c => c.To == to ) )
                return false;

            return true;
        }

        /// <summary>
        /// Connects two given ports
        /// </summary>
        /// <param name="from">Port where connection starts</param>
        /// <param name="to">Port where connection ends/param>
        /// <returns>Connection between given ports</returns>
        /// <exception cref="ArgumentNullException">When any of ports is null</exception>
        /// <exception cref="InvalidOperationException">When these ports cannot be connected</exception>
        public ConnectionModel Connect( PortModel from, PortModel to )
        {
            if( from == null )
                throw new ArgumentNullException( nameof( from ) );

            if( to == null )
                throw new ArgumentNullException( nameof( to ) );

            if( !CanConnect( from, to ) )
                throw new InvalidOperationException( $"Cannot connect '{from}' to '{to}'" );

            // this operation may remove some connections. start a batch if not already
            ChangeSet scopedChangeSet = null;
            if( _changeSet == null )
                _changeSet = scopedChangeSet = new ChangeSet( this );

            try
            {
                var ret = _changeSet.RemovedConnections?.FirstOrDefault( c => c.From == from && c.To == to );

                if( ret == null )
                {
                    ret = new ConnectionModel( Type.FindConnectionType( from.Type, to.Type ) )
                    {
                        From = from,
                        To = to,
                    };

                    ret.Type.PostLoad( ret );
                }

                // check from port capacity
                if( from.Capacity == PortCapacity.Single )
                {
                    foreach( var c in from.Connections.ToArray() )
                        Disconnect( c as ConnectionModel );
                }

                if( to.Capacity == PortCapacity.Single )
                {
                    foreach( var c in to.Connections.ToArray() )
                        Disconnect( c as ConnectionModel );
                }

                from.Connections.Add( ret );
                to.Connections.Add( ret );

                DoAddConnection( ret );

                return ret;
            }
            finally
            {
                if( scopedChangeSet != null )
                {
                    scopedChangeSet?.Dispose();
                }
            }
        }

        /// <summary>
        /// Removes connection from a graph
        /// </summary>
        /// <param name="connection">Connection to remove</param>
        /// <exception cref="ArgumentNullException">When <paramref name="connection"/> is null</exception>
        /// <exception cref="InvalidOperationException">When <paramref name="connection"/> not found in this graph</exception>
        public void Disconnect( ConnectionModel connection )
        {
            if( connection == null )
                throw new ArgumentNullException( nameof( connection ) );

            if( !DoRemoveConnection( connection ) )
                throw new InvalidOperationException( $"Connection '{connection}' not found in graph" );

            connection.From.Connections.Remove( connection );
            connection.To.Connections.Remove( connection );

            if( _changeSet == null )
            {
                // Connection data must be cleared after callback is fired, or receiver will not know which connection is it
                // ChangeSet will handle this later
                connection.From = null;
                connection.To = null;
            }
        }

        /// <summary>
        /// Starts batch change operation.
        /// All changes made to a graph before returned <see cref="IDisposable"/> is disposed will be batched to single call of <see cref="GraphChanged"/>
        /// </summary>
        /// <returns>Objec to be disposed when batch is completed</returns>
        public IDisposable BeginChangeSet()
        {
            if( _changeSet != null )
                throw new InvalidOperationException( "Previous ChangeSet not finished!" );

            _changeSet = new ChangeSet( this );
            return _changeSet;
        }

        private void DoAddNode( NodeModel node )
        {
            _nodes.Add( node );
            node.Graph = this;

            if( _changeSet != null )
                _changeSet.Add( node );
            else
                InvokeChanged( addedNodes: new[] { node } );
        }

        private bool DoRemoveNode( NodeModel node )
        {
            var ret = _nodes.Remove( node );

            if( ret )
            {
                node.Graph = null;

                if( _changeSet != null )
                    _changeSet.Remove( node );
                else
                    InvokeChanged( removedNodes: new[] { node } );
            }

            return ret;
        }

        private void DoAddConnection( ConnectionModel connection )
        {
            _connections.Add( connection );

            if( _changeSet != null )
                _changeSet.Add( connection );
            else
                InvokeChanged( addedConnections: new[] { connection } );
        }

        private bool DoRemoveConnection( ConnectionModel connection )
        {
            var ret = _connections.Remove( connection );

            if( ret )
            {
                if( _changeSet != null )
                    _changeSet.Remove( connection );
                else
                    InvokeChanged( removedConnections: new[] { connection } );
            }

            return ret;
        }

        private void InvokeChanged( IReadOnlyList<NodeModel> addedNodes = null, IReadOnlyList<NodeModel> removedNodes = null, IReadOnlyList<ConnectionModel> addedConnections = null, IReadOnlyList<ConnectionModel> removedConnections = null )
        {
            GraphChanged?.Invoke( addedNodes ?? _emptyNodes, removedNodes ?? _emptyNodes, addedConnections ?? _emptyConnections, removedConnections ?? _emptyConnections );
        }

        private class ChangeSet : IDisposable
        {
            private GraphModel _graph;

            // DO NOT modify these lists directly. Use Add and Remove methods
            public List<NodeModel> AddedNodes;
            public List<NodeModel> RemovedNodes;
            public List<ConnectionModel> AddedConnections;
            public List<ConnectionModel> RemovedConnections;

            public ChangeSet( GraphModel graph )
            {
                _graph = graph;
            }

            public void Add( NodeModel node ) => AddRemoveCommon( node, ref AddedNodes, RemovedNodes );
            public void Add( ConnectionModel connection ) => AddRemoveCommon( connection, ref AddedConnections, RemovedConnections );

            public void Remove( NodeModel node ) => AddRemoveCommon( node, ref RemovedNodes, AddedNodes );
            public void Remove( ConnectionModel connection ) => AddRemoveCommon( connection, ref RemovedConnections, AddedConnections );

            public void Dispose()
            {
                if( _graph != null )
                {
                    _graph.InvokeChanged( AddedNodes, RemovedNodes, AddedConnections, RemovedConnections );

                    if( RemovedConnections != null )
                    {
                        // now, after callback was fired, we may clear Connection data
                        for( int i = 0, k = RemovedConnections.Count; i < k; ++i )
                        {
                            var c = RemovedConnections[ i ];
                            c.From = null;
                            c.To = null;
                        }
                    }

                    _graph._changeSet = null;
                    _graph = null;
                }
            }

            private void AddRemoveCommon<T>( T value, ref List<T> toAdd, List<T> toRemove )
            {
                if( toAdd == null )
                    toAdd = new List<T>();

                if( !toAdd.Contains( value ) )
                {
                    toAdd.Add( value );
                    toRemove?.Remove( value );
                }
            }
        }
    }
}