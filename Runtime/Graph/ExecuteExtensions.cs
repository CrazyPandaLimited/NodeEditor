using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public static class ExecuteExtensions
    {
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
            var nodes = graph.Nodes;

            // nothing to do if we have no nodes
            if( nodes.Count == 0 )
                return ctx;

            var nodesToCheck = new Queue< NodeModel >( nodes.Where( n => !n.InputPorts().Any() ) );
            if( nodesToCheck.Count == 0 )
            {
                Debug.LogError( "Cannot walk graph! All of its nodes have inputs" );
                return ctx;
            }

            var fulfilledConnections = new HashSet< ConnectionModel >();

            while( nodesToCheck.Count > 0 )
            {
                var node = nodesToCheck.Dequeue();

                ctx.Node = node;

                try
                {
                    nodeAction.Invoke( ctx );
                    ctx.ValidateOutputs();
                }
                catch( Exception e )
                {
                    ctx.AddException( e );
                }

                var outputConnections = node.OutputConnections();

                // mark connected nodes' ports as fulfilled
                foreach( var connection in outputConnections )
                {
                    ctx.Connection = connection;

                    try
                    {
                        connectionAction?.Invoke( ctx );
                    }
                    catch( Exception e )
                    {
                        ctx.AddException( e );
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
            var nodes = graph.Nodes;

            // nothing to do if we have no nodes
            if( nodes.Count == 0 )
                return ctx;

            var nodesToCheck = new Queue< NodeModel >( nodes.Where( n => !n.InputPorts().Any() ) );
            if( nodesToCheck.Count == 0 )
            {
                Debug.LogError( "Cannot walk graph! All of its nodes have inputs" );
                return ctx;
            }

            var fulfilledConnections = new HashSet< ConnectionModel >();

            while( nodesToCheck.Count > 0 )
            {
                var node = nodesToCheck.Dequeue();

                ctx.Node = node;

                try
                {
                    await nodeAction.Invoke( ctx );
                    ctx.ValidateOutputs();
                }
                catch( Exception e )
                {
                    ctx.AddException( e );
                }

                var outputConnections = node.OutputConnections();

                // mark connected nodes' ports as fulfilled
                foreach( var connection in outputConnections )
                {
                    if( connectionAction != null )
                    {
                        ctx.Connection = connection;
                        try
                        {
                            await connectionAction.Invoke( ctx );
                        }
                        catch( Exception e )
                        {
                            ctx.AddException( e );
                        }
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
    }
}
