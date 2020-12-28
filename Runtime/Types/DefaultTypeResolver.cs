using System;
using System.Collections.Generic;
using System.Reflection;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Default implementation of <see cref="IGraphTypeResolver"/>. Uses Type.FullName as type name
    /// </summary>
    class DefaultTypeResolver : IGraphTypeResolver
    {
        private readonly Dictionary< string, object > _instances = new Dictionary< string, object >();
        private readonly Lazy< Assembly[] > _assemblies = new Lazy< Assembly[] >( () => AppDomain.CurrentDomain.GetAssemblies() );

        public T GetInstance< T >( string typeName )
            where T : class
        {
            if( string.IsNullOrEmpty( typeName ) )
            {
                throw new ArgumentNullException( nameof( typeName ) );
            }

            if( !_instances.TryGetValue( typeName, out var instance ) )
            {
                var type = FindType( typeName );

                if( type == null )
                {
                    return default;
                }

                instance = Activator.CreateInstance( type );
                _instances[ typeName ] = instance;
            }

            return instance as T;
        }

        public string GetTypeName< T >( T instance )
            where T : class
        {
            if( instance == null )
            {
                throw new ArgumentNullException( nameof( instance ) );
            }

            return instance.GetType().FullName;
        }

        public Type FindType( string typeName )
        {
            if( string.IsNullOrEmpty( typeName ) )
                return null;

            foreach( var assembly in _assemblies.Value )
            {
                var type = assembly.GetType( typeName );
                if( type != null )
                    return type;
            }

            return null;
        }
    }
}
