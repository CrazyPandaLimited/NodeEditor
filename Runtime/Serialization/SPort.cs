using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public sealed class SPort : IPort
    {
        public string Id { get; set; }

        public PortDirection Direction;

        public PortCapacity Capacity { get; set; }

        public string NodeId;

        public Type Type { get; set; }

        public List< SConnection > Connections { get; } = new List< SConnection >();

        IEnumerable< IConnection > IPort.Connections => this.Connections;
        
        public static implicit operator SPort( PortModel port )
        {
            return new SPort
            {
                Id = port.Id,
                Direction = port.Direction,
                Capacity = port.Capacity,
                Type = port.Type,
                NodeId = port?.Node?.Id ?? string.Empty
            };
        }
    }
}