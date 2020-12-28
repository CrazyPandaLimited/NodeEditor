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
        /// Init <see cref="INode"/> during its construction
        /// </summary>
        /// <param name="node">Node to init</param>
        void Init( INode node );

        /// <summary>
        /// Init <see cref="INode"/> after it was loaded from serialized data
        /// </summary>
        /// <param name="node">Node to init</param>
        void PostLoad( INode node );
    }
}