using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base class for <see cref="ConnectionModel"/> view
    /// </summary>
    public class BaseConnectionView : Edge
    {
        private ConnectionModel _connection;

        /// <summary>
        /// Associated <see cref="ConnectionModel"/>
        /// </summary>
        public ConnectionModel Connection
        {
            get => _connection;
            set => this.SetOnce( ref _connection, value );
        }
                
        public BaseConnectionView()
            : this( null )
        {
            // we need this constructor for EdgeConnector<T> to work
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection"><see cref="ConnectionModel"/> for this view</param>
        public BaseConnectionView( ConnectionModel connection )
        {
            Connection = connection;
        }
    }
}
