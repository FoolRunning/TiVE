using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Plugins
{
    public static class PluginManager
    {
        public const string pluginDir = "Plugins";

        private static readonly Dictionary<Type, List<Type>> pluginInterfaceMap = new Dictionary<Type, List<Type>>();

        public static void LoadPlugins()
        {
            Messages.Print("Loading plugins...");
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path == null)
            {
                Messages.AddFailText();
                return;
            }

            string pluginPath = Path.Combine(path, pluginDir);
            if (!Directory.Exists(pluginPath))
            {
                Messages.AddFailText();
                Messages.AddWarning(pluginDir + " directory was not found.");
                return;
            }

            string[] pluginFiles = Directory.GetFiles(pluginPath, "*.tive", SearchOption.AllDirectories);

            bool foundPlugins = false;
            List<string> warnings = new List<string>();
            foreach (Type pluginType in pluginFiles.Select(Assembly.LoadFile).SelectMany(asm => asm.GetExportedTypes()).Where(pt => !pt.IsAbstract && pt.IsClass))
            {
                foreach (Type pluginInterface in pluginType.GetInterfaces().Where(pi => pi.FullName.Contains("ProdigalSoftware.TiVEPluginFramework")))
                {
                    if (pluginType.GetConstructor(Type.EmptyTypes) == null)
                    {
                        warnings.Add(pluginType + " does not contain a default constructor.");
                        break;
                    }
                    foundPlugins = true;
                    List<Type> typesFound;
                    if (!pluginInterfaceMap.TryGetValue(pluginInterface, out typesFound))
                        pluginInterfaceMap[pluginInterface] = typesFound = new List<Type>();
                    typesFound.Add(pluginType);
                }
            }

            if (foundPlugins)
                Messages.AddDoneText();
            else
                Messages.AddFailText();

            foreach (string warning in warnings)
                Messages.AddWarning(warning);

        }
    }
}
