using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base class for asset importer for a <see cref="IGraphType"/>
    /// </summary>
    public abstract class BaseGraphAssetImporter : ScriptedImporter
    {
        /// <summary>
        /// File extension to import
        /// </summary>
        public abstract string Extension { get; }

        /// <summary>
        /// Type of window to use as editor
        /// </summary>
        public abstract Type EditorWindowType { get; }

        public override void OnImportAsset( AssetImportContext ctx )
        {
            var textGraph = File.ReadAllText( ctx.assetPath, Encoding.UTF8 );

            var graph = GraphSerializer.Deserialize( textGraph );

            var graphAsset = GraphAsset.Create( graph );

            ctx.AddObjectToAsset("MainAsset", graphAsset);
            ctx.SetMainObject( graphAsset );
        }

        /// <summary>
        /// Creates new graph asset using Unity's usual workflow
        /// </summary>
        /// <param name="graph">Graph to store in a new asset</param>
        /// <param name="defaultName">Default name to show in UI</param>
        /// <param name="icon">Icon to show on asset file</param>
        /// <param name="resourceFile"></param>
        public static void CreateNewGraphAsset(GraphModel graph, string defaultName, Texture2D icon = null, string resourceFile = null)
        {
            var graphCreateAction = ScriptableObject.CreateInstance<NewGraphAction>();
            graphCreateAction.Graph = graph;

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists( 0, graphCreateAction, defaultName, icon, resourceFile );
        }

        class NewGraphAction : EndNameEditAction
        {
            public GraphModel Graph { get; internal set; }

            public override void Action( int instanceId, string pathName, string resourceFile )
            {
                File.WriteAllText( pathName, GraphSerializer.Serialize( Graph ) );
                AssetDatabase.Refresh();

                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<GraphAsset>( pathName );
                Selection.activeObject = obj;
            }
        }
    }
}
