using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using NLua.Exceptions;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Scripts
{
    public sealed class LuaScripts : IDisposable
    {
        private readonly Dictionary<string, dynamic> scripts = new Dictionary<string, dynamic>();

        public bool Initialize()
        {
            Messages.Print("Loading scripts...");

            string dataDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data");
            List<string> errors = new List<string>();
            
            if (!Directory.Exists(dataDir))
                errors.Add("Could not find Data directory");
            else
            {
                foreach (string file in Directory.EnumerateFiles(dataDir, "*.lua", SearchOption.AllDirectories))
                {
                    try
                    {
                        dynamic lua = new DynamicLua.DynamicLua();
                        AddGlobalLuaMethods(lua);
                        ((DynamicLua.DynamicLua)lua).DoFile(file);
                        scripts.Add(Path.GetFileNameWithoutExtension(file), lua);
                    }
                    catch (LuaScriptException e)
                    {
                        errors.Add(e.Message);
                    }
                }
            }

            if (errors.Count == 0)
                Messages.AddDoneText();
            else
            {
                Messages.AddFailText();
                foreach (string error in errors)
                    Messages.AddError(error);
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            foreach (DynamicLua.DynamicLua script in scripts.Values)
                script.Dispose();
        }

        public dynamic GetScript(string name)
        {
            dynamic script;
            scripts.TryGetValue(name, out script);
            return script;
        }

        private static void AddGlobalLuaMethods(dynamic lua)
        {
            lua.Log = new Action<object>(obj => Messages.Println(obj.ToString(), Color.CadetBlue));
            lua.BlockSize = BlockInformation.BlockSize;
            lua.PI = (float)Math.PI;
            lua.Max = new Func<float, float, float>(Math.Max);
            lua.Min = new Func<float, float, float>(Math.Min);
            lua.Sin = new Func<float, float>(v => (float)Math.Sin(v));
            lua.Cos = new Func<float, float>(v => (float)Math.Cos(v));
            lua.Tan = new Func<float, float>(v => (float)Math.Tan(v));
        }
    }
}
