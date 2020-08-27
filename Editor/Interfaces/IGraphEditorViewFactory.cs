using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Creates View objects for nodes and connections
    /// </summary>
    public interface IGraphEditorViewFactory
    {
        /// <summary>
        /// Creates view for <see cref="NodeModel" />
        /// </summary>
        /// <param name="node">Node Model</param>
        /// <param name="edgeConnectorListener">Listener to use for ports</param>
        BaseNodeView CreateNodeView( NodeModel node, IEdgeConnectorListener edgeConnectorListener );

        /// <summary>
        /// Creates view for <see cref="ConnectionModel"/>
        /// </summary>
        /// <param name="connection">Connection Model</param>
        BaseConnectionView CreateConnectionView( ConnectionModel connection );
    }
}