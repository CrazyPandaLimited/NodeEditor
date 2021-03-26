using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class GraphSettingsModel : IGraphSettings
    {

        public string TestSettings { get; set; }
        public string CustomSettingsHolder { get; set; }

        public GraphSettingsModel() 
        { 
        }

        public void OnBeforeSerialization()
        {
            throw new System.NotImplementedException();
        }

        public void OnAfterDeserialization()
        {
            throw new System.NotImplementedException();
        }
    }
}
