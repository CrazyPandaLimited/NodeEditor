using System;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    class GraphModelExecutionTests
    {
        [Test]
        public void Execute_Should_Succeed_NoNodes()
        {
            var graph = new GraphModel( GraphModelTests.GraphTypeWithConnections );

            var result = graph.Execute( ExecuteNode );

            Assert.That( result, Is.Not.Null );
        }

        [AsyncTest]
        public async Task ExecuteAsync_Should_Succeed_NoNodes()
        {
            var graph = new GraphModel( GraphModelTests.GraphTypeWithConnections );

            var result = await graph.ExecuteAsync( ExecuteNodeAsync );

            Assert.That( result, Is.Not.Null );
        }

        [Test]
        public void Execute_Should_Succeed_SingleNode()
        {
            var graph = new GraphModel( GraphModelTests.GraphTypeWithConnections );
            var nodeType = new SourceNode( "test" );
            var node = nodeType.CreateNode();
            graph.AddNode( node );

            var result = graph.Execute( ExecuteNode );

            Assert.That( result, Is.Not.Null );

            var v1 = GetPortValue<string>( nodeType.Out, result, node );

            Assert.That( v1.HasValue, Is.True );
            Assert.That( v1.Value, Is.EqualTo( "test" ) );
        }

        [Test]
        public void Execute_Should_Succeed_SingleNode_Cached()
        {
            var graph = new GraphModel( GraphModelTests.GraphTypeWithConnections );
            var nodeType = new SourceNode( "test" );
            var node = nodeType.CreateNode();
            graph.AddNode( node );

            var result = graph.Execute( ExecuteNode );
            result = graph.Execute( ExecuteNode ); // this line will take value from cache

            Assert.That( result, Is.Not.Null );

            var v1 = GetPortValue<string>( nodeType.Out, result, node );

            Assert.That( v1.HasValue, Is.True );
            Assert.That( v1.Value, Is.EqualTo( "test" ) );
        }

        [AsyncTest]
        public async Task ExecuteAsync_Should_Succeed_SingleNode()
        {
            var graph = new GraphModel( GraphModelTests.GraphTypeWithConnections );
            var nodeType = new SourceNode( "test" );
            var node = nodeType.CreateNode();
            graph.AddNode( node );

            var result = await graph.ExecuteAsync( ExecuteNodeAsync );

            Assert.That( result, Is.Not.Null );

            var v1 = GetPortValue<string>( nodeType.Out, result, node );

            Assert.That( v1.HasValue, Is.True );
            Assert.That( v1.Value, Is.EqualTo( "test" ) );
        }

        [AsyncTest]
        public async Task ExecuteAsync_Should_Succeed_SingleNode_Cached()
        {
            var graph = new GraphModel( GraphModelTests.GraphTypeWithConnections );
            var nodeType = new SourceNode( "test" );
            var node = nodeType.CreateNode();
            graph.AddNode( node );

            var result = await graph.ExecuteAsync( ExecuteNodeAsync );
            result = await graph.ExecuteAsync( ExecuteNodeAsync ); // this line will take value from cache

            Assert.That( result, Is.Not.Null );

            var v1 = GetPortValue<string>( nodeType.Out, result, node );

            Assert.That( v1.HasValue, Is.True );
            Assert.That( v1.Value, Is.EqualTo( "test" ) );
        }

        [Test]
        public void Execute_Should_Throw_WhenNullCallback()
        {
            var graph = new GraphModel( GraphModelTests.DefaultType );

            Assert.That( () => graph.Execute( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void ExecuteAsync_Should_Throw_WhenNullCallback()
        {
            var graph = new GraphModel( GraphModelTests.DefaultType );

            //graph.ExecuteAsync( null );
            Assert.That( () => graph.ExecuteAsync( null ), Throws.ArgumentNullException );
        }

        [Test]
        public void Execute_Should_Succeed_MultipleNodes()
        {
            // graph sceme:
            //   node1 -------------> node2 -------------> node3
            //          connection1          connection2

            var graph = new GraphModel( GraphModelTests.CreateGraphType( new AppendConnection( "+" ) ) );
            var sourceType = new SourceNode( "test" );
            var transferType = new TransferNode();
            var destType = new DestinationNode();

            var node1 = sourceType.CreateNode();
            graph.AddNode( node1 );

            var node2 = transferType.CreateNode();
            graph.AddNode( node2 );

            var node3 = destType.CreateNode();
            graph.AddNode( node3 );

            var connection1 = graph.Connect( sourceType.Out.FromNode( node1 ), transferType.In.FromNode( node2 ) );
            var connection2 = graph.Connect( transferType.Out.FromNode( node2 ), destType.In.FromNode( node3 ) );

            var result = graph.Execute( ExecuteNode, ExecuteConnection );

            Assert.That( result, Is.Not.Null );

            var pv1 = GetPortValue<string>( sourceType.Out, result, node1 );

            Assert.That( pv1.HasValue, Is.True );
            Assert.That( pv1.Value, Is.EqualTo( "test" ) );

            var pv2 = GetPortValue<string>( transferType.Out, result, node2 );

            Assert.That( pv2.HasValue, Is.True );
            Assert.That( pv2.Value, Is.EqualTo( "test+" ) );

            var cv1 = GetConnectionValue<string>( result, connection1 );

            Assert.That( cv1.HasValue, Is.True );
            Assert.That( cv1.Value, Is.EqualTo( "test+" ) );

            var cv2 = GetConnectionValue<string>( result, connection2 );

            Assert.That( cv2.HasValue, Is.True );
            Assert.That( cv2.Value, Is.EqualTo( "test++" ) );

            Assert.That( destType.OutValue, Is.EqualTo( "test++" ) );
        }

        [AsyncTest]
        public async Task ExecuteAsync_Should_Succeed_MultipleNodes()
        {
            // graph sceme:
            //   node1 -------------> node2 -------------> node3
            //          connection1          connection2

            var graph = new GraphModel( GraphModelTests.CreateGraphType( new AppendConnection( "+" ) ) );
            var sourceType = new SourceNode( "test" );
            var transferType = new TransferNode();
            var destType = new DestinationNode();

            var node1 = sourceType.CreateNode();
            graph.AddNode( node1 );

            var node2 = transferType.CreateNode();
            graph.AddNode( node2 );

            var node3 = destType.CreateNode();
            graph.AddNode( node3 );

            var connection1 = graph.Connect( sourceType.Out.FromNode( node1 ), transferType.In.FromNode( node2 ) );
            var connection2 = graph.Connect( transferType.Out.FromNode( node2 ), destType.In.FromNode( node3 ) );

            var result = await graph.ExecuteAsync( ExecuteNodeAsync, ExecuteConnectionAsync );

            Assert.That( result, Is.Not.Null );

            var pv1 = GetPortValue<string>( sourceType.Out, result, node1 );

            Assert.That( pv1.HasValue, Is.True );
            Assert.That( pv1.Value, Is.EqualTo( "test" ) );

            var pv2 = GetPortValue<string>( transferType.Out, result, node2 );

            Assert.That( pv2.HasValue, Is.True );
            Assert.That( pv2.Value, Is.EqualTo( "test+" ) );

            var cv1 = GetConnectionValue<string>( result, connection1 );

            Assert.That( cv1.HasValue, Is.True );
            Assert.That( cv1.Value, Is.EqualTo( "test+" ) );

            var cv2 = GetConnectionValue<string>( result, connection2 );

            Assert.That( cv2.HasValue, Is.True );
            Assert.That( cv2.Value, Is.EqualTo( "test++" ) );

            Assert.That( destType.OutValue, Is.EqualTo( "test++" ) );
        }

        [ AsyncTest ]
        public async Task ExecuteAsync_Should_Succeed_CatchException_DuringNodeActionExecution()
        {
            var exceptionToCatch = new Exception("some_exception");
            var graph = new GraphModel( GraphModelTests.GraphTypeWithConnections );
            graph.AddNode( new SourceNode( "test" ).CreateNode() );
            
            var result = await graph.ExecuteAsync( _ => throw exceptionToCatch );

            Assert.IsNotEmpty( result.Exceptions );
            Assert.That( result.Exceptions, Has.Member( exceptionToCatch ) );
        }
        
        [ AsyncTest ]
        public async Task ExecuteAsync_Should_Succeed_CatchException_DuringConnectionActionExecution()
        {
            var exceptionToCatch = new Exception("some_exception");
            
            var graph = new GraphModel( GraphModelTests.CreateGraphType( new AppendConnection( "+" ) ) );
            var sourceType = new SourceNode( "test" );
            var transferType = new TransferNode();

            var node1 = sourceType.CreateNode();
            graph.AddNode( node1 );

            var node2 = transferType.CreateNode();
            graph.AddNode( node2 );

            graph.Connect( sourceType.Out.FromNode( node1 ), transferType.In.FromNode( node2 ) );

            var result = await graph.ExecuteAsync(  ExecuteNodeAsync, _ => throw exceptionToCatch );
            
            Assert.IsNotEmpty( result.Exceptions );
            Assert.That( result.Exceptions, Has.Member( exceptionToCatch ) );
        }

        [ Test ]
        public void Execute_Should_Succeed_CatchException_DuringNodeActionExecution()
        {
            var exceptionToCatch = new Exception("some_exception");
            var graph = new GraphModel( GraphModelTests.GraphTypeWithConnections );
            graph.AddNode( new SourceNode( "test" ).CreateNode() );
            
            var result = graph.Execute( context => throw exceptionToCatch );

            Assert.IsNotEmpty( result.Exceptions );
            Assert.That( result.Exceptions, Has.Member( exceptionToCatch ) );
        }
        
        [ Test ]
        public void Execute_Should_Succeed_CatchException_DuringConnectionActionExecution()
        {
            var exceptionToCatch = new Exception("some_exception");
            
            var graph = new GraphModel( GraphModelTests.CreateGraphType( new AppendConnection( "+" ) ) );
            var sourceType = new SourceNode( "test" );
            var transferType = new TransferNode();

            var node1 = sourceType.CreateNode();
            graph.AddNode( node1 );

            var node2 = transferType.CreateNode();
            graph.AddNode( node2 );

            graph.Connect( sourceType.Out.FromNode( node1 ), transferType.In.FromNode( node2 ) );

            var result = graph.Execute( ExecuteNode, _ => throw exceptionToCatch );
            
            Assert.IsNotEmpty( result.Exceptions );
            Assert.That( result.Exceptions, Has.Member( exceptionToCatch ) );
        }

        [Test]
        public void ExecuteGraph_Should_Succeed_WithGraph()
        {
            var graphType = Substitute.For<IExecutableGraphType<string>>();

            var graph = new GraphModel( graphType );
            var nodeType = new SourceNode( "test" );
            var node = nodeType.CreateNode();
            graph.AddNode( node );

            var args = "test";
            var result = GraphExecutor.ExecuteGraph( graph, args );

            Assert.That( result, Is.Not.Null );

            graphType.Received().Execute( graph, args );
        }

        [Test]
        public void ExecuteGraphAsync_Should_Succeed_WithGraph()
        {
            var graphType = Substitute.For<IExecutableAsyncGraphType<string>>();
            graphType.ExecuteAsync( Arg.Any<GraphModel>(), Arg.Any<string>() ).Returns( Task.FromResult( Substitute.For<IGraphExecutionResult>() ) );

            var graph = new GraphModel( graphType );
            var nodeType = new SourceNode( "test" );
            var node = nodeType.CreateNode();
            graph.AddNode( node );

            var args = "test";
            var result = GraphExecutor.ExecuteGraphAsync( graph, args );

            Assert.That( result, Is.Not.Null );

            graphType.Received().ExecuteAsync( graph, args );
        }

        [Test]
        public void ExecuteGraph_Should_Succeed_WithGraphAsset()
        {
            var graphType = Substitute.For<IExecutableGraphType<string>>();

            var graph = new GraphModel( graphType );
            var nodeType = new SourceNode( "test" );
            var node = nodeType.CreateNode();
            graph.AddNode( node );

            var graphAsset = GraphAsset.Create( graph );

            var args = "test";
            var result = GraphExecutor.ExecuteGraph( graphAsset, args );

            Assert.That( result, Is.Not.Null );

            graphType.Received().Execute( graph, args );
        }

        [Test]
        public void ExecuteGraphAsync_Should_Succeed_WithGraphAsset()
        {
            var graphType = Substitute.For<IExecutableAsyncGraphType<string>>();
            graphType.ExecuteAsync( Arg.Any<GraphModel>(), Arg.Any<string>() ).Returns( Task.FromResult( Substitute.For<IGraphExecutionResult>() ) );

            var graph = new GraphModel( graphType );
            var nodeType = new SourceNode( "test" );
            var node = nodeType.CreateNode();
            graph.AddNode( node );

            var graphAsset = GraphAsset.Create( graph );

            var args = "test";
            var result = GraphExecutor.ExecuteGraphAsync( graphAsset, args );

            Assert.That( result, Is.Not.Null );

            graphType.Received().ExecuteAsync( graph, args );
        }

        [Test]
        public void ExecuteGraph_Should_Throw_WhenNullGraphModel()
        {
            Assert.That( () => GraphExecutor.ExecuteGraph( null as GraphModel, "test" ), Throws.ArgumentNullException );
        }

        [Test]
        public void ExecuteGraphAsync_Should_Throw_WhenNullGraphModel()
        {
            Assert.That( () => GraphExecutor.ExecuteGraphAsync( null as GraphModel, "test" ), Throws.ArgumentNullException );
        }

        [Test]
        public void ExecuteGraph_Should_Throw_WhenNullGraphAsset()
        {
            Assert.That( () => GraphExecutor.ExecuteGraph( null as GraphAsset, "test" ), Throws.ArgumentNullException );
        }

        [Test]
        public void ExecuteGraphAsync_Should_Throw_WhenNullGraphAsset()
        {
            Assert.That( () => GraphExecutor.ExecuteGraphAsync( null as GraphAsset, "test" ), Throws.ArgumentNullException );
        }

        [Test]
        public void ExecuteGraphAsset_Should_Throw_WhenNullGuid()
        {
            Assert.That( () => GraphExecutor.ExecuteGraphAsset( null as string, "test" ), Throws.ArgumentNullException );
        }

        [Test]
        public void ExecuteGraphAssetAsync_Should_Throw_WhenNullGuid()
        {
            Assert.That( () => GraphExecutor.ExecuteGraphAssetAsync( null as string, "test" ), Throws.ArgumentNullException );
        }

        private void ExecuteNode( INodeExecutionContext ctx )
        {
            (ctx.Node.Type as IExecutableNode).Execute( ctx );
        }

        private Task ExecuteNodeAsync( INodeExecutionContext ctx )
        {
            return (ctx.Node.Type as IExecutableNode).ExecuteAsync( ctx );
        }

        private void ExecuteConnection( IConnectionExecutionContext ctx )
        {
            (ctx.Connection.Type as IExecutableConnection).Execute( ctx );
        }

        private Task ExecuteConnectionAsync( IConnectionExecutionContext ctx )
        {
            return (ctx.Connection.Type as IExecutableConnection).ExecuteAsync( ctx );
        }

        private (bool HasValue, T Value) GetPortValue<T>( string portId, IGraphExecutionResult result, NodeModel node )
        {
            var hasValue = result.TryGetPortValue<T>( node.Port( portId ), out var value );
            return (hasValue, value);
        }

        private (bool HasValue, T Value) GetConnectionValue<T>( IGraphExecutionResult result, ConnectionModel connection )
        {
            var hasValue = result.TryGetConnectionValue<T>( connection, out var value );
            return (hasValue, value);
        }
    }
}