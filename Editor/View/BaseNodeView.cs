using System;
using System.Collections.Generic;
using System.Linq;
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

        protected virtual BasePortView CreatePortView( PortModel port )
        {
            return new BasePortView( port, Orientation.Horizontal, EdgeConnectorListener );
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
