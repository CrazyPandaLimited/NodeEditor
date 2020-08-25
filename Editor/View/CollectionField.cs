using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class CollectionField : VisualElement
    {
        private FieldInfo _field;
        private object _propertyBlock;
        private bool _isExpanded;

        private Foldout _foldout;

        public FieldInfo Field
        {
            get => _field;
            set
            {
                _field = value;
                Rebuild();
            }
        }

        public object CollectionOwner
        {
            get => _propertyBlock;
            set
            {
                _propertyBlock = value;
                Rebuild();
            }
        }

        public event Action<CollectionField> Changed;

        public IList List => _field?.GetValue( _propertyBlock ) as IList;

        public CollectionField()
        {
            _foldout = new Foldout();
            _foldout.RegisterValueChangedCallback( e => _isExpanded = e.newValue );

            Add( _foldout );
        }

        private void Rebuild()
        {
            _foldout.Clear();
            _foldout.text = ObjectNames.NicifyVariableName( _field?.Name ?? "" );
            _foldout.value = _isExpanded;

            if( _field == null || _propertyBlock == null )
                return;

            var fieldValue = _field.GetValue( _propertyBlock );
            if( fieldValue == null )
            {
                // deserialized nodes always have not null fields in propertyblock, but newly created nodes may have not initialized properties
                try
                {
                    fieldValue = Activator.CreateInstance( _field.FieldType );
                    _field.SetValue( _propertyBlock, fieldValue );
                }
                catch
                {
                    // if we can't create a value, there is nothing we can do here
                    return;
                }
            }

            var list = fieldValue as IList;
            var itemType = GetEnumerableType( _field.FieldType );

            for( int i = 0; i < list.Count; i++ )
            {
                var item = list[ i ];

                var actualType = itemType;
                if( actualType == typeof( object ) && item != null )
                    actualType = item.GetType();

                int idx = i;
                var editor = ObjectPropertiesField.CreateEditor( actualType, $"[{i}]", item, v => { list[ idx ] = v; Changed?.Invoke( this ); } );

                if( editor != null )
                {
                    editor.style.flexGrow = 1;
                    var horzBox = new VisualElement() { name = "collection-row" };
                    {
                        horzBox.style.flexDirection = FlexDirection.Row;

                        horzBox.Add( editor );

                        var removeButton = new Button( () => RemoveItem( idx ) ) { text = "X" };
                        {
                            removeButton.style.width = 20;
                            removeButton.style.maxHeight = 22;
                            removeButton.style.marginBottom = 0;
                        }
                        horzBox.Add( removeButton );
                    }
                    _foldout.Add( horzBox );
                }
            }

            var addButton = new Button( AddItem ) { text = "Add item" };
            _foldout.Add( addButton );
        }

        private void AddItem()
        {
            var itemType = GetEnumerableType( _field.FieldType );

            if( itemType == typeof( string ) )
                List.Add( "" );
            else
                List.Add( Activator.CreateInstance( itemType ) );

            Changed?.Invoke( this );
            Rebuild();
        }

        private void RemoveItem( int idx )
        {
            List.RemoveAt( idx );

            Changed?.Invoke( this );
            Rebuild();
        }

        private Type GetEnumerableType( Type type )
        {
            var enumerableType = type.GetInterfaces().FirstOrDefault( t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof( IEnumerable<> ) );
            return enumerableType.GetGenericArguments()[ 0 ];
        }
    }
}
