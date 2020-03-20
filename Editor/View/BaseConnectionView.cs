using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseConnectionView : Edge
    {
        private ConnectionModel _connection;

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

        public BaseConnectionView( ConnectionModel connection )
        {
            Connection = connection;
        }
    }
}
