using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base class for Unity inspector of custom graph asset importer
    /// </summary>
    [CustomEditor( typeof( BaseGraphAssetImporter ), true )]
    public class BaseGraphAssetImporterEditor : ScriptedImporterEditor
    {
        [OnOpenAsset( 0 )]
        public static bool OnOpenAsset( int instanceID, int line )
        {
            var path = AssetDatabase.GetAssetPath( instanceID );
            return ShowGraphEditWindow( path );
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if( GUILayout.Button( "Open Graph Editor" ) )
            {
                AssetImporter importer = target as AssetImporter;
                Debug.Assert( importer != null, "importer != null" );
                ShowGraphEditWindow( importer.assetPath );
            }
        }

        protected static bool ShowGraphEditWindow( string path )
        {
            var importer = AssetImporter.GetAtPath( path );
            if( !(importer is BaseGraphAssetImporter graphImporter) )
                return false;

            var guid = AssetDatabase.AssetPathToGUID( path );

            var foundWindow = false;
            foreach( var w in Resources.FindObjectsOfTypeAll( graphImporter.EditorWindowType ) )
            {
                if (w is IGraphEditorWindow graphWindow)
                {
                    foundWindow = true;
                    graphWindow.Focus();
                    graphWindow.Show();
                }
            }            
            if( !foundWindow )
            {
                //Debug.Log( $"graphImporter.EditorWindowType={graphImporter.EditorWindowType}" );
                var window = CreateInstance( graphImporter.EditorWindowType ) as IGraphEditorWindow;
                window.Show();
                window.LoadGraph( guid );
            }

            return true;
        }
    }
}
