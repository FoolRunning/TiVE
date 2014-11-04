using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    internal sealed class UserSettingsManager
    {
        private readonly Dictionary<string, object> settings = new Dictionary<string, object>();

        public UserSettingsManager()
        {
        }

        public void Load()
        {
            Messages.Print("Loading user settings...");
            
            settings.Add("useSimpleLighting", false);

            Messages.AddDoneText();
        }
    }
}
