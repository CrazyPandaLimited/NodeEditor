using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.NodeEditor
{
    class DefaultTypeResolver
    {
        private Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public T GetInstance<T>( Type type )
            where T : class
        {
            if( type == null )
                throw new ArgumentNullException( nameof( type ) );

            if( !_instances.TryGetValue( type, out var instance ) )
            {
                instance = Activator.CreateInstance( type );
                _instances[ type ] = instance;
            }

            return instance as T;
        }
    }
}
