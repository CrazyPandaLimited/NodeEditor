using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    struct SearchWindowResult
    {
        public INodeType NodeType;
        public Vector2 ScreenPosition;
        public PortModel FromPort;
    }

    class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private INodeTypeRegistry _nodeTypeRegistry;
        private Func<SearchWindowResult, bool> _nodeCreationRequested;
        private PortModel _fromPort;

        private List<SearchTreeEntry> _results = new List<SearchTreeEntry>();

        public void Init( INodeTypeRegistry nodeTypeRegistry, Func<SearchWindowResult, bool> nodeCreationRequested, PortModel fromPort )
        {
            _nodeTypeRegistry = nodeTypeRegistry;
            _nodeCreationRequested = nodeCreationRequested;
            _fromPort = fromPort;
        }

        public List<SearchTreeEntry> CreateSearchTree( SearchWindowContext context )
        {
            _results.Clear();

            _results.Add( new SearchTreeGroupEntry( new GUIContent( "Create Node" ) ) );

            foreach( var nodeType in _nodeTypeRegistry.AvailableNodes )
            {
                _results.Add( new SearchTreeEntry( new GUIContent( nodeType.Name ) ) { level = 1, userData = nodeType } );
            }

            return _results;
        }

        public bool OnSelectEntry( SearchTreeEntry treeEntry, SearchWindowContext context )
        {
            var searchResult = new SearchWindowResult
            {
                NodeType = treeEntry.userData as INodeType,
                ScreenPosition = context.screenMousePosition,
                FromPort = _fromPort,
            };

            return _nodeCreationRequested.Invoke( searchResult );
        }
    }
}