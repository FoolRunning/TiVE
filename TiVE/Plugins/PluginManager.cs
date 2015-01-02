using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Plugins
{
    internal sealed class PluginManager
    {
        private const string PluginDir = "Plugins";

        private readonly Dictionary<Type, List<Type>> pluginInterfaceMap = new Dictionary<Type, List<Type>>();

        public bool LoadPlugins()
        {
            Messages.Print("Loading plugins...");
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (path == null)
            {
                Messages.AddFailText();
                return false;
            }

            string pluginPath = Path.Combine(path, PluginDir);
            string[] pluginFiles = Directory.Exists(pluginPath) ? Directory.GetFiles(pluginPath, "*.tive", SearchOption.AllDirectories) : new string[0];

            List<string> warnings = new List<string>();
            foreach (string pluginFilePath in pluginFiles)
            {
                List<string> errorMessages;
                Assembly asm = LoadPlugin(pluginFilePath, out errorMessages);
                if (asm == null)
                {
                    // Could not compile plugin
                    warnings.AddRange(errorMessages);
                    continue;
                }

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
                    warnings.Add("Failed to load plugins in " + Path.GetFileName(pluginFilePath));
                    warnings.Add(e.ToString());
                }
            }

            if (pluginInterfaceMap.Count > 0)
                Messages.AddDoneText();
            else
            {
                Messages.AddFailText();
                Messages.AddError("Could not find plugins");
            }

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

        private static Assembly LoadPlugin(string filePath, out List<string> errorMessages)
        {
            Debug.Assert(File.Exists(filePath));

            errorMessages = null;

            string fileName = Path.GetFileName(filePath);
            
            string pluginCode;
            try
            {
                pluginCode = File.ReadAllText(filePath);
            }
            catch (IOException)
            {
                errorMessages = new List<string> { "Error reading plugin: " + fileName };
                return null;
            }

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("TiVEPluginFramework.dll");
            parameters.ReferencedAssemblies.Add("Utils.dll");
            parameters.CompilerOptions = "/optimize";
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, pluginCode);
            if (results.Errors.HasErrors)
            {
                errorMessages = new List<string>();
                errorMessages.Add(string.Format("File {0}:", fileName));
                foreach (CompilerError error in results.Errors)
                {
                    if (!error.IsWarning)
                        errorMessages.Add(string.Format("   {0},{1}: {2}", error.Line, error.Column, error.ErrorText));
                }
                return null;
            }

            return results.CompiledAssembly;
        }
    }
}
