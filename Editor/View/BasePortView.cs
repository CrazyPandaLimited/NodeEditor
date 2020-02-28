using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BasePortView : Port
    {
        public PortModel Port { get; }

        public BasePortView( PortModel port, Orientation portOrientation )
            : base( portOrientation,
                  port.Direction == PortDirection.Input ? Direction.Input : Direction.Output,
                  port.Capacity == PortCapacity.Multiple ? Capacity.Multi : Capacity.Single,
                  port.Type )
        {
            Port = port;
            portName = ObjectNames.NicifyVariableName( port.Id );
        }

        public void SetupEdgeConnector<TEdge>( IEdgeConnectorListener edgeConnectorListener )
            where TEdge : BaseConnectionView, new()
        {
            m_EdgeConnector = new EdgeConnector<TEdge>( edgeConnectorListener );
            this.AddManipulator( m_EdgeConnector );
        }
    }
}
