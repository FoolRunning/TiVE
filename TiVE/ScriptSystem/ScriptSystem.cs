using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using MoonSharp.Interpreter;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.ScriptSystem
{
    /// <summary>
    /// Engine system for handling entities that contain a script component
    /// </summary>
    internal sealed class ScriptSystem : TimeSlicedEngineSystem
    {
        private readonly Dictionary<string, Script> scripts = new Dictionary<string, Script>();
        private readonly IKeyboard keyboard;
        private readonly IMouse mouse;

        public ScriptSystem(IKeyboard keyboard, IMouse mouse) : base("Scripts", 60)
        {
            this.keyboard = keyboard;
            this.mouse = mouse;
        }

        public override bool Initialize()
        {
            Messages.Print("Loading scripts...");

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                UserData.RegisterAssembly(assembly);
            UserData.RegisterType<Vector3f>();
            UserData.RegisterType<Color3f>();

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
                        Script script = new Script();
                        AddLuaGlobals(script);
                        script.DoFile(file);
                        scripts.Add(Path.GetFileNameWithoutExtension(file), script);
                    }
                    catch (InterpreterException e)
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
            scripts.Clear();
        }

        protected override void Update(float timeSinceLastUpdate, Scene currentScene)
        {
            keyboard.Update();

            DynValue tsluValue = DynValue.NewNumber(timeSinceLastUpdate);

            foreach (IEntity entity in currentScene.GetEntitiesWithComponent<ScriptComponent>())
            {
                ScriptComponent scriptData = entity.GetComponent<ScriptComponent>();
                if (!scriptData.Loaded)
                {
                    scripts.TryGetValue(scriptData.ScriptName, out scriptData.Script);
                    if (scriptData.Script == null)
                        Messages.AddWarning("Unable to find script " + scriptData.ScriptName + " needed by entity " + entity.Name);
                    else
                    {
                        DynValue entityValue = DynValue.FromObject(scriptData.Script, entity);
                        scriptData.UpdateFunctionCached = scriptData.Script.Globals.Get("update");
                        if (scriptData.UpdateFunctionCached == null)
                            Messages.AddWarning("Unable to find update method for " + scriptData.ScriptName);
                        
                        DynValue initFunction = scriptData.Script.Globals.Get("initialize");
                        if (initFunction != null)
                            CallLuaFunction(scriptData.Script, initFunction, entity, entityValue);
                        else
                            Messages.AddWarning("Unable to find initialize method for " + scriptData.ScriptName);
                    }
                    scriptData.Loaded = true;
                }

                if (scriptData.Script != null && scriptData.UpdateFunctionCached != null)
                {
                    DynValue entityValue = DynValue.FromObject(scriptData.Script, entity);
                    CallLuaFunction(scriptData.Script, scriptData.UpdateFunctionCached, entity, entityValue, tsluValue);
                }
            }
        }

        public static void AddLuaTableForEnum<T>(Script lua)
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("Type parameter must be for an Enum type");

            string tableName = enumType.Name;
            Table keyTable = new Table(lua);

            foreach (string enumProperty in Enum.GetNames(enumType))
                keyTable[enumProperty] = (int)Enum.Parse(enumType, enumProperty);
            lua.Globals[tableName] = keyTable;
        }

        private static void CallLuaFunction(Script script, DynValue function, IEntity entity, params DynValue[] args)
        {
            try
            {
                script.Call(function, args);
            }
            catch (InterpreterException)
            {
                Messages.AddError("Got exception when processing script for " + entity.Name);
                throw;
            }
        }

        private void AddLuaGlobals(Script lua)
        {
            AddLuaTableForEnum<Keys>(lua);
            lua.Globals["BlockSize"] = Block.VoxelSize;
            lua.Globals["PI"] = (float)Math.PI;

            lua.Globals["print"] = (Action<object>)LuaGlobalHelper.Print;
            lua.Globals["max"] = (Func<float, float, float>)LuaGlobalHelper.Max;
            lua.Globals["min"] = (Func<float, float, float>)LuaGlobalHelper.Min;
            lua.Globals["toRadians"] = (Func<float, float>)LuaGlobalHelper.ToRadians;
            lua.Globals["log"] = (Func<float, float>)LuaGlobalHelper.Log;
            lua.Globals["sin"] = (Func<float, float>)LuaGlobalHelper.Sin;
            lua.Globals["cos"] = (Func<float, float>)LuaGlobalHelper.Cos;
            lua.Globals["tan"] = (Func<float, float>)LuaGlobalHelper.Tan;
            lua.Globals["vector"] = (Func<float, float, float, Vector3f>)LuaGlobalHelper.Vector;
            lua.Globals["color"] = (Func<float, float, float, Color3f>)LuaGlobalHelper.Color;
            lua.Globals["keyPressed"] = new Func<int, bool>(key => keyboard.IsKeyPressed((Keys)key));
            lua.Globals["voxelAt"] = new Func<float, float, float, uint>((x, y, z) => 0);

            //gameScript.Renderer = new Func<IGameWorldRenderer>(() => renderer);
            //gameScript.Camera = new Func<Camera>(() => renderer.Camera);
            //gameScript.UserSettings = new Func<UserSettings>(() => TiVEController.UserSettings);
            //gameScript.GameWorld = new Func<GameWorld>(() => renderer.GameWorld);
            //gameScript.ReloadLevel = new Action(() => renderer.RefreshLevel());
            //gameScript.EmptyBlock = BlockImpl.Empty;
        }

        private static class LuaGlobalHelper
        {
            public static void Print(object obj)
            {
                Messages.Println(obj.ToString(), System.Drawing.Color.CadetBlue);
            }

            public static float Max(float v1, float v2)
            {
                return Math.Max(v1, v2);
            }

            public static float Min(float v1, float v2)
            {
                return Math.Min(v1, v2);
            }

            public static float ToRadians(float angle)
            {
                return angle * (float)Math.PI / 180.0f;
            }

            public static float Log(float v)
            {
                return (float)Math.Log(v);
            }

            public static float Sin(float v)
            {
                return (float)Math.Sin(v);
            }

            public static float Cos(float v)
            {
                return (float)Math.Cos(v);
            }

            public static float Tan(float v)
            {
                return (float)Math.Tan(v);
            }

            public static Vector3f Vector(float x, float y, float z)
            {
                return new Vector3f(x, y, z);
            }

            public static Color3f Color(float r, float g, float b)
            {
                return new Color3f(r, g, b);
            }
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
