using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Base class for <see cref="NodeModel"/> view
    /// </summary>
    public class BaseNodeView : Node
    {
        private readonly List<BasePortView> _ports = new List<BasePortView>();
        private VisualElement _propertiesExpander;
        private VisualElement _propertiesView;
        private VisualElement _propertiesToggle;

        /// <summary>
        /// Associated node
        /// </summary>
        public NodeModel Node { get; }

        /// <summary>
        /// Listener used to create new connections.
        /// Needed for <see cref="UnityEditor.Experimental.GraphView"/> to work properly
        /// </summary>
        public IEdgeConnectorListener EdgeConnectorListener { get; }

        /// <summary>
        /// Collection of <see cref="PortModel"/> views
        /// </summary>
        public IReadOnlyList<BasePortView> Ports => _ports;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node that needs view</param>
        /// <param name="edgeConnectorListener">Edge connector listener for this view</param>
        public BaseNodeView( NodeModel node, IEdgeConnectorListener edgeConnectorListener )
        {
            styleSheets.Add( Resources.Load<StyleSheet>( "Styles/BaseNodeView" ) );

            Node = node;
            EdgeConnectorListener = edgeConnectorListener;

            title = ObjectNames.NicifyVariableName( node.Type.Name );
            base.SetPosition( new Rect( node.Position, Vector2.zero ) );

            foreach( var port in Node.Ports )
            {
                AddPort( port );
            }

            var contents = this.Q( "contents" );

            if( Node.PropertyBlock.GetType() != typeof( PropertyBlock ) )
            {
                _propertiesExpander = new VisualElement() { name = "properties-expander" };
                {
                    var divider = new VisualElement() { name = "divider" };
                    {
                        divider.AddToClassList( "horizontal" );
                    }
                    _propertiesExpander.Add( divider );

                    _propertiesToggle = new VisualElement() { name = "properties-toggle" };
                    {
                        _propertiesToggle.AddManipulator( new Clickable( TogglePropertiesView ) );

                        var icon = new VisualElement() { name = "icon" };
                        _propertiesToggle.Add( icon );
                    }
                    _propertiesExpander.Add( _propertiesToggle );
                }
                contents.Add( _propertiesExpander );
            }

            Node.PortsChanged += OnNodePortsChanged;

            RefreshPorts();
            RefreshExpandedState();
        }

        /// <summary>
        /// Creates a view for additional properties inside <see cref="NodeModel.PropertyBlock"/>.
        /// Override this to create custom view
        /// </summary>
        public virtual VisualElement CreatePropertiesView()
        {
            var res = new ObjectPropertiesField() { PropertyBlock = Node.PropertyBlock };
            res.Changed += NodePropertyBlockChanged;
            return res;
        }

        /// <summary>
        /// Changes position of this node.
        /// Snaps to grid of 16px by default
        /// </summary>
        /// <param name="newPos">New postion of a node</param>
        public override void SetPosition( Rect newPos )
        {
            newPos.x = Mathf.Round( newPos.x / 16 ) * 16;
            newPos.y = Mathf.Round( newPos.y / 16 ) * 16;

            base.SetPosition( newPos );
            Node.Position = newPos.position;
        }

        /// <summary>
        /// Creates a view for <see cref="PortModel"/>.
        /// Override this to create custom view
        /// </summary>
        /// <param name="port"></param>
        protected virtual BasePortView CreatePortView( PortModel port )
        {
            return new BasePortView( port, Orientation.Horizontal, EdgeConnectorListener );
        }
        
        /// <summary>
        /// Called when any property inside <see cref="NodeModel.PropertyBlock"/> is changed
        /// </summary>
        /// <param name="changedField">Editor field that was changed</param>
        /// <param name="fieldName">Name of <see cref="NodeModel.PropertyBlock"/> field that was changed</param>
        protected virtual void NodePropertyBlockChanged( ObjectPropertiesField changedField, string fieldName )
        {
        }

        private void AddPort( PortModel port )
        {
            var target = port.Direction == PortDirection.Input ? inputContainer : outputContainer;
            var portView = CreatePortView( port );

            target.Add( portView );
            _ports.Add( portView );
        }

        private void RemovePort( PortModel port )
        {
            var target = port.Direction == PortDirection.Input ? inputContainer : outputContainer;
            var portView = target.Children().OfType<BasePortView>().First( v => v.Port == port );

            target.Remove( portView );
            _ports.Remove( portView );
        }

        private void TogglePropertiesView()
        {
            if( _propertiesView == null )
            {
                _propertiesView = CreatePropertiesView();
                _propertiesView.name = "properties-view";
                _propertiesToggle.AddToClassList( "expanded" );
                extensionContainer.Insert(0, _propertiesView );
                BringToFront();
            }
            else
            {
                extensionContainer.Remove( _propertiesView );
                _propertiesToggle.RemoveFromClassList( "expanded" );
                _propertiesView = null;
            }

            RefreshExpandedState();
        }

        private void OnNodePortsChanged( NodeModel.NodePortsChangedArgs args )
        {
            if( args.IsAdded )
            {
                AddPort( args.Port );
            }
            else
            {
                RemovePort( args.Port );
            }
        }
    }
}
