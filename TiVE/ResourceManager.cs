using System;
using ProdigalSoftware.TiVE.Plugins;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVE.Scripts;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    internal static class ResourceManager
    {
        public static PluginManager PluginManager { get; private set; }
        public static BlockListManager BlockListManager { get; private set; }
        public static WorldChunkManager ChunkManager { get; private set; }
        public static GameWorldManager GameWorldManager { get; private set; }
        public static ShaderManager ShaderManager { get; private set; }
        public static ParticleSystemManager ParticleManager { get; private set; }
        public static LuaScripts LuaScripts { get; private set; }
        public static ResourceTableDefinitionManager TableDefinitionManager { get; private set; }

        public static bool Initialize()
        {
            GameWorldManager = new GameWorldManager();

            PluginManager = new PluginManager();
            if (!PluginManager.LoadPlugins())
                return false;

            TableDefinitionManager = new ResourceTableDefinitionManager();
            if (!TableDefinitionManager.Initialize())
                return false;

            LuaScripts = new LuaScripts();
            if (!LuaScripts.Initialize())
                return false;

            BlockListManager = new BlockListManager();
            if (!BlockListManager.Initialize())
                return false;

            ShaderManager = new ShaderManager();
            if (!ShaderManager.Initialize())
                return false;

            ChunkManager = new WorldChunkManager();
            if (!ChunkManager.Initialize())
                return false;

            ParticleManager = new ParticleSystemManager();
            if (!ParticleManager.Initialize())
                return false;

            return true;
        }

        public static void Cleanup()
        {
            Messages.Print("Deleting resources...");

            if (ChunkManager != null)
                ChunkManager.Dispose(); // Must happen before block list dispose

            if (ParticleManager != null)
                ParticleManager.Dispose();

            if (ShaderManager != null)
                ShaderManager.Dispose();

            if (BlockListManager != null)
                BlockListManager.Dispose();

            if (TableDefinitionManager != null)
                TableDefinitionManager.Dispose();

            if (LuaScripts != null)
                LuaScripts.Dispose();

            if (PluginManager != null)
                PluginManager.Dispose();

            PluginManager = null;
            BlockListManager = null;
            GameWorldManager = null;
            ChunkManager = null;
            ParticleManager = null;
            TableDefinitionManager = null;
            LuaScripts = null;

            GC.Collect();

            Messages.AddDoneText();
        }
    }
}
