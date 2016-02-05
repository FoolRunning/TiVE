using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVE.ScriptSystem
{
    /// <summary>
    /// Engine system for handling entities that contain a script component
    /// </summary>
    internal sealed class ScriptSystem : TimeSlicedEngineSystem
    {
        private const string ScriptsDirName = "Scripts";
        private const string ScriptFileExtension = ".lua";
        private const CoreModules AllowedModules = CoreModules.Preset_SoftSandbox | CoreModules.LoadMethods;

        private readonly Dictionary<string, ScriptDataInternal> scripts = new Dictionary<string, ScriptDataInternal>();
        private readonly IKeyboard keyboard;
        private readonly IMouse mouse;
        private bool keepRunning;
        private Scene currentScene;

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

            Script.DefaultOptions.ScriptLoader = new ScriptLoader();

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

            Messages.AddDoneText();
            return true;
        }

        private ScriptDataInternal GetScript(string scriptName)
        {
            ScriptDataInternal scriptData;
            if (!scripts.TryGetValue(scriptName, out scriptData))
            {
                Script script = new Script(AllowedModules);
                AddLuaGlobals(script);

                using (Stream stream = TiVEController.ResourceLoader.OpenFile(Path.Combine(ScriptsDirName, scriptName + ScriptFileExtension)))
                using (StreamReader reader = new StreamReader(stream))
                    script.DoString(reader.ReadToEnd()); // Can't use the DoStream() method because it calls Seek() on the stream which isn't supported by zip file entry streams

                scriptData = new ScriptDataInternal(script);
                scripts.Add(scriptName, scriptData);
                Messages.AddDebug("Loaded script " + scriptName);
            }
            return scriptData;
        }

        public override void Dispose()
        {
            scripts.Clear();
        }

        public override void ChangeScene(Scene newScene)
        {
        }

        protected override bool UpdateInternal(float timeSinceLastUpdate, Scene newCurrentScene)
        {
            currentScene = newCurrentScene;
            keyboard.Update();

            DynValue tsluValue = DynValue.NewNumber(timeSinceLastUpdate);

            foreach (IEntity entity in currentScene.GetEntitiesWithComponent<ScriptComponent>())
            {
                ScriptComponent scriptData = entity.GetComponent<ScriptComponent>();
                ScriptDataInternal scriptDataInternal = GetScript(scriptData.ScriptName);
                if (!scriptData.Loaded)
                {
                    if (scriptDataInternal == null)
                        Messages.AddWarning("Unable to find script " + scriptData.ScriptName + " needed by entity " + entity.Name);
                    else
                    {
                        DynValue entityValue = DynValue.FromObject(scriptDataInternal.Script, entity);
                        scriptDataInternal.UpdateFunctionCached = scriptDataInternal.Script.Globals.Get("update");
                        if (scriptDataInternal.UpdateFunctionCached == null || Equals(scriptDataInternal.UpdateFunctionCached, DynValue.Nil))
                            Messages.AddWarning("Unable to find update(entity, frameTime) method for " + scriptData.ScriptName);

                        DynValue initFunction = scriptDataInternal.Script.Globals.Get("initialize");
                        if (initFunction != null && !Equals(initFunction, DynValue.Nil))
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
            try
            {
                script.Call(function, args);
            }
            catch (Exception)
            {
                Messages.AddError("Got unknown exception when processing script for " + entity.Name);
                throw;
            }
        }

        private void AddLuaGlobals(Script lua)
        {
            AddLuaTableForEnum<Keys>(lua);
            lua.Globals["BlockSize"] = Block.VoxelSize;
            lua.Globals["EmptyVoxel"] = Voxel.Empty;
            lua.Globals["EmptyBlock"] = BlockImpl.Empty;
            //lua.Globals["Scene"] = currentScene;

            lua.Globals["blockAt"] = (Func<int, int, int, ushort>)BlockAt;
            lua.Globals["blockAtVoxel"] = (Func<int, int, int, ushort>)BlockAtVoxel;
            lua.Globals["color"] = (Func<float, float, float, Color3f>)Color;
            lua.Globals["message"] = (Action<object>)Message;
            lua.Globals["stopRunning"] = (Action)StopRunning;
            lua.Globals["keyPressed"] = (Func<int, bool>)IsKeyPressed;
            lua.Globals["setting"] = (Func<string, object>)GetSetting;
            lua.Globals["vector"] = (Func<float, float, float, Vector3f>)Vector;
            lua.Globals["voxelAt"] = (Func<int, int, int, Voxel>)VoxelAt;

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

        internal static void AddLuaTableForEnum<T>(Script lua)
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

        #region Private helper methods
        private ushort BlockAt(int blockX, int blockY, int blockZ)
        {
            Scene scene = currentScene; // For thread-safety
            if (scene == null || scene.GameWorld == null)
                return 0;

            return scene.GameWorld[blockX, blockY, blockZ];
        }

        private ushort BlockAtVoxel(int voxelX, int voxelY, int voxelZ)
        {
            Scene scene = currentScene; // For thread-safety
            if (scene == null || scene.GameWorld == null)
                return 0;

            int blockX = voxelX >> Block.VoxelSizeBitShift;
            int blockY = voxelY >> Block.VoxelSizeBitShift;
            int blockZ = voxelZ >> Block.VoxelSizeBitShift;
            return scene.GameWorld[blockX, blockY, blockZ];
        }

        private static Color3f Color(float r, float g, float b)
        {
            return new Color3f(r, g, b);
        }

        private static object GetSetting(string name)
        {
            return TiVEController.UserSettings.Get(name).RawValue;
        }

        private bool IsKeyPressed(int key)
        {
            return keyboard.IsKeyPressed((Keys)key);
        }

        private static void Message(object obj)
        {
            Messages.Println(obj.ToString(), System.Drawing.Color.CadetBlue);
        }

        private void StopRunning()
        {
            keepRunning = false;
        }

        private static Vector3f Vector(float x, float y, float z)
        {
            return new Vector3f(x, y, z);
        }

        private Voxel VoxelAt(int voxelX, int voxelY, int voxelZ)
        {
            Scene scene = currentScene; // For thread-safety
            if (scene == null || scene.GameWorld == null)
                return Voxel.Empty;

            Vector3i voxelSize = scene.GameWorld.VoxelSize;
            if (voxelX < 0 || voxelX >= voxelSize.X ||
                voxelY < 0 || voxelY >= voxelSize.Y ||
                voxelZ < 0 || voxelZ >= voxelSize.Z)
            {
                return Voxel.Empty;
            }
                
            return scene.GameWorld.GetVoxel(voxelX, voxelY, voxelZ);
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

        private sealed class ScriptLoader : ScriptLoaderBase
        {
            #region Implementation of ScriptLoaderBase
            public override bool ScriptFileExists(string name)
            {
                return false;
            }

            public override string ResolveModuleName(string modname, Table globalContext)
            {
                return modname;
            }

            public override object LoadFile(string file, Table globalContext)
            {
                using (Stream stream = TiVEController.ResourceLoader.OpenFile(Path.Combine(ScriptsDirName, file)))
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
            #endregion
        }
    }
}
