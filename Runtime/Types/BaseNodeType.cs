using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseNodeType : INodeType
    {
        public virtual string Name => GetType().Name.Replace( "Type", "" );

        public void InitModel( NodeModel node )
        {
            node.PropertyBlock = CreatePropertyBlock();
        }

        public void PostLoad( NodeModel node )
        {
            if( node.Id == null )
                node.Id = Guid.NewGuid().ToString();

            node.Ports = CreatePorts();            
        }

        protected virtual IReadOnlyList<PortModel> CreatePorts()
        {
            return new PortModel[ 0 ];
        }

        protected virtual PropertyBlock CreatePropertyBlock()
        {
            return new PropertyBlock();
        }

        protected static PortModel TypedPort<T>( string id, PortDirection direction, PortCapacity capacity = PortCapacity.NotSet )
        {
            var ret = new PortModel() { Type = typeof( T ) };
            ret.Direction = direction;
            ret.Id = id;

            if( capacity == PortCapacity.NotSet )
            {
                // by default input ports are single, output ports are multiple
                ret.Capacity = direction == PortDirection.Input ? PortCapacity.Single : PortCapacity.Multiple;
            }

            return ret;
        }
    }
}