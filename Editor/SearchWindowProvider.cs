using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CrazyPanda.UnityCore.NodeEditor
{
    class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private INodeTypeRegistry _nodeTypeRegistry;
        private Func<INodeType, Vector2, bool> _nodeCreationRequested;

        private List<SearchTreeEntry> _results = new List<SearchTreeEntry>();

        public void Init( INodeTypeRegistry nodeTypeRegistry, Func<INodeType, Vector2, bool> nodeCreationRequested )
        {
            _nodeTypeRegistry = nodeTypeRegistry;
            _nodeCreationRequested = nodeCreationRequested;
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
            var nodeType = treeEntry.userData as INodeType;

            return _nodeCreationRequested?.Invoke( nodeType, context.screenMousePosition ) == true;
        }
    }
}