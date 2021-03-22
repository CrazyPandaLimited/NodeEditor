using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class GraphSettingsView : VisualElement
    {
        private Toggle _toggle;
        private Label _systemId;
        private Label _systemStatus;

        public GraphSettingsViewModel Model { get; private set; }

        public GraphSettingsView()
        {
            LoadResources();

            //_toggle = this.Q< Toggle >( "toggle" );
            //_systemId = this.Q< Label >( "system-name" );
            //_systemStatus = this.Q< Label >( "system-status" );

            //_toggle.RegisterValueChangedCallback( e => ViewModel.IsSelectedForBuild = e.newValue );
            
        }

        private void LoadResources()
        {            
            var uxmlName = "Uxml/" + GetType().Name;
            var vt = Resources.Load<VisualTreeAsset>( uxmlName );
            if( vt != null )
            {
                vt.CloneTree( this );
            }


            var ussName = "Styles/" + GetType().Name;
            var ss = Resources.Load<StyleSheet>( ussName );
            if( ss != null )
            {
                styleSheets.Add( ss );
            }
        }

        public void SetupModel( GraphSettingsViewModel model)
        {
            Model = model;
            UpdateControls();
        }

        private void UpdateControls()
        {
            this.Q< Label >( "test-settings-name" ).text = nameof(Model.GraphSettings.TestSettings);
            var testValueField = this.Q<TextField>( "test-settings-field" );
            testValueField.value = Model.GraphSettings.TestSettings;
            testValueField.UnregisterCallback<ChangeEvent<string>>( OnTestFieldChanged );
            testValueField.RegisterCallback<ChangeEvent<string>>( OnTestFieldChanged );
        }

        private void OnTestFieldChanged( ChangeEvent<string> evt )
        {
            Model.GraphSettings.TestSettings = evt.newValue;
        }

        //protected override void BindProperties()
        //{
        //    Bind( x => x.SystemId, _systemId );
        //    Bind( x => x.SystemName, v => _systemId.tooltip = v );
        //    Bind( x => x.AdditionalStatus, SetStatus );
        //    Bind( x => x.IsSelectedForBuild, _toggle );
        //}

        //private void SetStatus( SystemRepoStatus status )
        //{
        //    if( status != SystemRepoStatus.None )
        //        _systemStatus.text = status.ToString();
        //    else
        //        _systemStatus.text = "";
        //}
    }
}
