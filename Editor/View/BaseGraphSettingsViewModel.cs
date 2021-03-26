using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class BaseGraphSettingsViewModel
    {
        public IGraphSettings GraphSettings { get; private set; }

        public void SetGraphSettings( IGraphSettings graphSettings )
        {
            this.GraphSettings = graphSettings;
        }

        public virtual void SaveChangesToCustomSettingsHolder()
        {            
        }
    }
}
