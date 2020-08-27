using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base implementation of <see cref="INodeTypeRegistry"/>.
    /// Scans loaded assemblies for types implementing <see cref="INodeType"/>
    /// </summary>
    public abstract class BaseNodeTypeRegistry : INodeTypeRegistry
    {
        private Lazy<IReadOnlyList<INodeType>> _allNodes;

        public IReadOnlyList<INodeType> AvailableNodes => _allNodes.Value;

        protected bool ExcludeEditorAssemblies { get; set; } = false;

        protected BaseNodeTypeRegistry()
        {
            _allNodes = new Lazy<IReadOnlyList<INodeType>>( CollectNodeTypes );
        }

        private IReadOnlyList<INodeType> CollectNodeTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var ret = new List<INodeType>();
            var emptyArgs = new Type[ 0 ];

            foreach( var asm in assemblies )
            {
                if( !ShouldProcessAssembly( asm ) )
                    continue;

                var types = asm.GetTypes();

                foreach( var type in types )
                {
                    if( !ShouldProcessType( type ) )
                        continue;

                    if( !type.IsAbstract && !type.IsGenericType && typeof( INodeType ).IsAssignableFrom( type ) && type.GetConstructor( emptyArgs ) != null )
                    {
                        var nodeType = Activator.CreateInstance( type ) as INodeType;
                        ret.Add( nodeType );
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns true if this assembly should be scanned
        /// </summary>
        /// <param name="asm">Assembly to check</param>
        protected virtual bool ShouldProcessAssembly( Assembly asm )
        {
            if( ExcludeEditorAssemblies && asm.GetReferencedAssemblies().Any( a => a.Name.Contains( "UnityEditor" ) ) )
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if this type should be added to collection
        /// </summary>
        /// <param name="type">Type to check</param>
        protected virtual bool ShouldProcessType( Type type )
        {
            return true;
        }
    }
}
