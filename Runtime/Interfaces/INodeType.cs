namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Type of node
    /// </summary>
    public interface INodeType
    {
        /// <summary>
        /// Name of this type
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Init <see cref="NodeModel"/> during its construction
        /// </summary>
        /// <param name="connection">Node to init</param>
        void InitModel( NodeModel node );

        /// <summary>
        /// Init <see cref="NodeModel"/> after it was loaded from serialized data
        /// </summary>
        /// <param name="connection">Node to init</param>
        void PostLoad( NodeModel node );
    }
}