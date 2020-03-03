using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public abstract class BaseGraphAssetImporter : ScriptedImporter
    {
        public abstract string Extension { get; }

        public abstract Type EditorWindowType { get; }

        public override void OnImportAsset( AssetImportContext ctx )
        {
            var textGraph = File.ReadAllText( ctx.assetPath, Encoding.UTF8 );

            var graph = GraphSerializer.Deserialize( textGraph );

            var graphAsset = ScriptableObject.CreateInstance<GraphAsset>();
            graphAsset.Graph = graph;

            ctx.AddObjectToAsset("MainAsset", graphAsset);
            ctx.SetMainObject( graphAsset );
        }

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
