using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor.Tests
{
    class ObjectPropertiesFieldTests
    {
        // VisualElements must be attached to Panel to work correctly
        // So we create a simple window for them
        private EditorWindow _window;

        [SetUp]
        public void SetupWindow()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowPopup();
        }

        [TearDown]
        public void ShutdownWindow()
        {
            _window.Close();
            UnityEngine.Object.DestroyImmediate( _window );
        }

        [TestCaseSource( nameof( _builtinTypesSource ) )]
        public void Should_CreateEditor_ForBuiltinTypes( PropertyBlock block, object newValue )
        {
            var field = new ObjectPropertiesField();
            _window.rootVisualElement.Add( field );
            field.PropertyBlock = block;

            var helper = block as IGenericPropertyBlockHelper;
            var editor = field.Q( null, BaseField<int>.ussClassName );

            Assert.That( editor, Is.Not.Null );
            Assert.That( editor, Is.AssignableTo( helper.EditorType ) );

            helper.SetValueToField( editor, newValue );

            Assert.That( helper.PropertyBlockValue, Is.EqualTo( newValue ) );
        }

        [Test]
        public void Should_CreateEditor_ForCollection()
        {
            var field = new ObjectPropertiesField();
            _window.rootVisualElement.Add( field );
            field.PropertyBlock = new WithProperty<List<string>>();

            var editor = field.Children().First();

            Assert.That( editor, Is.Not.Null );
            Assert.That( editor, Is.InstanceOf<CollectionField>() );
        }

        [Test]
        public void Should_CreateEditor_ForEnum()
        {
            var field = new ObjectPropertiesField();
            _window.rootVisualElement.Add( field );

            var block = new WithProperty<PortCapacity>();
            field.PropertyBlock = block;

            var editor = field.Q( null, BaseField<string>.ussClassName );

            Assert.That( editor, Is.Not.Null );
            Assert.That( editor, Is.AssignableTo( typeof( BaseField<string> ) ) );

            (editor as BaseField<string>).value = PortCapacity.Multiple.ToString();

            Assert.That( block.Property, Is.EqualTo( PortCapacity.Multiple ) );
        }

        private static object[][] _builtinTypesSource = new[]
        {
            new object[] { new WithProperty<string>(), "123" },
            new object[] { new WithProperty<int>(), 123 },
            new object[] { new WithProperty<bool>(), true },
            new object[] { new WithProperty<float>(), 1.5f },
            new object[] { new WithProperty<Vector2>(), Vector2.one },
            new object[] { new WithProperty<Vector3>(), Vector3.one },
            new object[] { new WithProperty<Vector4>(), Vector4.one },
            new object[] { new WithProperty<Vector2Int>(), Vector2Int.one },
            new object[] { new WithProperty<Vector3Int>(), Vector3Int.one },
            new object[] { new WithProperty<Rect>(), new Rect(0, 0, 1, 1) },
            new object[] { new WithProperty<Color>(), Color.white },
        };

        interface IGenericPropertyBlockHelper
        {
            object PropertyBlockValue { get; }
            Type EditorType { get; }

            void SetValueToField( VisualElement editor, object value );
        }

        class WithProperty<T> : PropertyBlock, IGenericPropertyBlockHelper
        {
            public T Property = default;

            public object PropertyBlockValue => Property;
            public Type EditorType => typeof( BaseField<T> );

            public void SetValueToField( VisualElement editor, object value )
            {
                (editor as BaseField<T>).value = ( T )value;
            }

            public override string ToString()
            {
                return $"Property<{typeof( T ).Name}>";
            }
        }
    }
}