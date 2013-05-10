using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Plugins
{
    internal static class PluginManager
    {
        public const string PluginDir = "Plugins";

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

            string pluginPath = Path.Combine(path, PluginDir);
            if (!Directory.Exists(pluginPath))
            {
                Messages.AddFailText();
                Messages.AddWarning(PluginDir + " directory was not found.");
                return;
            }

            pluginInterfaceMap.Clear();
            string[] pluginFiles = Directory.GetFiles(pluginPath, "*.tive", SearchOption.AllDirectories);

            bool foundPlugins = false;
            List<string> warnings = new List<string>();
            foreach (Assembly asm in pluginFiles.Select(Assembly.LoadFile))
            {
                try
                {
                    foreach (Type pluginType in asm.GetExportedTypes())
                    {
                        if (!pluginType.IsAbstract && pluginType.IsClass)
                        {
                            foreach (Type pluginInterface in pluginType.GetInterfaces().Where(pi => pi.FullName.Contains("ProdigalSoftware.TiVEPluginFramework")))
                            {
                                if (pluginType.GetConstructor(Type.EmptyTypes) == null)
                                {
                                    warnings.Add(pluginType.Name + " does not contain a default constructor.");
                                    break;
                                }
                                foundPlugins = true;
                                List<Type> typesFound;
                                if (!pluginInterfaceMap.TryGetValue(pluginInterface, out typesFound))
                                    pluginInterfaceMap[pluginInterface] = typesFound = new List<Type>();
                                typesFound.Add(pluginType);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    warnings.Add("Failed to load " + asm.FullName);
                    warnings.Add(e.ToString());
                }
            }

            if (foundPlugins)
                Messages.AddDoneText();
            else
                Messages.AddFailText();

            foreach (string warning in warnings)
                Messages.AddWarning(warning);
        }

        public static IEnumerable<T> GetPluginsOfType<T>() where T : class
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
