using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    interface IExecutableNode
    {
        void Execute( INodeExecutionContext ctx );
        Task ExecuteAsync( INodeExecutionContext ctx );
    }

    interface IExecutableConnection
    {
        void Execute( IConnectionExecutionContext ctx );
        Task ExecuteAsync( IConnectionExecutionContext ctx );
    }

    class BaseExecutableNode : BaseNodeType, IExecutableNode
    {
        public virtual void Execute( INodeExecutionContext ctx )
        {
        }

        public virtual Task ExecuteAsync( INodeExecutionContext ctx )
        {
            Execute( ctx );
            return Task.CompletedTask;
        }
    }

    class BaseExecutableConnection : BaseConnectionType, IExecutableConnection
    {
        public virtual void Execute( IConnectionExecutionContext ctx )
        {
        }

        public virtual Task ExecuteAsync( IConnectionExecutionContext ctx )
        {
            Execute( ctx );
            return Task.CompletedTask;
        }
    }

    class SourceNode : BaseExecutableNode
    {
        public OutputPort<string> Out { get; }

        public string Value { get; }

        public SourceNode()
            : this( "<not set>" )
        {
        }

        public SourceNode( string value )
        {
            Value = value;
        }

        public override void Execute( INodeExecutionContext ctx )
        {
            Out.Set( ctx, Value );
        }
    }

    class TransferNode : BaseExecutableNode
    {
        public InputPort<string> In { get; }
        public OutputPort<string> Out { get; }

        public override void Execute( INodeExecutionContext ctx )
        {
            Out.Set( ctx, In.Get( ctx ) );
        }
    }

    class DestinationNode : BaseExecutableNode
    {
        public InputPort<string> In { get; }

        public string OutValue { get; private set; }

        public override void Execute( INodeExecutionContext ctx )
        {
            OutValue = In.Get( ctx );
        }
    }

    class AppendConnection : BaseExecutableConnection, IExecutableConnection
    {
        private readonly string _append;

        public AppendConnection( string append )
        {
            _append = append;
        }

        public override void Execute( IConnectionExecutionContext ctx )
        {
            var input = ctx.GetInput<string>();
            ctx.SetOutput( input + _append );
        }
    }
}