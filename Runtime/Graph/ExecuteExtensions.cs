using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public static class ExecuteExtensions
    {
        // cached storage of execution list for graphs. optimization for faster execution of same graph
        private static readonly ConditionalWeakTable< GraphModel, IReadOnlyList< object > > _executionLists = new ConditionalWeakTable< GraphModel, IReadOnlyList< object > >();

        /// <summary>
        /// Walks graph starting from nodes without inputs and then follows connections from them.
        /// Calls <paramref name="nodeAction"/> on every node and <paramref name="connectionAction"/> on every connection.
        /// <paramref name="nodeAction"/> must set values to all output ports of a node.
        /// </summary>
        /// <param name="graph">Graph to execute</param>
        /// <param name="nodeAction">Action to perform on each node</param>
        /// <param name="connectionAction">Action to perform on each connection</param>
        /// <returns>Execution result</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="graph"/> is null</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="nodeAction"/> is null</exception>
        public static IGraphExecutionResult Execute( this GraphModel graph, Action< INodeExecutionContext > nodeAction, Action< IConnectionExecutionContext > connectionAction = null )
        {
            if( graph == null )
                throw new ArgumentNullException( nameof( graph ) );

            if( nodeAction == null )
                throw new ArgumentNullException( nameof( nodeAction ) );

            var ctx = new GraphExecutionContext();
            var executionList = GetExecutionList( graph );

            for( int i = 0, k = executionList.Count; i < k; ++i )
            {
                try
                {
                    switch( executionList[ i ] )
                    {
                        case NodeModel node:
                        {
                            ctx.Node = node;
                            nodeAction.Invoke( ctx );
                            ctx.ValidateOutputs();
                            break;
                        }

                        case ConnectionModel connection:
                        {
                            ctx.Connection = connection;
                            connectionAction?.Invoke( ctx );
                            break;
                        }
                    }
                }
                catch( Exception e )
                {
                    ctx.AddException( e );
                }
            }

            ctx.Node = null;
            ctx.Connection = null;
            return ctx;
        }

        /// <summary>
        /// Walks graph starting from nodes without inputs and then follows connections from them.
        /// Calls <paramref name="nodeAction"/> on every node and <paramref name="connectionAction"/> on every connection.
        /// <paramref name="nodeAction"/> must set values to all output ports of a node.
        /// </summary>
        /// <param name="graph">Graph to execute</param>
        /// <param name="nodeAction">Action to perform on each node</param>
        /// <param name="connectionAction">Action to perform on each connection</param>
        /// <returns>Execution result</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="graph"/> is null</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="nodeAction"/> is null</exception>
        public static Task< IGraphExecutionResult > ExecuteAsync( this GraphModel graph, Func< INodeExecutionContext, Task > nodeAction, Func< IConnectionExecutionContext, Task > connectionAction = null )
        {
            if( graph == null )
                throw new ArgumentNullException( nameof( graph ) );

            if( nodeAction == null )
                throw new ArgumentNullException( nameof( nodeAction ) );

            return ExecuteAsyncInner( graph, nodeAction, connectionAction );
        }

        private static async Task< IGraphExecutionResult > ExecuteAsyncInner( GraphModel graph, Func< INodeExecutionContext, Task > nodeAction, Func< IConnectionExecutionContext, Task > connectionAction )
        {
            var ctx = new GraphExecutionContext();
            var executionList = GetExecutionList( graph );

            for( int i = 0, k = executionList.Count; i < k; ++i )
            {
                try
                {
                    switch( executionList[ i ] )
                    {
                        case NodeModel node:
                        {
                            ctx.Node = node;
                            await nodeAction.Invoke( ctx );
                            ctx.ValidateOutputs();
                            break;
                        }

                        case ConnectionModel connection:
                        {
                            ctx.Connection = connection;
                            await connectionAction?.Invoke( ctx );
                            break;
                        }
                    }
                }
                catch( Exception e )
                {
                    ctx.AddException( e );
                }
            }

            ctx.Node = null;
            ctx.Connection = null;
            return ctx;
        }

        private static IReadOnlyList< object > GetExecutionList( GraphModel graph )
        {
            // check if it is not already in cache
            if( !_executionLists.TryGetValue( graph, out var list ) )
            {
                // subscribe for changes in graph
                graph.GraphChanged +=  OnGraphModelChanged;

                //build new list and store in cache
                list = BuildExecutionList( graph );
                _executionLists.Add( graph, list );
            }

            return list;
        }

        private static IReadOnlyList< object > BuildExecutionList( GraphModel graph )
        {
            var nodes = graph.Nodes;
            if( nodes.Count == 0 )
                return Array.Empty< object >();

            var nodesToCheck = new Queue< NodeModel >( nodes.Where( n => !n.InputPorts().Any() ) );
            if( nodesToCheck.Count == 0 )
            {
                throw new InvalidOperationException( "Cannot walk graph! All of its nodes have inputs" );
            }

            var connections = graph.Connections;
            var ret = new object[ nodes.Count + connections.Count ];
            var fulfilledConnections = new HashSet< ConnectionModel >();

            int indexer = 0;
            while( nodesToCheck.Count > 0 )
            {
                var node = nodesToCheck.Dequeue();
                ret[ indexer++ ] = node;

                var outputConnections = node.OutputConnections();

                // mark connected nodes' ports as fulfilled
                foreach( var connection in outputConnections )
                {
                    ret[ indexer++ ] = connection;
                    fulfilledConnections.Add( connection );
                }

                // find new nodes, that can be added to queue
                foreach( var connection in outputConnections )
                {
                    var connectedNode = connection.To.Node;

                    var inputConnections = connectedNode.InputConnections();

                    // check if all input ports are connected and all input connections are fulfilled
                    if( connectedNode.InputPorts().All( p => p.Connections.Count > 0 || p.Optional ) &&
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
                throw new InvalidOperationException( "Cannot walk graph! Some nodes have unfulfilled connections!" );
            }

            return ret;
        }

        private static void OnGraphModelChanged( IReadOnlyList< NodeModel > addedNodes, IReadOnlyList< NodeModel > removedNodes, IReadOnlyList< ConnectionModel > addedConnections, IReadOnlyList< ConnectionModel > removedConnections )
        {
            // one of this conditions will always return valid Graph
            var graph = addedNodes?.FirstOrDefault()?.Graph ??
                        removedNodes?.FirstOrDefault()?.Graph ??
                        addedConnections?.FirstOrDefault()?.From?.Node?.Graph ??
                        removedConnections?.FirstOrDefault()?.From?.Node?.Graph;

            _executionLists.Remove( graph );
            graph.GraphChanged -= OnGraphModelChanged;
        }
    }
}
