namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface IGraphEditorWindow
    {
        string GraphAssetGuid { get; }

        void Focus();
        void Show();
        void LoadGraph( string guid );
    }
}