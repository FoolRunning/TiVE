using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Resources
{
    internal sealed class PluginManager
    {
        private const string PluginDir = "Plugins";

        private readonly Dictionary<Type, List<Type>> pluginInterfaceMap = new Dictionary<Type, List<Type>>();

        public bool LoadPlugins()
        {
            Messages.Print("Loading plugins...");
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path == null)
            {
                Messages.AddFailText();
                return false;
            }

            string pluginPath = Path.Combine(path, PluginDir);
            string[] pluginFiles = Directory.Exists(pluginPath) ? Directory.GetFiles(pluginPath, "*.tive", SearchOption.AllDirectories) : new string[0];

            List<string> warnings = new List<string>();
            foreach (Assembly asm in pluginFiles.Select(Assembly.LoadFile).Concat(new[] { Assembly.GetEntryAssembly() }))
            {
                try
                {
                    foreach (Type pluginType in asm.GetExportedTypes().Where(t => !t.IsAbstract && t.IsClass))
                    {
                        foreach (Type pluginInterface in pluginType.GetInterfaces().Where(pi => pi.FullName.StartsWith("ProdigalSoftware.TiVEPluginFramework", StringComparison.Ordinal)))
                        {
                            if (pluginType.GetConstructor(Type.EmptyTypes) == null)
                            {
                                warnings.Add(pluginType.Name + " does not contain a default constructor.");
                                break;
                            }
                            List<Type> typesFound;
                            if (!pluginInterfaceMap.TryGetValue(pluginInterface, out typesFound))
                                pluginInterfaceMap[pluginInterface] = typesFound = new List<Type>();
                            typesFound.Add(pluginType);
                        }
                    }
                }
                catch (Exception e)
                {
                    warnings.Add("Failed to load plugins in " + asm.FullName);
                    warnings.Add(e.ToString());
                }
            }

            if (pluginInterfaceMap.Count > 0)
                Messages.AddDoneText();
            else
                Messages.AddFailText();

            foreach (string warning in warnings)
                Messages.AddWarning(warning);

            return pluginInterfaceMap.Count > 0;
        }

        public void Dispose()
        {
            pluginInterfaceMap.Clear();
        }

        public IEnumerable<T> GetPluginsOfType<T>() where T : class
        {
            List<Type> typesFound;
            pluginInterfaceMap.TryGetValue(typeof(T), out typesFound);
            if (typesFound == null || typesFound.Count == 0)
                yield break;

            foreach (Type type in typesFound)
            {
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                    continue;

                T newObject = constructor.Invoke(null) as T;
                if (newObject == null)
                    continue;

                yield return newObject;
            }
        }
    }
}
