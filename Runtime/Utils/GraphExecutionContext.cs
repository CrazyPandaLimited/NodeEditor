using System;
using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Context of single node execution
    /// </summary>
    public interface INodeExecutionContext
    {
        /// <summary>
        /// Executing node
        /// </summary>
        NodeModel Node { get; }

        /// <summary>
        /// Returns value of incoming connection
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="connection">Connection for which we need value</param>
        T GetInput<T>( ConnectionModel connection );

        /// <summary>
        /// Sets output value for given port id
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="portId"></param>
        /// <param name="value"></param>
        void SetOutput<T>( string portId, T value );
    }

    /// <summary>
    /// Context of a single connection execution
    /// </summary>
    public interface IConnectionExecutionContext
    {
        /// <summary>
        /// Executing connection
        /// </summary>
        ConnectionModel Connection { get; }

        /// <summary>
        /// Returns input value of current connection
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        T GetInput<T>();

        /// <summary>
        /// Sets output value for current connection
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="value">Value to set</param>
        void SetOutput<T>( T value );
    }

    /// <summary>
    /// Result of a graph execution
    /// </summary>
    public interface IGraphExecutionResult
    {
        /// <summary>
        /// Collection of all exceptions happened during execution
        /// </summary>
        IReadOnlyCollection< Exception > Exceptions { get; }

        /// <summary>
        /// Returns value computed for given <see cref="ConnectionModel"/> if it exists
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="connection">Connection for which we need value</param>
        /// <param name="value">Variable to store retrieved value</param>
        /// <returns>True if value was found</returns>
        bool TryGetConnectionValue<T>( ConnectionModel connection, out T value );

        /// <summary>
        /// Returns value computed for given <see cref="PortModel"/> if it exists
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="port">Port for which we need value</param>
        /// <param name="value">Variable to store retrieved value</param>
        /// <returns>True if value was found</returns>
        bool TryGetPortValue<T>( PortModel port, out T value );
    }

    public class GraphExecutionContext : INodeExecutionContext, IConnectionExecutionContext, IGraphExecutionResult
    {
        private Dictionary<PortModel, object> _portValues = new Dictionary<PortModel, object>();
        private Dictionary<ConnectionModel, object> _connectionValues = new Dictionary<ConnectionModel, object>();
        private readonly List<Exception> _exceptions = new List< Exception >();
        public NodeModel Node { get; set; }

        public ConnectionModel Connection { get; set; }

        public IReadOnlyCollection< Exception > Exceptions => _exceptions;

        public void ValidateOutputs()
        {
            foreach( var port in Node.OutputPorts() )
            {
                if( !_portValues.ContainsKey( port ) )
                {
                    throw new InvalidOperationException( $"Not all outputs set for node {Node}" );
                }
            }
        }

        public void AddException( Exception e )
        {
            _exceptions.Add( e );
        }
        
        T INodeExecutionContext.GetInput<T>( ConnectionModel connection )
        {
            if( !TryGetInput( connection, out var valueObject ) )
                throw new KeyNotFoundException( $"Cannot find value for connection {connection}" );

            if( !(valueObject is T) )
                throw new ArgumentException( $"Expected value of type {typeof( T ).Name} but got {valueObject?.GetType().Name ?? "<null>"}" );

            return ( T )valueObject;
        }

        void INodeExecutionContext.SetOutput<T>( string portId, T value )
        {
            if( portId == null )
                throw new ArgumentNullException( nameof( portId ) );

            var port = Node.OutputPorts().FirstOrDefault( p => p.Id == portId );

            if( port == null )
                throw new ArgumentException( $"Cannot find port with id '{portId}'" );

            if( port.Node != Node )
                throw new InvalidOperationException( $"Cannot modify output of other nodes" );

            if( port.Type.IsValueType && value == null )
                throw new ArgumentNullException( $"Cannot set value <null> to port of type {port.Type}" );

            if( value != null && !port.Type.IsAssignableFrom( value.GetType() ) )
                throw new InvalidOperationException( $"Cannot set value '{value}' of type {value.GetType().Name} to port of type {port.Type}" );
            
            _portValues[ port ] = value;
        }

        T IConnectionExecutionContext.GetInput<T>()
        {
            if( _portValues.TryGetValue( Connection.From, out var value ) )
                return ( T )value;

            throw new KeyNotFoundException( $"Cannot find value for connection {Connection}" );
        }

        void IConnectionExecutionContext.SetOutput<T>( T value )
        {
            var port = Connection.To;

            if( !port.Type.IsAssignableFrom( typeof( T ) ) )
                throw new InvalidOperationException( $"Cannot set value '{value}' of type {typeof( T ).Name} to port of type {port.Type} in connection {Connection}" );

            _connectionValues[ Connection ] = value;
        }

        bool IGraphExecutionResult.TryGetConnectionValue<T>( ConnectionModel connection, out T value )
        {
            if( TryGetInput( connection, out var valueObject ) && valueObject is T )
            {
                value = ( T )valueObject;
                return true;
            }

            value = default;
            return false;
        }

        bool IGraphExecutionResult.TryGetPortValue<T>( PortModel port, out T value  )
        {
            if( _portValues.TryGetValue( port, out var valueObject ) )
            {
                value = ( T )valueObject;
                return true;
            }

            value = default;
            return false;
        }

        private bool TryGetInput( ConnectionModel connection, out object value  )
        {
            if( connection == null )
                throw new ArgumentNullException( nameof( connection ) );

            if( _connectionValues.TryGetValue( connection, out value ) )
                return true;

            if( _portValues.TryGetValue( connection.From, out value ) )
                return true;

            value = default;
            return false;
        }
    }
}