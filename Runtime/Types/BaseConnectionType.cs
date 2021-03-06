﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base implementation of <see cref="IConnectionType"/>
    /// </summary>
    public class BaseConnectionType : IConnectionType
    {
        public virtual void InitModel( ConnectionModel connection )
        {
        }

        public virtual void PostLoad( ConnectionModel connection )
        {
        }
    }
}
