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
    /// <summary>
    /// Base class for Port view
    /// </summary>
    public class BasePortView : Port
    {
        /// <summary>
        /// Associated port
        /// </summary>
        public PortModel Port { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Port that needs view</param>
        /// <param name="portOrientation">Orientation of port</param>
        /// <param name="edgeConnectorListener">Edge connector listener for a port</param>
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

            tooltip = port.Type.ToString();
        }

        protected virtual EdgeConnector CreateEdgeConnector( IEdgeConnectorListener edgeConnectorListener )
        {
            return new EdgeConnector<BaseConnectionView>( edgeConnectorListener );
        }
    }

    /// <summary>
    /// Base class for port view that needs specific connection type
    /// </summary>
    /// <typeparam name="TConnectionView">Connection type for this port</typeparam>
    public class BasePortView<TConnectionView> : BasePortView
        where TConnectionView : BaseConnectionView, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Port that needs view</param>
        /// <param name="portOrientation">Orientation of port</param>
        /// <param name="edgeConnectorListener">Edge connector listener for a port</param>
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
