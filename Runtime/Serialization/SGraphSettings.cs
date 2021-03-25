using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrazyPanda.UnityCore.NodeEditor
{
    [Serializable]
    public class SGraphSettings : IGraphSettings
    {
        public object CustomSettingsHolder { get; set; }

        public string TestSettings { get; set; }

        public SGraphSettings()
        {
        }
    }
}
