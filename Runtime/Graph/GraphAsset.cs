using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class GraphAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        string _serializedGraph;

        public GraphModel Graph { get; set; }

        public void OnBeforeSerialize()
        {
            if( Graph != null )
            {
                _serializedGraph = SerializationHelper.SerializeGraph( Graph );
            }
        }

        public void OnAfterDeserialize()
        {
            if( !string.IsNullOrEmpty( _serializedGraph ) )
            {
                Graph = SerializationHelper.DeserializeGraph( _serializedGraph );
            }
        }
    }
}
