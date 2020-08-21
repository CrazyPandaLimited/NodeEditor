﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface INodeExecutionContext
    {
        NodeModel Node { get; }
        T GetInput<T>( ConnectionModel connection );
        void SetOutput<T>( string portId, T value );
    }

    public interface IConnectionExecutionContext
    {
        ConnectionModel Connection { get; }
        T GetInput<T>();
        void SetOutput<T>( T value );
    }

    public interface IGraphExecutionResult
    {
        IReadOnlyCollection< Exception > Exceptions { get; }
        bool TryGetConnectionValue<T>( ConnectionModel connection, out T value );
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

            if( !port.Type.IsValueType && value == null )
                throw new ArgumentNullException( $"Cannot set value <null> to port of type {port.Type}" );

            if( !port.Type.IsAssignableFrom( value.GetType() ) )
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