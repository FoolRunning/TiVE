﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Plugins
{
    internal sealed class PluginManager
    {
        private static readonly CSharpCodeProvider codeCompiler = new CSharpCodeProvider(); 
        private const string PluginDir = "Plugins";

        private readonly Dictionary<Type, List<Type>> pluginInterfaceMap = new Dictionary<Type, List<Type>>();

        public bool LoadPlugins()
        {
            Messages.Print("Loading plugins...");

            List<string> errorMessages;
            Assembly asm = LoadPlugins(TiVEController.ResourceLoader.Files(PluginDir, "*.cs"), out errorMessages);

            List<string> warnings = new List<string>();
            if (asm == null)
                warnings.AddRange(errorMessages); // Could not compile plugins
            else
            {
                try
                {
                    foreach (Type pluginType in asm.ExportedTypes.Where(t => !t.IsAbstract && t.IsClass))
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
                yield return (T)type.GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        private static Assembly LoadPlugins(IEnumerable<string> codeFiles, out List<string> errorMessages)
        {
            errorMessages = null;

            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("TiVEPluginFramework.dll");
            parameters.ReferencedAssemblies.Add("Utils.dll");
            parameters.CompilerOptions = "/optimize";
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            parameters.IncludeDebugInformation = true;

            List<string> pluginCode = new List<string>();
            foreach (string codeFile in codeFiles)
            {
                using (TextReader reader = new StreamReader(TiVEController.ResourceLoader.OpenFile(codeFile)))
                    pluginCode.Add(reader.ReadToEnd());
            }

            CompilerResults results = codeCompiler.CompileAssemblyFromSource(parameters, pluginCode.ToArray());

            if (results.Errors.HasErrors)
            {
                errorMessages = new List<string>();
                errorMessages.AddRange(results.Errors.Cast<CompilerError>()
                    .Where(error => !error.IsWarning)
                    .Select(error => $"   {error.Line},{error.Column}: {error.ErrorText}"));
                return null;
            }

            return results.CompiledAssembly;
        }
    }
}
