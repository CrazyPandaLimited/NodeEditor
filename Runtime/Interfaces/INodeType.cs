namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface INodeType
    {
        string Name { get; }

        void InitModel( NodeModel node );
        void PostLoad( NodeModel node );
    }
}