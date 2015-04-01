using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Reflection;
using NLua.Exceptions;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVE.ScriptSystem
{
    internal sealed class ScriptSystem : TimeSlicedEngineSystem
    {
        private readonly Dictionary<string, dynamic> scripts = new Dictionary<string, dynamic>();

        public ScriptSystem() : base("Script System", 60)
        {
        }

        public override bool Initialize()
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

        public override void Dispose()
        {
            foreach (DynamicLua.DynamicLua script in scripts.Values)
                script.Dispose();
        }

        protected override void Update(float timeSinceLastUpdate, Scene currentScene)
        {
            foreach (IEntity entity in currentScene.GetEntitiesWithComponent<ScriptComponent>())
            {
                ScriptComponent scriptData = entity.GetComponent<ScriptComponent>();
                dynamic dynamicEntity = entity;
                if (!scriptData.Loaded)
                {
                    scriptData.Script = GetScript(scriptData.ScriptName);
                    if (scriptData.Script == null)
                        Messages.AddWarning("Unable to find script " + scriptData.ScriptName + " needed by entity " + entity.Name);
                    else
                        scriptData.Script.initialize(dynamicEntity);
                    scriptData.Loaded = true;
                }

                if (scriptData.Script != null)
                    scriptData.Script.update(dynamicEntity, timeSinceLastUpdate);
            }
        }

        public dynamic GetScript(string name)
        {
            dynamic script;
            scripts.TryGetValue(name, out script);
            return script;
        }

        public static void AddLuaTableForEnum<T>(dynamic luaScript)
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("Type parameter must be for an Enum type");

            string tableName = enumType.Name;
            dynamic keyTable = luaScript.NewTable(tableName);

            foreach (string enumProperty in Enum.GetNames(enumType))
                ((DynamicObject)keyTable).TrySetMember(new SetPropertyBinder(enumProperty), Enum.Parse(enumType, enumProperty));
        }

        private static void AddGlobalLuaMethods(dynamic lua)
        {
            lua.Log = new Action<object>(obj => Messages.Println(obj.ToString(), Color.CadetBlue));
            lua.BlockSize = Block.VoxelSize;
            lua.PI = (float)Math.PI;
            lua.Max = new Func<float, float, float>(Math.Max);
            lua.Min = new Func<float, float, float>(Math.Min);
            lua.ToRad = new Func<float, float>(a => a * (float)Math.PI / 180.0f);
            lua.Sin = new Func<float, float>(a => (float)Math.Sin(a));
            lua.Cos = new Func<float, float>(a => (float)Math.Cos(a));
            lua.Tan = new Func<float, float>(a => (float)Math.Tan(a));
        }

        private sealed class SetPropertyBinder : SetMemberBinder
        {
            public SetPropertyBinder(string name) : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            {
                throw new NotImplementedException();
            }
        }
    }
}
