using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CrazyPanda.UnityCore.NodeEditor
{
    [ Serializable ]
    public class SGraph : BaseGraph< SNode, SConnection, SPort>
    {
        public string Type;

        [ JsonIgnore ] 
        private readonly Lazy< IGraphType > _graphType ;

        [ JsonIgnore ]
        public IGraphType GraphType => _graphType.Value;

        [ JsonProperty ] 
        private string _customSettingsFullTypeName = string.Empty;

        [ JsonProperty ] 
        private string _customSettingsSerializedValue = string.Empty;

        [ JsonIgnore ] 
        private IGraphSettings _customSettings;
        
        [ JsonIgnore ]
        public override IGraphSettings CustomSettings
        {
            get => _customSettings;
            set
            {
                _customSettings = value;
                _customSettingsFullTypeName = value?.GetType()?.FullName ?? string.Empty;

                OnCustomSettingsChanged?.Invoke( _customSettings );
            }
        }


        public event Action< IGraphSettings > OnCustomSettingsChanged;
        
        public SGraph()
        {
            _graphType = new Lazy< IGraphType >( () => GraphSerializer.StaticResolver.GetInstance< IGraphType >( Type ) ?? throw new InvalidOperationException( $"Graph type {Type ?? "<null>"} not found!" ) );
        }

        public override bool CanConnect( SPort from, SPort to )
        {
            if( from == null )
                throw new ArgumentNullException( nameof( from ) );

            if( to == null )
                throw new ArgumentNullException( nameof( to ) );

            // check direction
            if( from.Direction == PortDirection.Input || to.Direction == PortDirection.Output )
                return false;

            // check same node
            if( from.NodeId == to.NodeId)
                return false;
            
            // check if ports are in this graph
            if( Nodes.All( node => node.Id != @from.NodeId ) || Nodes.All( node => node.Id != to.NodeId ) )
                return false;

            // check types
            if( GraphType.FindConnectionType( from.Type, to.Type ) == null )
                return false;

            // check same connection exists
            if( from.Connections.Any( connection => connection.ToPortId == to.Id && connection.ToNodeId == to.NodeId ) )
                return false;

            return true;
        }        

        public override void Disconnect( SConnection connection )
        {
            RemovePort( connection.FromNodeId, connection.FromPortId );
            RemovePort( connection.ToNodeId, connection.ToPortId );
            
            base.Disconnect( connection );

            void RemovePort( string nodeId, string portId )
            {
                _nodes.FirstOrDefault( node => node.Id == nodeId )?.Ports?.FirstOrDefault( port => port.Id == portId )?.Connections?.Remove( connection );
            }
        }
        
        public void AddConnection( SConnection connection )
        {
            if( connection == null )
                throw new ArgumentNullException( nameof(connection) );

            if( _connections.Any( connection.Equals ) )
            {
                throw new ArgumentException( $"Connection '{connection}' already added to graph" );
            }

            _connections.Add( connection );

            InvokeChanged( addedConnections: new[] { connection } );
        }
        
        protected override SConnection CreateConnection( SPort @from, SPort to )
        {
            var ret = _changeSet.RemovedConnections?.FirstOrDefault( c => c.FromPortId == @from.Id && c.FromNodeId == from.NodeId && 
                                                                               c.ToPortId == to.Id && c.ToNodeId == to.NodeId  ) ??
                      new SConnection { FromNodeId = @from.NodeId, FromPortId = @from.Id, ToNodeId = to.NodeId, ToPortId = to.Id };

            RemoveConnectionsFromPort( from );
            RemoveConnectionsFromPort( to );

            from.Connections.Add( ret );
            to.Connections.Add( ret );
            
            DoAddConnection( ret );

            return ret;
        }

        [ OnSerializing ]
        private void OnSerializing( StreamingContext context )
        {
            if( _customSettings == null ) return;

            _customSettingsSerializedValue = JsonConvert.SerializeObject( CustomSettings );
        }

        [ OnDeserialized ]
        private void OnDeserialized( StreamingContext context )
        {
            if( string.IsNullOrEmpty( _customSettingsSerializedValue ) || string.IsNullOrEmpty( _customSettingsFullTypeName ) )
            {
                return;
            }

            _customSettings = ( IGraphSettings ) JsonConvert.DeserializeObject( _customSettingsSerializedValue, GraphSerializer.StaticResolver.FindType( _customSettingsFullTypeName ) );
        }
    }
}