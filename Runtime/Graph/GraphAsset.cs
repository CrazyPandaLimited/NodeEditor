using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Graph asset that can be serialized via Unity serialization
    /// </summary>
    public class GraphAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        string _serializedGraph;

        /// <summary>
        /// Contained graph
        /// </summary>
        public GraphModel Graph { get; private set; }

        /// <summary>
        /// Creates <see cref="GraphAsset"/> resources from given graph
        /// </summary>
        /// <param name="graph">Source graph</param>
        public static GraphAsset Create( GraphModel graph )
        {
            var ret = CreateInstance<GraphAsset>();
            ret.Graph = graph;
            return ret;
        }

        public void OnBeforeSerialize()
        {
            if( Graph != null )
            {
                _serializedGraph = GraphSerializer.Serialize( Graph );
            }
        }

        public void OnAfterDeserialize()
        {
            if( !string.IsNullOrEmpty( _serializedGraph ) )
            {
                Graph = GraphSerializer.Deserialize( _serializedGraph );
            }
        }
    }
}
