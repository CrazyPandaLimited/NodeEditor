using System;
using System.Collections.Generic;

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
}