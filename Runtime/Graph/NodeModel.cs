using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class NodeModel
    {
        private string _id;
        private IReadOnlyList<PortModel> _ports;
        private PropertyBlock _propertyBlock;

        public string Id
        {
            get => _id;
            set => this.SetOnce( ref _id, value );
        }

        public INodeType Type { get; }

        public GraphModel Graph { get; set; }

        public IReadOnlyList<PortModel> Ports
        {
            get => _ports;
            set
            {
                this.SetOnce( ref _ports, value );

                foreach( PortModel port in _ports )
                    port.Node = this;
            }
        }

        public Vector2 Position { get; set; }

        public PropertyBlock PropertyBlock
        {
            get => _propertyBlock;
            set => this.SetOnce( ref _propertyBlock, value );
        }

        public NodeModel( INodeType type )
        {
            Type = type;
            Type.InitModel( this );
        }

        public override string ToString() => $"{Type.Name}. Id: {Id}";
    }
}