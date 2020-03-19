using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseGraphEditorWindow : EditorWindow
    {
        [SerializeField] private string _graphAssetGuid; // store guid, not path to prevent errors when moving assets

        private GraphModel _graph;
        private BaseGraphEditorView _graphEditorView;

        public string GraphAssetGuid => _graphAssetGuid;

        private void OnEnable()
        {
            _graphEditorView = CreateEditorView();
            _graphEditorView.name = "GraphEditorView";
            _graphEditorView.Window = this;

            _graphEditorView.SaveRequested += SaveGraph;
            _graphEditorView.ShowInProjectRequested += ShowInProject;

            rootVisualElement.Add( _graphEditorView );

            if( _graphAssetGuid != null )
            {
                LoadGraph( _graphAssetGuid );
            }
        }

        public void LoadGraph( string graphAssetGuid )
        {
            _graphAssetGuid = graphAssetGuid;
            _graph = GraphSerializer.DeserializeFromGuid( _graphAssetGuid );
            _graphEditorView.Graph = _graph;

            titleContent = new GUIContent( Path.GetFileNameWithoutExtension( AssetDatabase.GUIDToAssetPath( _graphAssetGuid ) ) );
        }

        public void SaveGraph()
        {
            if( _graphAssetGuid != null && _graph != null )
            {
                var path = AssetDatabase.GUIDToAssetPath( _graphAssetGuid );

                if( !string.IsNullOrEmpty( path ) )
                {
                    File.WriteAllText( path, GraphSerializer.Serialize( _graph ) );
                }
            }
        }

        private void ShowInProject()
        {
            if( _graphAssetGuid != null )
            {
                var path = AssetDatabase.GUIDToAssetPath( _graphAssetGuid );
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path );
                EditorGUIUtility.PingObject( asset );
            }
        }

        protected virtual BaseGraphEditorView CreateEditorView()
        {
            return new BaseGraphEditorView();
        }
    }
}
