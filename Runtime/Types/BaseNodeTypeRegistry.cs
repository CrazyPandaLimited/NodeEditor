using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.NodeEditor
{
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

        protected virtual bool ShouldProcessAssembly( Assembly asm )
        {
            if( ExcludeEditorAssemblies && asm.GetReferencedAssemblies().Any( a => a.Name.Contains( "UnityEditor" ) ) )
                return false;

            return true;
        }

        protected virtual bool ShouldProcessType( Type type )
        {
            return true;
        }
    }
}
