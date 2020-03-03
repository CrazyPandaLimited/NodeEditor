using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.NodeEditor
{
    class DefaultConnectionTypeRegistry : IConnectionTypeRegistry
    {
        private IConnectionType _defaultConnectionType = new BaseConnectionType();

        public IConnectionType FindConnectionType( Type from, Type to )
        {
            if( !to.IsAssignableFrom( from ) )
                return null;

            return _defaultConnectionType;
        }
    }
}
