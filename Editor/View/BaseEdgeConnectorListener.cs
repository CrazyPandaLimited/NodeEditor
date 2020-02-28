using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    class EdgeConnectorListener : IEdgeConnectorListener
    {
        public void OnDrop( GraphView graphView, Edge edge )
        {
            var baseGraphView = graphView as BaseGraphView;
            var from = edge.output as BasePortView;
            var to = edge.input as BasePortView;

            if( !baseGraphView.Graph.CanConnect( from.Port, to.Port ) )
                return;

            baseGraphView.Graph.Connect( from.Port, to.Port );
        }

        public void OnDropOutsidePort( Edge edge, Vector2 position )
        {
        }
    }
}
