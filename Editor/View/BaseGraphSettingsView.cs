using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseGraphSettingsView : VisualElement
    {
        protected const string UxmlLocationPath = "UXML/";
        protected const string StylesLocationPath = "Styles/";
        protected const string UxmlStringProperty = "SettingsStringProperty";

        private SGraph _model;

        public SGraph Model
        {
            get => _model;
            set
            {
                _model = value;
                UpdateControls();
            }
        }

        public BaseGraphSettingsView()
        {
            LoadResources();
        }

        private void LoadResources()
        {            
            var uxmlName = UxmlLocationPath + "BaseGraphSettingsView";
            var vt = Resources.Load<VisualTreeAsset>( uxmlName );
            if( vt != null )
            {
                vt.CloneTree( this );
            }


            var ussNameBase = StylesLocationPath + "BaseGraphSettingsView";
            var ssBase = Resources.Load<StyleSheet>( ussNameBase );
            if( ssBase != null )
            {
                styleSheets.Add( ssBase );
            }

            var ussName = StylesLocationPath + GetType().Name;
            var ss = Resources.Load<StyleSheet>( ussName );
            if( ss != null )
            {
                styleSheets.Add( ss );
            }
        }

        protected virtual void UpdateControls()
        {
            GetPropertiesHolder().Clear();
            //BindProperty("Test value:", ()=> Model.GraphSettings.TestSettings, (newVal)=> { Model.GraphSettings.TestSettings = newVal; } );
        }

        protected VisualElement BindProperty(string propertyName, Func<string> propertyGetter, Action<string> propertySetter)
        {
            if( string.IsNullOrEmpty( propertyName ) ) throw new ArgumentNullException( nameof( propertyName ) );
            if( propertyGetter == null ) throw new ArgumentNullException(nameof( propertyGetter ) );
            if( propertySetter == null ) throw new ArgumentNullException( nameof( propertySetter ) );

            var vt = Resources.Load<VisualTreeAsset>( UxmlLocationPath + UxmlStringProperty );
            
            if( vt != null )
            {
                VisualElement propBlock = vt.CloneTree();
                propBlock.Q<Label>( "property-name" ).text = propertyName;
                var textField = propBlock.Q<TextField>( "property-value" );
                textField.value = propertyGetter();
                textField.RegisterCallback<ChangeEvent<string>>((arg)=> { propertySetter(arg.newValue); } );

                GetPropertiesHolder().Add( propBlock );

                return propBlock;
            }

            return default;
        }

        protected VisualElement GetPropertiesHolder()
        {
            return this.Q<VisualElement>( "properties-holder" );
        }
    }
}
