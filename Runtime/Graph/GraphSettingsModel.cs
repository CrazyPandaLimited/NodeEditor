using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class GraphSettingsModel : IGraphSettings
    {

        public string TestSettings { get; set; }
        public object CustomSettingsHolder { get; set; }

        public GraphSettingsModel() 
        { 
        }
    }
}
