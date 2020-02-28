﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public enum PortDirection
    {
        NotSet, Input, Output
    }

    public enum PortCapacity
    {
        NotSet, Single, Multiple
    }

    public sealed class PortModel
    {
        private string _id;

        private Type _type;
        private PortDirection _direction;
        private PortCapacity _capacity;
        private NodeModel _node;
        private object _runtimeValue;

        public string Id
        {
            get => _id;
            set => this.SetOnce( ref _id, value );
        }

        public Type Type
        {
            get => _type;
            set => this.SetOnce( ref _type, value );
        }

        public PortDirection Direction
        {
            get => _direction;
            set => this.SetOnce( ref _direction, value );
        }

        public PortCapacity Capacity
        {
            get => _capacity;
            set => this.SetOnce( ref _capacity, value );
        }

        public NodeModel Node
        {
            get => _node;
            set => this.SetOnce( ref _node, value );
        }

        public ICollection<ConnectionModel> Connections { get; } = new List<ConnectionModel>();
    }
}