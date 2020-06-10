﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        private IGraphType _graphType;
        private PortModel _fromPort;
        private Func<SearchWindowResult, bool> _nodeCreationRequested;

        private List<SearchTreeEntry> _results = new List<SearchTreeEntry>();

        public void Init( IGraphType graphType, PortModel fromPort, Func< SearchWindowResult, bool > nodeCreationRequested )
        {
            _graphType = graphType;
            _fromPort = fromPort;
            _nodeCreationRequested = nodeCreationRequested;
        }

        public List<SearchTreeEntry> CreateSearchTree( SearchWindowContext context )
        {
            _results.Clear();

            _results.Add( new SearchTreeGroupEntry( new GUIContent( "Create Node" ) ) );

            foreach( var nodeType in _graphType.AvailableNodes )
            {
                // we need to create a node to know what ports it have
                var newNode = nodeType.CreateNode();

                // if we have _fromPort, check that we can connect it to any of the input ports in the newNode
                if( _fromPort == null ||
                    newNode.InputPorts().Any( p => _graphType.FindConnectionType( _fromPort.Type, p.Type ) != null ) )
                {
                    var content = new GUIContent( ObjectNames.NicifyVariableName( nodeType.Name ) );
                    var searchEntry = new SearchTreeEntry( content )
                    {
                        level = 1,
                        userData = nodeType
                    };

                    _results.Add( searchEntry );
                }
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