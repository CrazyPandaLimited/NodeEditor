using System;
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
    public class PropertyBlockField : VisualElement
    {
        private static Dictionary<Type, Func<string, object, Action<object>, VisualElement>> _typeMappings = new Dictionary<Type, Func<string, object, Action<object>, VisualElement>>
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

        private PropertyBlock _propertyBlock;

        public PropertyBlock PropertyBlock
        {
            get => _propertyBlock;
            set
            {
                _propertyBlock = value;
                Rebuild();
            }
        }

        public static TEditor AddEditor<TEditor, TValue>( VisualElement parent, string label, TValue value, Action<TValue> setter )
            where TEditor : BaseField<TValue>, new()
        {
            var ret = new TEditor();
            ret.label = ObjectNames.NicifyVariableName( label );
            ret.value = value;
            ret.RegisterValueChangedCallback( e => setter( e.newValue ) );
            parent.Add( ret );
            return ret;
        }

        public static VisualElement CreateEditor( Type valueType, string label, object value, Action<object> setter )
        {
            if( _typeMappings.TryGetValue( valueType, out var action ) )
            {
                return action( label, value, setter );
            }
            else if( valueType.IsEnum )
            {
                var popupField = new PopupField<string>( ObjectNames.NicifyVariableName( label ), Enum.GetNames( valueType ).ToList(), value.ToString() );
                popupField.RegisterValueChangedCallback( v => setter( Enum.Parse( valueType, v.newValue ) ) );
                return popupField;
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

        protected virtual VisualElement ProcessField( FieldInfo field )
        {
            if( typeof( IList ).IsAssignableFrom( field.FieldType ) )
            {
                var collectionField = new CollectionField();
                collectionField.PropertyBlock = _propertyBlock;
                collectionField.Field = field;
                return collectionField;
            }
            else
            {
                return CreateEditor( field.FieldType, field.Name, field.GetValue( _propertyBlock ), v => field.SetValue( _propertyBlock, v ) );
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
