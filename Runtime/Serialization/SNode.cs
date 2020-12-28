using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    [ Serializable ]
    public class SNode : BaseNode< SNode, SPort >
    {
        public string Type;

        /// <summary>
        /// Unique id of a node
        /// </summary>
        public override string Id { get; set; }
        /// <summary>
        /// Position of a node view inside graph
        /// </summary>
        public Vector2 Position = default;

        public string Properties = string.Empty;

        public override PropertyBlock PropertyBlock { get; set; }

        /// <summary>
        /// Type of a node
        /// </summary>
        [ NonSerialized, JsonIgnore ] 
        public INodeType NodeType;
        
        public SNode()
        {
            base.PortsChanged += OnPortsChanged; 
        }
        
        public static implicit operator SNode( NodeModel node )
        {
            var typeResolver = node.Graph.Type as IGraphTypeResolver ?? GraphSerializer.StaticResolver;
            var typeName = typeResolver.GetTypeName( node.Type ) ?? throw new InvalidOperationException( $"Cannot resolve name for node {node}" );

            var sNode = new SNode
            {
                Id = node.Id,
                Type = typeName,
                Properties = JsonConvert.SerializeObject( node.PropertyBlock ),
                PropertyBlock = node.PropertyBlock,
                NodeType = node.Type
            };
                
            foreach( var port in node.Ports )
            {
                sNode.AddPort( port );
            }

            return sNode;
        }
        
        public override void AddPort( IPort port )
        {
            switch( port )
            {
                case PortModel portModel: 
                    AddPort( portModel );
                    break;
                case SPort sPort:
                    AddPort( sPort );
                    break;
            }
        }

        public override void RemovePort( IPort port )
        {
            switch( port )
            {
                case PortModel portModel: 
                    RemovePort( portModel );
                    break;
                case SPort sPort:
                    RemovePort( sPort );
                    break;
            }
        }

        protected override bool NeedToAddPort( SPort port )
        {
            if( _ports.Any( p => DoesPortExists( p, port.Id ) ) )
            {
                throw new ArgumentException( $"Port with id {port.Id} already added to node {port.NodeId}", nameof(port) );
            }

            return true;
        }

        protected override bool DoesPortExists( SPort sPort, string portId )
        {
            return sPort.Id == portId;
        }
        
        private void OnPortsChanged( NodePortsChangedArgs args )
        {
            args.Port.NodeId = args.IsAdded ? Id : string.Empty;
        }
    }
}