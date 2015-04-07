using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, ScriptDataInternal> scripts = new Dictionary<string, ScriptDataInternal>();
        private readonly IKeyboard keyboard;
        private readonly IMouse mouse;
        private bool keepRunning;

        public ScriptSystem(IKeyboard keyboard, IMouse mouse) : base("Scripts")
        {
            this.keyboard = keyboard;
            this.mouse = mouse;
        }

        #region Implementation of TimeSlicedEngineSystem
        public override bool Initialize()
        {
            Messages.Print("Loading scripts...");
            keepRunning = true;

            // Register anything explicitly registered as script-accessible
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                UserData.RegisterAssembly(assembly);

            // Register all public classes in the plugin framework and the utils
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string asmName = assembly.GetName().Name;
                if (asmName == "TiVEPluginFramework" || asmName == "Utils")
                    RegisterTypesIn(assembly);
            }

            string dataDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data");
            List<string> errors = new List<string>();
            
            if (!Directory.Exists(dataDir))
                errors.Add("Could not find Data directory");
            else
            {
                foreach (string file in Directory.EnumerateFiles(dataDir, "*.lua", SearchOption.AllDirectories))
                {
                    Script script = new Script();
                    AddLuaGlobals(script);
                    script.DoFile(file);
                    scripts.Add(Path.GetFileNameWithoutExtension(file), new ScriptDataInternal(script));
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

        protected override bool UpdateInternal(float timeSinceLastUpdate, Scene currentScene)
        {
            keyboard.Update();

            DynValue tsluValue = DynValue.NewNumber(timeSinceLastUpdate);

            foreach (IEntity entity in currentScene.GetEntitiesWithComponent<ScriptComponent>())
            {
                ScriptComponent scriptData = entity.GetComponent<ScriptComponent>();
                ScriptDataInternal scriptDataInternal;
                scripts.TryGetValue(scriptData.ScriptName, out scriptDataInternal);
                if (!scriptData.Loaded)
                {
                    if (scriptDataInternal == null)
                        Messages.AddWarning("Unable to find script " + scriptData.ScriptName + " needed by entity " + entity.Name);
                    else
                    {
                        DynValue entityValue = DynValue.FromObject(scriptDataInternal.Script, entity);
                        scriptDataInternal.UpdateFunctionCached = scriptDataInternal.Script.Globals.Get("update");
                        if (scriptDataInternal.UpdateFunctionCached == null)
                            Messages.AddWarning("Unable to find update(entity, frameTime) method for " + scriptData.ScriptName);

                        DynValue initFunction = scriptDataInternal.Script.Globals.Get("initialize");
                        if (initFunction != null)
                            CallLuaFunction(scriptDataInternal.Script, initFunction, entity, entityValue);
                        else
                            Messages.AddWarning("Unable to find initialize(entity) method for " + scriptData.ScriptName);
                    }
                    scriptData.Loaded = true;
                }

                if (scriptDataInternal != null && scriptDataInternal.UpdateFunctionCached != null)
                {
                    DynValue entityValue = DynValue.FromObject(scriptDataInternal.Script, entity);
                    CallLuaFunction(scriptDataInternal.Script, scriptDataInternal.UpdateFunctionCached, entity, entityValue, tsluValue);
                }
            }

            return keepRunning;
        }
        #endregion

        #region Private helper methods
        private static void RegisterTypesIn(Assembly asm)
        {
            foreach (Type type in asm.ExportedTypes)
            {
                if (!type.IsInterface)
                    UserData.RegisterType(type);

            }
        }

        private static void CallLuaFunction(Script script, DynValue function, IEntity entity, params DynValue[] args)
        {
            CameraComponent cameraData = entity.GetComponent<CameraComponent>();
            if (cameraData != null)
            {
                cameraData.PrevLocation = cameraData.Location;
                cameraData.PrevLookAtLocation = cameraData.LookAtLocation;
            }

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
            lua.Globals["stopRunning"] = new Action(() => keepRunning = false);

            //gameScript.Renderer = new Func<IGameWorldRenderer>(() => renderer);
            //gameScript.Camera = new Func<Camera>(() => renderer.Camera);
            //gameScript.UserSettings = new Func<UserSettings>(() => TiVEController.UserSettings);
            //gameScript.GameWorld = new Func<GameWorld>(() => renderer.GameWorld);
            //gameScript.ReloadLevel = new Action(() => renderer.RefreshLevel());
            //gameScript.EmptyBlock = BlockImpl.Empty;
            //gameScript.BlockAt = new Func<int, int, int, ushort>((blockX, blockY, blockZ) =>
            //{
            //    GameWorld gameWorld = renderer.GameWorld;
            //    if (blockX < 0 || blockX >= gameWorld.BlockSize.X || blockY < 0 || blockY >= gameWorld.BlockSize.Y || blockZ < 0 || blockZ >= gameWorld.BlockSize.Z)
            //        return 0;

            //    return gameWorld[blockX, blockY, blockZ];
            //});

            //gameScript.VoxelAt = new Func<int, int, int, uint>((voxelX, voxelY, voxelZ) =>
            //{
            //    GameWorld gameWorld = renderer.GameWorld;
            //    if (voxelX < 0 || voxelX >= gameWorld.VoxelSize.X || voxelY < 0 || voxelY >= gameWorld.VoxelSize.Y || voxelZ < 0 || voxelZ >= gameWorld.VoxelSize.Z)
            //        return 0;

            //    return gameWorld.GetVoxel(voxelX, voxelY, voxelZ);
            //});

            //gameScript.LoadWorld = new Func<string, GameWorld>(worldName =>
            //{
            //    BlockList blockList;
            //    GameWorld newWorld = GameWorldManager.LoadGameWorld(worldName, out blockList);
            //    if (newWorld == null)
            //        throw new TiVEException("Failed to create game world");
            //    newWorld.Initialize(blockList);
            //    renderer.SetGameWorld(blockList, newWorld);
            //    return newWorld;
            //});
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
        #endregion

        #region ScriptDataInternal class
        private sealed class ScriptDataInternal
        {
            public readonly Script Script;
            public DynValue UpdateFunctionCached;

            public ScriptDataInternal(Script script)
            {
                Script = script;
            }
        }
        #endregion

        #region LuaGlobalHelper class
        /// <summary>
        /// Helper class to keep from having to create new functions via anonomous delegates for the script functions
        /// </summary>
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
        #endregion
    }
}
