using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface IGraphSettings
    {
        string CustomSettingsHolder { get; set; }

        string TestSettings { get; set; }

        void OnBeforeSerialization();

        void OnAfterDeserialization();
    }
}
