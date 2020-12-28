using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base class for <see cref="ConnectionModel"/> view
    /// </summary>
    public class BaseConnectionView : Edge
    {
        private SConnection _connection;

        /// <summary>
        /// Associated <see cref="SConnection"/>
        /// </summary>
        public SConnection Connection
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
        /// <param name="connection"><see cref="SConnection"/> for this view</param>
        public BaseConnectionView( SConnection connection )
        {
            Connection = connection;
        }
    }
}
