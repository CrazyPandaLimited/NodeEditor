namespace CrazyPanda.UnityCore.NodeEditor
{
    public interface IGraphTypeResolver
    {
        T GetInstance< T >( string typeName ) where T : class;
    }
}