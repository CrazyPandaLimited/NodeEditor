using System;
using System.Linq;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Model of a graph. Contains nodes and connections between them
    /// </summary>
    public sealed class GraphModel : BaseGraph< NodeModel, ConnectionModel, PortModel, GraphSettingsModel>
    {
        /// <summary>
        /// Type of graph
        /// </summary>
        public IGraphType Type { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of graph</param>
        public GraphModel( IGraphType type )
        {
            Type = type ?? throw new ArgumentNullException( nameof(type) );
        }

        public override void AddNode( NodeModel node )
        {
            if( node == null )
                throw new ArgumentNullException( nameof( node ) );
            
            if( node.Graph != null )
                throw new ArgumentException( $"Node '{node}' already added to graph" );

            base.AddNode( node );
        }

        public override bool CanConnect( PortModel from, PortModel to )
        {
            if( from == null )
                throw new ArgumentNullException( nameof(from) );

            if( to == null )
                throw new ArgumentNullException( nameof(to) );

            // check direction
            if( from.Direction == PortDirection.Input || to.Direction == PortDirection.Output )
                return false;

            // check same node
            if( from.Node == to.Node )
                return false;

            // check if ports are in this graph
            if( from.Node?.Graph != this || to.Node?.Graph != this )
                return false;

            // check types
            if( Type.FindConnectionType( from.Type, to.Type ) == null )
                return false;

            // check same connection exists
            if( from.Connections.Any( c => c.To == to ) )
                return false;

            return true;
        }

        public override void Disconnect( ConnectionModel connection )
        {
            base.Disconnect( connection );

            connection.From.Connections.Remove( connection );
            connection.To.Connections.Remove( connection );
            
            if( _changeSet == null )
            {
                // Connection data must be cleared after callback is fired, or receiver will not know which connection is it
                // ChangeSet will handle this later
                connection.From = null;
                connection.To = null;
            }
        }

        protected override ConnectionModel CreateConnection( PortModel @from, PortModel to )
        {
            var ret = _changeSet.RemovedConnections?.FirstOrDefault( c => c.From == from && c.To == to );

            if( ret == null )
            {
                ret = new ConnectionModel( Type.FindConnectionType( from.Type, to.Type ) ) { From = from, To = to, };

                ret.Type.PostLoad( ret );
            }

            RemoveConnectionsFromPort( from );
            RemoveConnectionsFromPort( to );
            
            from.Connections.Add( ret );
            to.Connections.Add( ret );

            DoAddConnection( ret );

            return ret;
        }

        protected override void OnNodeAdded( NodeModel node )
        {
            node.Graph = this;
        }

        protected override void OnNodeRemoved( NodeModel node )
        {
            node.Graph = null;
        }

        protected override BaseGraph< NodeModel, ConnectionModel, PortModel, GraphSettingsModel>.ChangeSet CreateChangeSet()
        {
            return new ChangeSet( this );
        }

        private new sealed class ChangeSet : BaseGraph< NodeModel, ConnectionModel, PortModel, GraphSettingsModel>.ChangeSet
        {
            public ChangeSet( BaseGraph< NodeModel, ConnectionModel, PortModel, GraphSettingsModel> graph ) : base( graph )
            {
            }

            public override void Dispose()
            {
                base.Dispose();

                if( RemovedConnections != null )
                {
                    // now, after callback was fired, we may clear Connection data
                    for( int i = 0, k = RemovedConnections.Count; i < k; ++i )
                    {
                        var c = RemovedConnections[ i ];
                        c.From = null;
                        c.To = null;
                    }
                }
            }
        }
    }
}