using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Resources
{
    internal static class ResourceManager
    {
        public static PluginManager PluginManager { get; private set; }
        public static BlockListManager BlockListManager { get; private set; }
        public static WorldChunkManager ChunkManager { get; private set; }
        public static GameWorldManager GameWorldManager { get; private set; }
        public static ShaderManager ShaderManager { get; private set; }

        public static bool Initialize()
        {
            PluginManager = new PluginManager();
            if (!PluginManager.LoadPlugins())
                return false;

            BlockListManager = new BlockListManager();
            if (!BlockListManager.Initialize())
                return false;

            GameWorldManager = new GameWorldManager();

            ShaderManager = new ShaderManager();
            if (!ShaderManager.Initialize())
                return false;

            ChunkManager = new WorldChunkManager();
            if (!ChunkManager.Initialize())
                return false;

            return true;
        }

        public static void Cleanup()
        {
            Messages.Print("Deleting resources...");

            if (ShaderManager != null)
                ShaderManager.Dispose();

            if (BlockListManager != null)
                BlockListManager.Dispose();

            if (ChunkManager != null)
                ChunkManager.Dispose();

            PluginManager = null;
            BlockListManager = null;
            GameWorldManager = null;

            Messages.AddDoneText();
        }
    }
}
