using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    class EdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly BaseGraphView _graphView;

        public EdgeConnectorListener( BaseGraphView graphView )
        {
            _graphView = graphView;
        }

        public void OnDrop( GraphView graphView, Edge edge )
        {
            var from = edge.output as BasePortView;
            var to = edge.input as BasePortView;

            if( !_graphView.Graph.CanConnect( from.Port, to.Port ) )
                return;

            _graphView.Graph.Connect( from.Port, to.Port );
        }

        public void OnDropOutsidePort( Edge edge, Vector2 position )
        {
            var w = _graphView.GetFirstAncestorOfType<BaseGraphEditorView<BaseGraphSettingsView<BaseGraphSettingsViewModel>, BaseGraphSettingsViewModel>>().Window;
            var pos = w.position.position + position;

            _graphView.nodeCreationRequest?.Invoke( new NodeCreationContext() { screenMousePosition = pos, target = edge } );
        }
    }
}
