namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface IConnectionType
    {
        void InitModel( ConnectionModel connection );
        void PostLoad( ConnectionModel connection );
    }
}