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

        public BasePortView( PortModel port, Orientation portOrientation, IEdgeConnectorListener edgeConnectorListener )
            : base( portOrientation,
                  port.Direction == PortDirection.Input ? Direction.Input : Direction.Output,
                  port.Capacity == PortCapacity.Multiple ? Capacity.Multi : Capacity.Single,
                  port.Type )
        {
            Port = port;
            portName = ObjectNames.NicifyVariableName( port.Id );
            m_EdgeConnector = CreateEdgeConnector( edgeConnectorListener );
            this.AddManipulator( m_EdgeConnector );
        }

        protected virtual EdgeConnector CreateEdgeConnector( IEdgeConnectorListener edgeConnectorListener )
        {
            return new EdgeConnector<BaseConnectionView>( edgeConnectorListener );
        }
    }

    public class BasePortView<TConnectionView> : BasePortView
        where TConnectionView : BaseConnectionView, new()
    {
        public BasePortView( PortModel port, Orientation portOrientation, IEdgeConnectorListener edgeConnectorListener )
            : base( port, portOrientation, edgeConnectorListener )
        {
        }

        protected override EdgeConnector CreateEdgeConnector( IEdgeConnectorListener edgeConnectorListener )
        {
            return new EdgeConnector<TConnectionView>( edgeConnectorListener );
        }
    }
}
