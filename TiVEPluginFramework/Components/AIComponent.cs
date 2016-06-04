using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    public sealed class AIComponent : IComponent
    {
        public static readonly Guid ID = new Guid("E73C97A9-C603-42DE-B31B-7AC6FDA7DBA4");

        public AIComponent(BinaryReader reader)
        {
        }

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
        {
            
        }
        #endregion
    }
}
