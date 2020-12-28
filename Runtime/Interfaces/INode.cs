using System.Collections.Generic;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface INode
    {
        string Id { get; set; }
        
        PropertyBlock PropertyBlock { get; set; }

        IEnumerable< IPort > Ports { get; }

        void AddPort( IPort port );

        void RemovePort( IPort port );
        
        void RemovePort( string portId );
    }
}