using System;
using System.Collections.Generic;
using UnityEditor;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface INodeTypeRegistry
    {
        IReadOnlyList<INodeType> AvailableNodes { get; }
    }

    public interface IConnectionTypeRegistry
    {
        IConnectionType FindConnectionType( Type from, Type to );
    }

    public interface IGraphType : INodeTypeRegistry, IConnectionTypeRegistry
    {
        string Name { get; }

        void PostLoad( GraphModel graph );
    }

    public interface IExecutableGraphType : IGraphType
    {
        IGraphExecutionResult Execute( GraphModel graph, BuildTarget target );
    }
}