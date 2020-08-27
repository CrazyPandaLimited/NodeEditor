namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Connection between two ports
    /// </summary>
    public sealed class ConnectionModel
    {
        private PortModel _from;
        private PortModel _to;

        /// <summary>
        /// Type of connection
        /// </summary>
        public IConnectionType Type { get; }

        /// <summary>
        /// Port where connection starts
        /// </summary>
        public PortModel From
        {
            get => _from;
            set => this.SetOnceOrNull( ref _from, value );
        }

        /// <summary>
        /// Port where connection ends
        /// </summary>
        public PortModel To
        {
            get => _to;
            set => this.SetOnceOrNull( ref _to, value );
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of connection</param>
        public ConnectionModel( IConnectionType type )
        {
            Type = type;
            Type.InitModel( this );
        }

        public override string ToString()
        {
            if( From == null || To == null )
                return "Unconnected";

            return $"{From.Node.Type.Name} {From.Id} -> {To.Node.Type.Name} {To.Id}";
        }
    }
}