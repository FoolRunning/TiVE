using System;
using System.Diagnostics;
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
using ProdigalSoftware.Utils;

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

        private readonly MostRecentlyUsedCache<string, string> scriptCodeCache = new MostRecentlyUsedCache<string, string>(10);
        private readonly IKeyboard keyboard;
        private readonly IMouse mouse;
        private DynValue keysEnum;
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
            Messages.Print("Initializing script system...");
            keepRunning = true;

            Script.DefaultOptions.ScriptLoader = new ScriptLoader();
            UserData.DefaultAccessMode = InteropAccessMode.Preoptimized;

            Stopwatch sw = Stopwatch.StartNew();
            
            UserData.RegisterType<Keys>();

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
            sw.Stop();

            keysEnum = UserData.CreateStatic<Keys>();

            Messages.AddDoneText();
            Messages.AddDebug("Object registration took " + sw.ElapsedMilliseconds + "ms");
            return true;
        }

        public override void Dispose()
        {
        }

        public override void ChangeScene(Scene oldScene, Scene newScene)
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
                if (!scriptData.Loaded)
                {
                    scriptData.Script = CreateScript(scriptData.ScriptName);
                    if (scriptData.Script == null)
                        Messages.AddWarning("Unable to find script " + scriptData.ScriptName + " needed by entity " + entity.Name);
                    else
                    {
                        DynValue entityValue = DynValue.FromObject(scriptData.Script, entity);
                        scriptData.UpdateFunctionCached = scriptData.Script.Globals.Get("update");
                        if (scriptData.UpdateFunctionCached == null || Equals(scriptData.UpdateFunctionCached, DynValue.Nil))
                            Messages.AddWarning("Unable to find update(entity, frameTime) method for " + scriptData.ScriptName);

                        //Stopwatch sw = Stopwatch.StartNew();
                        DynValue initFunction = scriptData.Script.Globals.Get("initialize");
                        if (initFunction != null && !Equals(initFunction, DynValue.Nil))
                            CallLuaFunction(scriptData.Script, initFunction, entity, entityValue);
                        else
                            Messages.AddWarning("Unable to find initialize(entity) method for " + scriptData.ScriptName);
                        //sw.Stop();
                        //Messages.AddDebug("Initialization for script " + scriptData.ScriptName + " took " + sw.ElapsedMilliseconds + "ms");
                    }
                    scriptData.Loaded = true;
                }

                if (scriptData.Script != null && scriptData.UpdateFunctionCached != null)
                {
                    DynValue entityValue = DynValue.FromObject(scriptData.Script, entity);
                    CallLuaFunction(scriptData.Script, scriptData.UpdateFunctionCached, entity, entityValue, tsluValue);
                }
            }

            return keepRunning;
        }
        #endregion

        #region Script-accessible methods
        private static Block GetBlock(string name)
        {
            return Factory.Get<Block>(name);
        }

        private Block BlockAt(int blockX, int blockY, int blockZ)
        {
            Scene scene = currentScene; // For thread-safety
            if (scene == null)
                return Block.Empty;

            GameWorld gameWorld = scene.GameWorldInternal; // For thread-safety
            return gameWorld != null ? gameWorld[blockX, blockY, blockZ] : Block.Empty;
        }

        private Block BlockAtVoxel(int voxelX, int voxelY, int voxelZ)
        {
            Scene scene = currentScene; // For thread-safety
            if (scene == null)
                return Block.Empty;

            GameWorld gameWorld = scene.GameWorldInternal; // For thread-safety
            if (gameWorld == null)
                return Block.Empty;

            int blockX = voxelX >> Block.VoxelSizeBitShift;
            int blockY = voxelY >> Block.VoxelSizeBitShift;
            int blockZ = voxelZ >> Block.VoxelSizeBitShift;
            return gameWorld[blockX, blockY, blockZ];
        }

        private static Color3f Color(float r, float g, float b)
        {
            return new Color3f(r, g, b);
        }

        private static object GetSetting(string name)
        {
            return TiVEController.UserSettings.Get(name).RawValue;
        }

        private bool IsKeyPressed(Keys key)
        {
            return keyboard.IsKeyPressed(key);
        }

        private Vector2i MouseLocation()
        {
            return mouse.Location;
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

        private static Vector3f RotateVectorZ(Vector3f vector, float angle)
        {
            return new Vector3f(vector.X * (float)Math.Cos(angle) - vector.Y * (float)Math.Sin(angle), 
                vector.X * (float)Math.Sin(angle) + vector.Y * (float)Math.Cos(angle), vector.Z);
        }

        private static Vector3f RotateVectorX(Vector3f vector, float angle)
        {
            return new Vector3f(vector.X, vector.Y * (float)Math.Cos(angle) - vector.Z * (float)Math.Sin(angle),
                vector.Y * (float)Math.Sin(angle) + vector.Z * (float)Math.Cos(angle));
        }

        private Voxel VoxelAt(int voxelX, int voxelY, int voxelZ)
        {
            Scene scene = currentScene; // For thread-safety
            if (scene == null)
                return Voxel.Empty;

            GameWorld gameWorld = scene.GameWorldInternal; // For thread-safety
            if (gameWorld == null)
                return Voxel.Empty;

            Vector3i voxelSize = gameWorld.VoxelSize;
            if (voxelX < 0 || voxelX >= voxelSize.X ||
                voxelY < 0 || voxelY >= voxelSize.Y ||
                voxelZ < 0 || voxelZ >= voxelSize.Z)
            {
                return Voxel.Empty;
            }

            return gameWorld.GetVoxel(voxelX, voxelY, voxelZ);
        }

        private IGameWorld GetGameWorld()
        {
            Scene scene = currentScene; // For thread-safety
            return scene != null ? scene.GameWorld : null;
        }

        private IScene GetScene()
        {
            return currentScene;
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

        private Script CreateScript(string scriptName)
        {
            string scriptCode = scriptCodeCache.GetFromCache(scriptName, name =>
            {
                string scriptPath = Path.Combine(ScriptsDirName, scriptName + ScriptFileExtension);
                if (!TiVEController.ResourceLoader.FileExists(scriptPath))
                    return null;

                using (Stream stream = TiVEController.ResourceLoader.OpenFile(scriptPath))
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            });

            if (scriptCode == null)
                return null;

            Script script = new Script(AllowedModules);
            AddLuaGlobals(script);

            script.DoString(scriptCode);
            return script;
        }

        private void AddLuaGlobals(Script lua)
        {
            lua.Globals["Keys"] = keysEnum;

            lua.Globals["BlockSize"] = Block.VoxelSize;
            lua.Globals["EmptyVoxel"] = Voxel.Empty;
            lua.Globals["EmptyBlock"] = Block.Empty;

            lua.Globals["block"] = (Func<string, Block>)GetBlock;
            lua.Globals["blockAt"] = (Func<int, int, int, Block>)BlockAt;
            lua.Globals["blockAtVoxel"] = (Func<int, int, int, Block>)BlockAtVoxel;
            lua.Globals["voxelAt"] = (Func<int, int, int, Voxel>)VoxelAt;
            lua.Globals["color"] = (Func<float, float, float, Color3f>)Color;
            lua.Globals["message"] = (Action<object>)Message;
            lua.Globals["stopRunning"] = (Action)StopRunning;
            lua.Globals["keyPressed"] = (Func<Keys, bool>)IsKeyPressed;
            lua.Globals["mouseLocation"] = (Func<Vector2i>)MouseLocation;
            lua.Globals["setting"] = (Func<string, object>)GetSetting;
            lua.Globals["vector"] = (Func<float, float, float, Vector3f>)Vector;
            lua.Globals["rotateVectorX"] = (Func<Vector3f, float, Vector3f>)RotateVectorX;
            lua.Globals["rotateVectorZ"] = (Func<Vector3f, float, Vector3f>)RotateVectorZ;
            lua.Globals["gameWorld"] = (Func<IGameWorld>)GetGameWorld;
            lua.Globals["scene"] = (Func<IScene>)GetScene;

            //gameScript.UserSettings = new Func<UserSettings>(() => TiVEController.UserSettings);
            //gameScript.ReloadLevel = new Action(() => renderer.RefreshLevel());

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
        #endregion

        #region ScriptLoader class
        private sealed class ScriptLoader : ScriptLoaderBase
        {
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
        }
        #endregion
    }
}
