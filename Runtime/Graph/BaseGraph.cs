using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public abstract class BaseGraph< TNode, TConnection, TPort, TGraphSettings > : IGraph where TNode : INode where TPort : IPort where TConnection : IConnection where TGraphSettings: IGraphSettings, new()
    {
        /// <summary>
        /// Calback that is fired when changes happen in a graph
        /// </summary>
        /// <param name="addedNodes">Collection of nodes that were added to graph</param>
        /// <param name="removedNodes">Collection of nodes that were removed from graph</param>
        /// <param name="addedConnections">Collection of connections that were added to graph</param>
        /// <param name="removedConnections">Collection of connections that were removed from graph</param>
        public delegate void GraphChangedCalback ( IReadOnlyList<TNode> addedNodes, IReadOnlyList<TNode> removedNodes, IReadOnlyList<TConnection> addedConnections, IReadOnlyList<TConnection> removedConnections );
        
        private static readonly IReadOnlyList< TNode > _emptyNodes = new TNode[ 0 ];
        private static readonly IReadOnlyList< TConnection > _emptyConnections = new TConnection[ 0 ];

        [ JsonIgnore ]
        protected ChangeSet _changeSet { get; private set; }

        [ JsonProperty( nameof(Nodes) ) ]
        protected List< TNode > _nodes = new List< TNode >();
        [ JsonProperty( nameof(Connections) ) ]
        protected List< TConnection > _connections = new List< TConnection >();

        [JsonProperty( nameof( GraphSettings ) )]
        protected IGraphSettings _graphSettings = new TGraphSettings();
        

        /// <summary>
        /// Collection of nodes
        /// </summary>
        [ JsonIgnore ]
        public IReadOnlyList< TNode > Nodes => _nodes;
        /// <summary>
        /// Collection of connections
        /// </summary>
        [ JsonIgnore ]
        public IReadOnlyList< TConnection > Connections => _connections;

        [JsonIgnore]
        public IGraphSettings GraphSettings => _graphSettings;

        /// <summary>
        /// Event that is fired when changes happen in a graph
        /// </summary>
        public event GraphChangedCalback GraphChanged;

        IEnumerable< INode > IGraph.Nodes => _nodes.Cast< INode >();

        void IGraph.AddNode( INode node ) => AddNode( ( TNode )node );

        void IGraph.RemoveNode( INode node ) => RemoveNode( ( TNode )node );

        bool IGraph.CanConnect( IPort from, IPort to ) => CanConnect( ( TPort ) from, ( TPort )to );
        
        IConnection IGraph.Connect( IPort from, IPort to ) => Connect( ( TPort )from, ( TPort )to );

        void IGraph.Disconnect( IConnection connection ) => Disconnect( ( TConnection ) connection );
        
        /// <summary>
        /// Checks whether two given ports can be connected
        /// </summary>
        /// <param name="from">Port where connection starts</param>
        /// <param name="to">Port where connection ends/param>
        /// <returns>True if these ports can be connected</returns>
        /// <exception cref="ArgumentNullException">When any of ports is null</exception>
        public abstract bool CanConnect( TPort from, TPort to );
        
        /// <summary>
        /// Adds a node to a graph
        /// </summary>
        /// <param name="node">Node to add</param>
        /// <exception cref="ArgumentNullException">When <paramref name="node"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="node"/> is already added to a graph</exception>
        public virtual void AddNode( TNode node )
        {
            if( node == null )
                throw new ArgumentNullException( nameof( node ) );

            if( _nodes.Contains( node ) )
                throw new ArgumentException( $"Node '{node}' already added to graph" );

            DoAddNode( node );
        }

        /// <summary>
        /// Removes a node from a graph. Also removes all of its connections
        /// </summary>
        /// <param name="node">Node to remove</param>
        /// <exception cref="ArgumentNullException">When <paramref name="node"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="node"/> is not found in this graph</exception>
        public virtual void RemoveNode( TNode node )
        {
            if( node == null )
                throw new ArgumentNullException( nameof( node ) );

            // this operation may remove some connections. start a batch if not already
            ChangeSet scopedChangeSet = null;
            if( _changeSet == null )
                _changeSet = scopedChangeSet = CreateChangeSet();

            try
            {
                if( !DoRemoveNode( node ) )
                    throw new ArgumentException( $"Node '{node}' not found in graph" );

                foreach( var port in node.Ports )
                {
                    foreach( var connection in port.Connections.ToArray() )
                    {
                        (this as IGraph).Disconnect( connection );
                    }
                }
            }
            finally
            {
                scopedChangeSet?.Dispose();
            }
        }

        /// <summary>
        /// Connects two given ports
        /// </summary>
        /// <param name="from">Port where connection starts</param>
        /// <param name="to">Port where connection ends/param>
        /// <returns>Connection between given ports</returns>
        /// <exception cref="ArgumentNullException">When any of ports is null</exception>
        /// <exception cref="InvalidOperationException">When these ports cannot be connected</exception>
        public TConnection Connect( TPort from, TPort to )
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
                return CreateConnection(from, to);
            }
            finally
            {
                scopedChangeSet?.Dispose();
            }
        }

        /// <summary>
        /// Removes connection from a graph
        /// </summary>
        /// <param name="connection">Connection to remove</param>
        /// <exception cref="ArgumentNullException">When <paramref name="connection"/> is null</exception>
        /// <exception cref="InvalidOperationException">When <paramref name="connection"/> not found in this graph</exception>
        public virtual void Disconnect( TConnection connection )
        {
            if( connection == null )
                throw new ArgumentNullException( nameof( connection ) );

            if( !DoRemoveConnection( connection ) )
                throw new InvalidOperationException( $"Connection '{connection}' not found in graph" );
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

        protected abstract TConnection CreateConnection( TPort from, TPort to );

        protected virtual ChangeSet CreateChangeSet()
        {
            return new ChangeSet( this );
        }

        protected virtual void OnNodeAdded( TNode node )
        {
        }

        protected virtual void OnNodeRemoved( TNode node )
        {
        }
        
        protected void DoAddConnection( TConnection connection )
        {
            _connections.Add( connection );

            if( _changeSet != null )
                _changeSet.Add( connection );
            else
                InvokeChanged( addedConnections: new[] { connection } );
        }

        protected void InvokeChanged( IReadOnlyList< TNode > addedNodes = null, IReadOnlyList< TNode > removedNodes = null, IReadOnlyList< TConnection > addedConnections = null, IReadOnlyList< TConnection > removedConnections = null )
        {
            GraphChanged?.Invoke( addedNodes ?? _emptyNodes, removedNodes ?? _emptyNodes, addedConnections ?? _emptyConnections, removedConnections ?? _emptyConnections );
        }
        
        protected void RemoveConnectionsFromPort( TPort port )
        {
            // check from port capacity
            if( port.Capacity == PortCapacity.Single )
            {
                foreach( var connection in port.Connections.ToArray() )
                {
                    (this as IGraph).Disconnect( connection );
                }
            }
        }
        
        private void DoAddNode( TNode node )
        {
            _nodes.Add( node );

            OnNodeAdded( node );
            
            if( _changeSet != null )
                _changeSet.Add( node );
            else
                InvokeChanged( addedNodes: new[] { node } );
        }

        private bool DoRemoveNode( TNode node )
        {
            var ret = _nodes.Remove( node );

            if( ret )
            {
                OnNodeRemoved( node );
                
                if( _changeSet != null )
                    _changeSet.Remove( node );
                else
                    InvokeChanged( removedNodes: new[] { node } );
            }

            return ret;
        }

        private bool DoRemoveConnection( TConnection connection )
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

        public void SetSettings( IGraphSettings graphSettings )
        {
            _graphSettings = graphSettings;
        }

        protected class ChangeSet : IDisposable
        {
            private BaseGraph< TNode, TConnection, TPort, TGraphSettings > _graph;

            // DO NOT modify these lists directly. Use Add and Remove methods
            public List< TNode > AddedNodes;
            public List< TNode > RemovedNodes;
            public List< TConnection > AddedConnections;
            public List< TConnection > RemovedConnections;

            public ChangeSet( BaseGraph< TNode, TConnection, TPort, TGraphSettings> graph )
            {
                _graph = graph;
            }

            public void Add( TNode node ) => AddRemoveCommon( node, ref AddedNodes, RemovedNodes );
            public void Add( TConnection connection ) => AddRemoveCommon( connection, ref AddedConnections, RemovedConnections );

            public void Remove( TNode node ) => AddRemoveCommon( node, ref RemovedNodes, AddedNodes );
            public void Remove( TConnection connection ) => AddRemoveCommon( connection, ref RemovedConnections, AddedConnections );

            public virtual void Dispose()
            {
                if( _graph != null )
                {
                    _graph.InvokeChanged( AddedNodes, RemovedNodes, AddedConnections, RemovedConnections );

                    _graph._changeSet = null;
                    _graph = null;
                }
            }

            private void AddRemoveCommon< T >( T value, ref List< T > toAdd, List< T > toRemove )
            {
                if( toAdd == null )
                    toAdd = new List< T >();

                if( !toAdd.Contains( value ) )
                {
                    toAdd.Add( value );
                    toRemove?.Remove( value );
                }
            }
        }
    }
}