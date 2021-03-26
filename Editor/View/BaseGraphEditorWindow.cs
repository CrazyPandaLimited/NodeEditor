using System.IO;
using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base class for GraphEditor window
    /// </summary>
    public class BaseGraphEditorWindow<TGraphEditorView, TGraphSettingsView> : 
        EditorWindow, 
        ISerializationCallbackReceiver,
        IGraphEditorWindow
        where TGraphEditorView : BaseGraphEditorView<TGraphSettingsView>, new() 
        where TGraphSettingsView:BaseGraphSettingsView, new() 
    {
        [SerializeField] private string _graphAssetGuid; // store guid, not path to prevent errors when moving assets
        [ SerializeField ]
        private string _serializedGraph = string.Empty;
        
        private SGraph _graph;
        private TGraphEditorView _graphEditorView;

        /// <summary>
        /// Guid of opened asset
        /// </summary>
        public string GraphAssetGuid => _graphAssetGuid;

        private void OnEnable()
        {
            _graphEditorView = CreateEditorView();
            _graphEditorView.name = "GraphEditorView";
            _graphEditorView.Window = this;

            _graphEditorView.SaveRequested += SaveGraph;
            _graphEditorView.ShowInProjectRequested += ShowInProject;

            rootVisualElement.Add( _graphEditorView );

            LoadGraph();
        }

        public void OnBeforeSerialize()
        {
            if( _graph != null )
            {
                _serializedGraph = GraphSerializer.Serialize( _graph );
            }
        }

        public void OnAfterDeserialize()
        {
            
        }
        
        /// <summary>
        /// Loads graph from given asset guid
        /// </summary>
        /// <param name="graphAssetGuid">Guid of a <see cref="GraphAsset"/></param>
        public void LoadGraph( string graphAssetGuid )
        {
            _graphAssetGuid = graphAssetGuid;
            _graph = GraphSerializer.DeserializeSGraphFromGuid( _graphAssetGuid );
            _graphEditorView.Graph = _graph;

            SetWindowTitle();
        }

        /// <summary>
        /// Saves current <see cref="GraphAsset"/>
        /// </summary>
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

        /// <summary>
        /// Creates <see cref="BaseGraphEditorView"/> for editing <see cref="GraphAsset"/>.
        /// Override this to create custom view
        /// </summary>
        protected virtual TGraphEditorView CreateEditorView()
        {
            return new TGraphEditorView();
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

        private void LoadGraph()
        {
            if( !string.IsNullOrEmpty( _serializedGraph ) )
            {
                LoadSerializedGraph();
            }
            else if( _graphAssetGuid != null )
            {
                LoadGraph( _graphAssetGuid );
            }
        }
        
        private void LoadSerializedGraph()
        {
            _graph = GraphSerializer.DeserializeSGraph( _serializedGraph );
           _graphEditorView.Graph = _graph;
           _serializedGraph = string.Empty;

           SetWindowTitle();
        }
        
        private void SetWindowTitle()
        {
            if( !string.IsNullOrEmpty( _graphAssetGuid ) )
            {
                titleContent = new GUIContent( Path.GetFileNameWithoutExtension( AssetDatabase.GUIDToAssetPath( _graphAssetGuid ) ) );
            }
        }
    }
}
