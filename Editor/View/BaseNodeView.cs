using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseNodeView : Node
    {
        private readonly List<BasePortView> _ports = new List<BasePortView>();
        private VisualElement _propertiesExpander;
        private VisualElement _propertiesView;
        private VisualElement _propertiesToggle;

        public NodeModel Node { get; }
        public IEdgeConnectorListener EdgeConnectorListener { get; }
        public IReadOnlyList<BasePortView> Ports => _ports;

        public BaseNodeView( NodeModel node, IEdgeConnectorListener edgeConnectorListener )
        {
            styleSheets.Add( Resources.Load<StyleSheet>( "Styles/BaseNodeView" ) );

            Node = node;
            EdgeConnectorListener = edgeConnectorListener;

            title = ObjectNames.NicifyVariableName( node.Type.Name );
            base.SetPosition( new Rect( node.Position, Vector2.zero ) );

            foreach( var port in Node.Ports )
            {
                var target = port.Direction == PortDirection.Input ? inputContainer : outputContainer;
                var portView = CreatePort( port );

                portView.SetupEdgeConnector<BaseConnectionView>( EdgeConnectorListener );

                target.Add( portView );
                _ports.Add( portView );
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

            RefreshPorts();
            RefreshExpandedState();
        }

        public virtual VisualElement CreatePropertiesView()
        {
            return new PropertyBlockField() { PropertyBlock = Node.PropertyBlock };
        }

        public override void SetPosition( Rect newPos )
        {
            newPos.x = Mathf.Round( newPos.x / 16 ) * 16;
            newPos.y = Mathf.Round( newPos.y / 16 ) * 16;

            base.SetPosition( newPos );
            Node.Position = newPos.position;
        }

        protected virtual BasePortView CreatePort( PortModel port )
        {
            return new BasePortView( port, Orientation.Horizontal );
        }

        private void TogglePropertiesView()
        {
            if( _propertiesView == null )
            {
                _propertiesView = CreatePropertiesView();
                _propertiesView.name = "properties-view";
                _propertiesToggle.AddToClassList( "expanded" );
                extensionContainer.Add( _propertiesView );
            }
            else
            {
                extensionContainer.Remove( _propertiesView );
                _propertiesToggle.RemoveFromClassList( "expanded" );
                _propertiesView = null;
            }

            RefreshExpandedState();
        }
    }
}
