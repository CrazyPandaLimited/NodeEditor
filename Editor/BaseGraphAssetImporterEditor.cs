using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
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
            foreach( BaseGraphEditorWindow w in Resources.FindObjectsOfTypeAll( graphImporter.EditorWindowType ) )
            {
                if( w.GraphAssetGuid == guid )
                {
                    foundWindow = true;
                    w.Show();
                    w.Focus();
                }
            }

            if( !foundWindow )
            {
                var window = CreateInstance( graphImporter.EditorWindowType ) as BaseGraphEditorWindow;
                window.Show();
                window.LoadGraph( guid );
            }

            return true;
        }
    }
}
