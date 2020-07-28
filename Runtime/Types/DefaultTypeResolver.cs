using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Default implementation of <see cref="IGraphTypeResolver"/>. Uses Type.FullName as type name
    /// </summary>
    class DefaultTypeResolver : IGraphTypeResolver
    {
        private readonly Dictionary< string, object > _instances = new Dictionary< string, object >();

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

        private Type FindType( string typeName )
        {
            if( string.IsNullOrEmpty( typeName ) )
                return null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach( var assembly in assemblies )
            {
                var type = assembly.GetType( typeName );
                if( type != null )
                    return type;
            }

            return null;
        }
    }
}
