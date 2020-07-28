namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Handles mapping of types and their names within a graph.
    /// Implement this in your GraphType to control NodeType names.
    /// </summary>
    public interface IGraphTypeResolver
    {
        /// <summary>
        /// Returns instance of specific type
        /// </summary>
        /// <typeparam name="T">Type of object to return</typeparam>
        /// <param name="typeName">Name of type to return. This is not always a name of C# type. It may be arbitrary string</param>
        T GetInstance< T >( string typeName ) where T : class;

        /// <summary>
        /// Returns typeName of given object
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="instance">Object instance</param>
        string GetTypeName<T>( T instance ) where T : class;
    }
}