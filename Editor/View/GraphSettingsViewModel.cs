using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.NodeEditor
{
    public class GraphSettingsViewModel
    {
        public IGraphSettings GraphSettings { get; private set; }

        public void SetGraphSettings( IGraphSettings graphSettings )
        {
            this.GraphSettings = graphSettings;
        }
    }
}
