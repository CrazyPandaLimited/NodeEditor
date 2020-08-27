namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Type of connection. Offers logic of how data is transferred through a connection
    /// </summary>
    public interface IConnectionType
    {
        /// <summary>
        /// Init <see cref="ConnectionModel"/> during its construction
        /// </summary>
        /// <param name="connection">Connection to init</param>
        void InitModel( ConnectionModel connection );

        /// <summary>
        /// Init <see cref="ConnectionModel"/> after it was loaded from serialized data
        /// </summary>
        /// <param name="connection">Connection to init</param>
        void PostLoad( ConnectionModel connection );
    }
}