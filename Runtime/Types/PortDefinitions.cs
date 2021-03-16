using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /* These structs have copy-pasted methods because there is no way to make them work via interface extensions with "compact syntax" like this:
     *   var value = MyPortStruct.Get( context ); 
     *
     * With extensions you will have to do something like this:
     *   var value = MyPortStruct.Get< InputPort< List< int > >, List< int > >( context );
     *
     * which is very inconvinient, right?
     * 
     * They are also located in a single file because of this. You'll less likely miss something when changing stuff here.
     */

    /// <summary>
    /// Defines input port with <see cref="PortCapacity.Single"/>
    /// </summary>
    /// <typeparam name="T">Port Type</typeparam>
    public struct InputPort<T>
    {
        /// <summary>
        /// Port Id. You should not modify it
        /// </summary>
        public string Id;

        public bool Optional;

        /// <summary>
        /// Returns actual <see cref="PortModel"/> produced by this definition
        /// </summary>
        /// <param name="node">Target node</param>
        public PortModel FromNode( NodeModel node ) => node.Port( Id );

        /// <summary>
        /// Returns value from given connection. Does NOT check if it's from this port.
        /// </summary>
        public T Get( INodeExecutionContext ctx, ConnectionModel connection ) => ctx.GetInput<T>( connection );

        /// <summary>
        /// Returns value from connection by its index.
        /// </summary>
        /// <param name="connectionIndex">Index of connection</param>
        public T Get( INodeExecutionContext ctx, int connectionIndex ) => ctx.GetInput<T>( ctx.Node.Port( Id ).Connections[ connectionIndex ] );

        /// <summary>
        /// Returns value from single connection. There must be only one connection to this port.
        /// </summary>
        public T Get( INodeExecutionContext ctx ) => ctx.GetInput<T>( ctx.Node.Port( Id ).Connections.Single() );

        /// <summary>
        /// Conversion operator to simplify Id initialization
        /// </summary>
        public static implicit operator InputPort<T>( string id ) => new InputPort<T> { Id = id };

        /// <summary>
        /// Conversion operator to simplify Id retrieval
        /// </summary>
        public static implicit operator string( in InputPort<T> portDefinition ) => portDefinition.Id;
    }

    /// <summary>
    /// Defines input port with <see cref="PortCapacity.Multiple"/>
    /// </summary>
    /// <typeparam name="T">Port Type</typeparam>
    public struct InputPortMulti<T>
    {
        /// <summary>
        /// Port Id. You should not modify it
        /// </summary>
        public string Id;

        public bool Optional;

        /// <summary>
        /// Returns actual <see cref="PortModel"/> produced by this definition
        /// </summary>
        /// <param name="node">Target node</param>
        public PortModel FromNode( NodeModel node ) => node.Port( Id );

        /// <summary>
        /// Returns value from given connection. Does NOT check if it's from this port.
        /// </summary>
        public T Get( INodeExecutionContext ctx, ConnectionModel connection ) => ctx.GetInput<T>( connection );

        /// <summary>
        /// Returns value from connection by its index.
        /// </summary>
        /// <param name="connectionIndex">Index of connection</param>
        public T Get( INodeExecutionContext ctx, int connectionIndex ) => ctx.GetInput<T>( ctx.Node.Port( Id ).Connections[ connectionIndex ] );

        /// <summary>
        /// Returns values from all connections.
        /// </summary>
        public IEnumerable<T> Get( INodeExecutionContext ctx ) => ctx.Node.Port( Id ).Connections.Select( c => ctx.GetInput<T>( c ) );

        /// <summary>
        /// Conversion operator to simplify Id initialization
        /// </summary>
        public static implicit operator InputPortMulti<T>( string id ) => new InputPortMulti<T> { Id = id };

        /// <summary>
        /// Conversion operator to simplify Id retrieval
        /// </summary>
        public static implicit operator string( in InputPortMulti<T> portDefinition ) => portDefinition.Id;
    }

    /// <summary>
    /// Defines output port with <see cref="PortCapacity.Multiple"/>
    /// </summary>
    /// <typeparam name="T">Port Type</typeparam>
    public struct OutputPort<T>
    {
        /// <summary>
        /// Port Id. You should not modify it
        /// </summary>
        public string Id;

        public bool Optional;

        /// <summary>
        /// Returns actual <see cref="PortModel"/> produced by this definition
        /// </summary>
        /// <param name="node">Target node</param>
        public PortModel FromNode( NodeModel node ) => node.Port( Id );

        /// <summary>
        /// Sets value to the port
        /// </summary>
        /// <param name="value">New Value</param>
        public void Set( INodeExecutionContext ctx, T value ) => ctx.SetOutput( Id, value );

        /// <summary>
        /// Conversion operator to simplify Id initialization
        /// </summary>
        public static implicit operator OutputPort<T>( string id ) => new OutputPort<T> { Id = id };

        /// <summary>
        /// Conversion operator to simplify Id retrieval
        /// </summary>
        public static implicit operator string( in OutputPort<T> portDefinition ) => portDefinition.Id;
    }

    /// <summary>
    /// Defines output port with <see cref="PortCapacity.Single"/>
    /// </summary>
    /// <typeparam name="T">Port Type</typeparam>
    public struct OutputPortSingle<T>
    {
        /// <summary>
        /// Port Id. You should not modify it
        /// </summary>
        public string Id;

        public bool Optional;

        /// <summary>
        /// Returns actual <see cref="PortModel"/> produced by this definition
        /// </summary>
        /// <param name="node">Target node</param>
        public PortModel FromNode( NodeModel node ) => node.Port( Id );

        /// <summary>
        /// Sets value to the port
        /// </summary>
        /// <param name="value">New Value</param>
        public void Set( INodeExecutionContext ctx, T value ) => ctx.SetOutput( Id, value );

        /// <summary>
        /// Conversion operator to simplify Id initialization
        /// </summary>
        public static implicit operator OutputPortSingle<T>( string id ) => new OutputPortSingle<T> { Id = id };

        /// <summary>
        /// Conversion operator to simplify Id retrieval
        /// </summary>
        public static implicit operator string( in OutputPortSingle<T> portDefinition ) => portDefinition.Id;
    }
}