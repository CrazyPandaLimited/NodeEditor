using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public delegate void GraphChangedCalback( IReadOnlyList<NodeModel> addedNodes, IReadOnlyList<NodeModel> removedNodes, IReadOnlyList<ConnectionModel> addedConnections, IReadOnlyList<ConnectionModel> removedConnections );

    public sealed class GraphModel
    {
        private static readonly IReadOnlyList<NodeModel> _emptyNodes = new NodeModel[ 0 ];
        private static readonly IReadOnlyList<ConnectionModel> _emptyConnections = new ConnectionModel[ 0 ];

        private List<NodeModel> _nodes = new List<NodeModel>();
        private List<ConnectionModel> _connections = new List<ConnectionModel>();
        private ChangeSet _changeSet;

        public IGraphType Type { get; }

        public IReadOnlyList<NodeModel> Nodes => _nodes;
        public IReadOnlyList<ConnectionModel> Connections => _connections;

        public event GraphChangedCalback GraphChanged;

        public GraphModel( IGraphType type )
        {
            Type = type ?? throw new ArgumentNullException( nameof( type ) );
        }

        public void AddNode( NodeModel node )
        {
            if( node == null )
                throw new ArgumentNullException( nameof( node ) );

            if( _nodes.Contains( node ) )
                throw new ArgumentException( $"Node '{node}' already added to graph" );

            DoAddNode( node );
        }

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

        public IDisposable BeginChangeSet()
        {
            if( _changeSet != null )
                throw new InvalidOperationException( "Previous ChangeSet not finished!" );

            _changeSet = new ChangeSet( this );
            return _changeSet;
        }

        public IGraphExecutionResult Execute( Action<INodeExecutionContext> nodeAction, Action<IConnectionExecutionContext> connectionAction = null )
        {
            if( nodeAction == null )
                throw new ArgumentNullException( nameof( nodeAction ) );

            var ctx = new GraphExecutionContext();

            // nothing to do if we have no nodes
            if( _nodes.Count == 0 )
                return ctx;

            var nodesToCheck = new Queue<NodeModel>( _nodes.Where( n => !n.InputPorts().Any() ) );
            if( nodesToCheck.Count == 0 )
            {
                Debug.LogError( "Cannot walk graph! All of its nodes have inputs" );
                return ctx;
            }

            var fulfilledConnections = new HashSet<ConnectionModel>();

            while( nodesToCheck.Count > 0 )
            {
                var node = nodesToCheck.Dequeue();

                ctx.Node = node;
                nodeAction.Invoke( ctx );
                ctx.ValidateOutputs();

                var outputConnections = node.OutputConnections();

                // mark connected nodes' ports as fulfilled
                foreach( var connection in outputConnections )
                {
                    ctx.Connection = connection;
                    connectionAction?.Invoke( ctx );

                    fulfilledConnections.Add( connection );
                }

                // find new nodes, that can be added to queue
                foreach( var connection in outputConnections )
                {
                    var connectedNode = connection.To.Node;

                    var inputConnections = connectedNode.InputConnections();

                    // check if all input ports are connected and all input connections are fulfilled
                    if( connectedNode.InputPorts().All( p => p.Connections.Count > 0 ) &&
                        inputConnections.All( c => fulfilledConnections.Contains( c ) ) )
                    {
                        nodesToCheck.Enqueue( connectedNode );

                        foreach( var inConnection in inputConnections )
                        {
                            fulfilledConnections.Remove( inConnection );
                        }
                    }
                }
            }

            if( fulfilledConnections.Count > 0 )
            {
                Debug.LogError( "Some nodes have unfulfilled connections!" );
            }

            ctx.Node = null;
            ctx.Connection = null;
            return ctx;
        }

        public Task<IGraphExecutionResult> ExecuteAsync( Func<INodeExecutionContext, Task> nodeAction, Func<IConnectionExecutionContext, Task> connectionAction = null )
        {
            if( nodeAction == null )
                throw new ArgumentNullException( nameof( nodeAction ) );

            return ExecuteAsyncInner( nodeAction, connectionAction );
        }

        private async Task<IGraphExecutionResult> ExecuteAsyncInner( Func<INodeExecutionContext, Task> nodeAction, Func<IConnectionExecutionContext, Task> connectionAction )
        {
            var ctx = new GraphExecutionContext();

            // nothing to do if we have no nodes
            if( _nodes.Count == 0 )
                return ctx;

            var nodesToCheck = new Queue<NodeModel>( _nodes.Where( n => !n.InputPorts().Any() ) );
            if( nodesToCheck.Count == 0 )
            {
                Debug.LogError( "Cannot walk graph! All of its nodes have inputs" );
                return ctx;
            }

            var fulfilledConnections = new HashSet<ConnectionModel>();

            while( nodesToCheck.Count > 0 )
            {
                var node = nodesToCheck.Dequeue();

                ctx.Node = node;
                await nodeAction.Invoke( ctx );
                ctx.ValidateOutputs();

                var outputConnections = node.OutputConnections();

                // mark connected nodes' ports as fulfilled
                foreach( var connection in outputConnections )
                {
                    if( connectionAction != null )
                    {
                        ctx.Connection = connection;
                        await connectionAction.Invoke( ctx );
                    }

                    fulfilledConnections.Add( connection );
                }

                // find new nodes, that can be added to queue
                foreach( var connection in outputConnections )
                {
                    var connectedNode = connection.To.Node;

                    var inputConnections = connectedNode.InputConnections();

                    // check if all input ports are connected and all input connections are fulfilled
                    if( connectedNode.InputPorts().All( p => p.Connections.Count > 0 ) &&
                        inputConnections.All( c => fulfilledConnections.Contains( c ) ) )
                    {
                        nodesToCheck.Enqueue( connectedNode );

                        foreach( var inConnection in inputConnections )
                        {
                            fulfilledConnections.Remove( inConnection );
                        }
                    }
                }
            }

            if( fulfilledConnections.Count > 0 )
            {
                Debug.LogError( "Some nodes have unfulfilled connections!" );
            }

            ctx.Node = null;
            ctx.Connection = null;
            return ctx;
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