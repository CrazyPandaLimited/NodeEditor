using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface IPort
    {
        string Id { get; }

        Type Type { get; }

        PortCapacity Capacity { get; }

        IEnumerable< IConnection > Connections { get; }
    }
}