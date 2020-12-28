using System;
using System.Linq;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Node in a graph
    /// </summary>
    public class NodeModel : BaseNode<NodeModel, PortModel>
    {
        private string _id;
        private PropertyBlock _propertyBlock;
        private GraphModel _graph;
        private INodeType _type;

        /// <summary>
        /// Unique id of a node
        /// </summary>
        public override string Id
        {
            get => _id;
            set => this.SetOnce( ref _id, value );
        }

        /// <summary>
        /// Type of a node
        /// </summary>
        public INodeType Type
        {
            get => _type;
            set => this.SetOnceOrNull( ref _type, value );
        }

        /// <summary>
        /// Owning graph or null
        /// </summary>
        public GraphModel Graph
        {
            get => _graph;
            set => this.SetOnceOrNull( ref _graph, value );
        }

        /// <summary>
        /// Custom properties associated with this node. Initialized by <see cref="INodeType"/>
        /// </summary>
        public override PropertyBlock PropertyBlock
        {
            get => _propertyBlock;
            set => this.SetOnce( ref _propertyBlock, value );
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

        public NodeModel()
        {
            base.PortsChanged += OnPortsChanged;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of a node</param>
        public NodeModel( INodeType type ) : this()
        {
            Type = type ?? throw new ArgumentNullException( nameof( type ) );
            Type.Init( this );
        }
        
        protected override bool NeedToAddPort( PortModel port )
        {
            if( port.Node != null || _ports.Any( p => DoesPortExists( p, port.Id ) ) )
                throw new ArgumentException( $"Port with id {port.Id} already added to node {port.Node ?? this}", nameof(port) );

            return true;
        }

        protected override bool DoesPortExists( PortModel portModel, string portId )
        {
            return portModel.Id == portId;
        }

        public override string ToString() => $"{Type.Name}. Id: {Id}";

        private void OnPortsChanged( NodePortsChangedArgs portsChangedArgs )
        {
            if( portsChangedArgs.IsAdded )
            {
                portsChangedArgs.Port.Node = this;
            }
            else
            {
                foreach( var c in portsChangedArgs.Port.Connections.ToArray() ) 
                {
                    Graph.Disconnect( c );
                }

                portsChangedArgs.Port.Node = null;
            }
        }

        public static implicit operator NodeModel( SNode sNode )
        {
            var node = new NodeModel
            {
                Type = sNode.NodeType,
                Id = sNode.Id,
                PropertyBlock = sNode.PropertyBlock
            };

            foreach( var port in sNode.Ports )
            {
                node.AddPort( port );
            }

            return node;
        }
    }
}