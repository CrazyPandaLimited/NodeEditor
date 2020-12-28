using System.Collections.Generic;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface IGraph
    {
        IEnumerable< INode > Nodes { get; }

        void AddNode( INode node );

        void RemoveNode( INode node );
        
        bool CanConnect( IPort from, IPort to );

        IConnection Connect( IPort from, IPort to );
        
        void Disconnect( IConnection connection );
    }
}