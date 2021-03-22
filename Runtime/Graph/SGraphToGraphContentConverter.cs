using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public sealed class SGraphToGraphContentConverter
    {
        private SGraph _sGraph;
        
        public GraphModel GraphModel { get; private set; }

        public void SetGraph( SGraph sGraph )
        {
            if( _sGraph != null )
            {
                _sGraph.GraphChanged -= OnGraphChanged;
            }

            _sGraph = sGraph;

            GraphModel = new GraphModel( sGraph.GraphType );
            sGraph.AddContentsToGraph( GraphModel );
            sGraph.GraphChanged += OnGraphChanged;
            GraphModel.SetSettings( _sGraph.GraphSettings );
        }
        
        private void OnGraphChanged( IReadOnlyList< SNode > addednodes, IReadOnlyList< SNode > removednodes, IReadOnlyList< SConnection > addedconnections, IReadOnlyList< SConnection > removedconnections )
        {
            foreach( SNode node in addednodes )
            {
                node.PortsChanged += OnPortsChanged;
            }
            
            foreach( var node in removednodes )
            {
                node.PortsChanged -= OnPortsChanged;
            }

            GraphModel.RemoveNodes( removednodes );
            GraphModel.RemoveConnections( removedconnections );
            GraphModel.AddNodes( addednodes );
            GraphModel.AddConnections( addedconnections );
        }

        private void OnPortsChanged( BaseNode< SNode, SPort >.NodePortsChangedArgs portsChangedArgs )
        {
            var changedNode = GraphModel.Nodes.First( node => node.Id == portsChangedArgs.Node.Id );
            
            if( portsChangedArgs.IsAdded )
            {
                changedNode.AddPort( portsChangedArgs.Port );
            }
            else
            {
                changedNode.RemovePort( changedNode.Port( portsChangedArgs.Port.Id ) );
            }
        }
    }
}