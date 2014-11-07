using System;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE
{
    internal static class ResourceManager
    {
        public static readonly BlockListManager BlockListManager = new BlockListManager();
        public static readonly WorldChunkManager ChunkManager = new WorldChunkManager();
        public static readonly GameWorldManager GameWorldManager = new GameWorldManager();
        public static readonly ShaderManager ShaderManager = new ShaderManager();
        public static readonly ParticleSystemManager ParticleManager = new ParticleSystemManager();

        public static bool Initialize()
        {
            if (!ShaderManager.Initialize())
                return false;

            if (!ChunkManager.Initialize())
                return false;

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

            if (GameWorldManager != null)
                GameWorldManager.Dispose();

            if (BlockListManager != null)
                BlockListManager.Dispose();

            if (ShaderManager != null)
                ShaderManager.Dispose();

            GC.Collect();

            Messages.AddDoneText();
        }
    }
}
