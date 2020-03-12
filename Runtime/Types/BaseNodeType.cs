using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseNodeType : INodeType
    {
        private static PortModel[] _emptyPorts = new PortModel[ 0 ];

        private List<PortDescription> _collectedPorts;

        public virtual string Name => GetType().Name.Replace( "Type", "" );

        public void InitModel( NodeModel node )
        {
            node.PropertyBlock = CreatePropertyBlock( node );
        }

        public void PostLoad( NodeModel node )
        {
            if( node.Id == null )
                node.Id = Guid.NewGuid().ToString();

            CreatePorts( node );
        }

        protected virtual void CreatePorts( NodeModel node )
        {
            if( _collectedPorts == null )
                CollectPortsFromProperties();

            if( _collectedPorts.Count == 0 )
                return;

            for( int i = 0; i < _collectedPorts.Count; i++ )
            {
                var desc = _collectedPorts[ i ];
                node.AddPort( CreatePort( desc.Id, desc.Type, desc.Direction, desc.Capacity ) );
            }
        }

        protected virtual PropertyBlock CreatePropertyBlock( NodeModel node )
        {
            return new PropertyBlock();
        }

        protected static PortModel CreatePort<T>( string id, PortDirection direction, PortCapacity capacity = PortCapacity.NotSet )
        {
            return CreatePort( id, typeof( T ), direction, capacity );
        }

        protected static PortModel CreatePort( string id, Type type, PortDirection direction, PortCapacity capacity = PortCapacity.NotSet )
        {
            if( capacity == PortCapacity.NotSet )
            {
                // by default input ports are single, output ports are multiple
                capacity = direction == PortDirection.Input ? PortCapacity.Single : PortCapacity.Multiple;
            }

            var ret = new PortModel() { Type = type };
            ret.Direction = direction;
            ret.Id = id;
            ret.Capacity = capacity;

            return ret;
        }

        private void CollectPortsFromProperties()
        {
            _collectedPorts = new List<PortDescription>();

            var type = GetType();
            var props = type.GetProperties();

            while( type != typeof( object ) )
            {
                var fields = type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic );
                ProcessFields( fields, props );

                type = type.BaseType;
            }
        }

        private void ProcessFields( FieldInfo[] fields, PropertyInfo[] properties )
        {
            foreach( var field in fields )
            {
                var fieldType = field.FieldType;

                PortDescription desc = default;

                if( fieldType.GetGenericTypeDefinition() == typeof( InputPort<> ) )
                {
                    desc.Type = fieldType.GetGenericArguments()[ 0 ];
                    desc.Direction = PortDirection.Input;
                }
                else if( fieldType.GetGenericTypeDefinition() == typeof( InputPortMulti<> ) )
                {
                    desc.Type = fieldType.GetGenericArguments()[ 0 ];
                    desc.Direction = PortDirection.Input;
                    desc.Capacity = PortCapacity.Multiple;
                }
                else if( fieldType.GetGenericTypeDefinition() == typeof( OutputPort<> ) )
                {
                    desc.Type = fieldType.GetGenericArguments()[ 0 ];
                    desc.Direction = PortDirection.Output;
                }
                else if( fieldType.GetGenericTypeDefinition() == typeof( OutputPortSingle<> ) )
                {
                    desc.Type = fieldType.GetGenericArguments()[ 0 ];
                    desc.Direction = PortDirection.Output;
                    desc.Capacity = PortCapacity.Single;
                }

                if( desc.Type != null )
                {
                    var value = field.GetValue( this );
                    //var value = Activator.CreateInstance( t );
                    var idField = fieldType.GetField( "Id" );

                    desc.Id = idField.GetValue( value ) as string;

                    if( string.IsNullOrEmpty( desc.Id ) )
                    {
                        desc.Id = properties.First( p => field.Name.Contains( p.Name ) ).Name;
                        fieldType.GetField( "Id" ).SetValue( value, desc.Id );
                    }

                    field.SetValue( this, value );

                    _collectedPorts.Add( desc );
                }
            }
        }

        struct PortDescription
        {
            public string Id;
            public Type Type;
            public PortDirection Direction;
            public PortCapacity Capacity;
        }
    }
}