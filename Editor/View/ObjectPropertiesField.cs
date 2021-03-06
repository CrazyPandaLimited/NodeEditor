﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public delegate VisualElement EditorCreator( string label, object initialValue, Action<object> setter );

    public class ObjectPropertiesField : VisualElement
    {
        private static Dictionary<Type, EditorCreator> _typeMappings = new Dictionary<Type, EditorCreator>
        {
            { typeof(string), CreateFieldEditor<TextField, string> },
            { typeof(int), CreateFieldEditor<IntegerField, int> },
            { typeof(bool), CreateFieldEditor<Toggle, bool> },
            { typeof(float), CreateFieldEditor<FloatField, float> },
            { typeof(Vector2), CreateFieldEditor<Vector2Field, Vector2> },
            { typeof(Vector3), CreateFieldEditor<Vector3Field, Vector3> },
            { typeof(Vector4), CreateFieldEditor<Vector4Field, Vector4> },
            { typeof(Vector2Int), CreateFieldEditor<Vector2IntField, Vector2Int> },
            { typeof(Vector3Int), CreateFieldEditor<Vector3IntField, Vector3Int> },
            { typeof(Rect), CreateFieldEditor<RectField, Rect> },
            { typeof(Color), CreateFieldEditor<ColorField, Color> },
        };

        private object _propertyBlock;

        public object PropertyBlock
        {
            get => _propertyBlock;
            set
            {
                _propertyBlock = value;
                Rebuild();
            }
        }

        /// <summary>
        /// changed field and field name
        /// </summary>
        public event Action<ObjectPropertiesField, string> Changed;

        public static TEditor AddEditor<TEditor, TValue>( VisualElement parent, string label, TValue value, Action<TValue> setter )
            where TEditor : BaseField<TValue>, new()
        {
            var ret = new TEditor
            {
                label = ObjectNames.NicifyVariableName( label ),
                value = value
            };

            ret.RegisterValueChangedCallback( e => setter( e.newValue ) );
            parent.Add( ret );
            return ret;
        }

        public static VisualElement CreateEditor( Type valueType, string label, object value, Action<object> setter )
        {
            // Type's static constructor may register editor mappings
            // make sure it was called prior to searching for appropriate editor
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( valueType.TypeHandle );

            if( _typeMappings.TryGetValue( valueType, out var action ) )
            {
                return action( label, value, setter );
            }
            else if( valueType.IsEnum )
            {
                var enumValues = Enum.GetNames( valueType ).ToList();
                if( enumValues.Count == 0 )
                    throw new ArgumentException( $"\"{valueType}\" enum can not be without any items!" );

                var defaultEnumValue = value.ToString();
                var popupField = new PopupField<string>( ObjectNames.NicifyVariableName( label ), enumValues, enumValues.Contains( defaultEnumValue ) ? defaultEnumValue : enumValues[ 0 ] );
                popupField.RegisterValueChangedCallback( v => setter( Enum.Parse( valueType, v.newValue ) ) );
                return popupField;
            }
            else if( !valueType.IsValueType )
            {
                var foldout = new Foldout() { text = ObjectNames.NicifyVariableName( label ) };
                {
                    var propertyBlockField = new ObjectPropertiesField() { PropertyBlock = value };
                    propertyBlockField.Changed += (pb, fName) => setter( pb.PropertyBlock );
                    foldout.Add( propertyBlockField );
                }
                return foldout;
            }

            return null;
        }

        public static TEditor CreateFieldEditor<TEditor, TValue>( string label, object value, Action<object> setter )
            where TEditor : BaseField<TValue>, new()
        {
            var ret = new TEditor
            {
                label = ObjectNames.NicifyVariableName( label ),
                value = ( TValue )value,
            };

            ret.RegisterValueChangedCallback( e => setter( e.newValue ) );
            return ret;
        }

        public static void RegisterEditorMapping( Type type, EditorCreator creator )
        {
            if( _typeMappings.ContainsKey( type ) )
                throw new ArgumentException( $"Editor mapping for type {type.Name} is already registered" );

            _typeMappings[ type ] = creator;
        }

        protected virtual VisualElement ProcessField( FieldInfo field )
        {
            if( typeof( IList ).IsAssignableFrom( field.FieldType ) )
            {
                var collectionField = new CollectionField
                {
                    CollectionOwner = _propertyBlock,
                    Field = field,
                };

                collectionField.Changed += _ => Changed?.Invoke( this, field.Name );

                return collectionField;
            }
            else
            {
                return CreateEditor( field.FieldType, field.Name, field.GetValue( _propertyBlock ), v => { field.SetValue( _propertyBlock, v ); Changed?.Invoke( this, field.Name ); } );
            }
        }

        protected virtual void OnAfterRebuild()
        {
        }

        private void Rebuild()
        {
            Clear();

            if( _propertyBlock == null )
                return;

            var fields = _propertyBlock.GetType().GetFields();

            foreach( var field in fields )
            {
                var newChild = ProcessField( field );

                if( newChild != null )
                    Add( newChild );
            }

            OnAfterRebuild();
        }
    }
}
