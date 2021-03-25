using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface IGraphSettings
    {
        object CustomSettingsHolder { get; set; }

        string TestSettings { get; set; }
    }
}
