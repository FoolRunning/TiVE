using System;
using OpenTK;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class GameLogic : IDisposable
    {
        public const int WorldXSize = 1000;
        public const int WorldYSize = 2000;
        public const int WorldZSize = 16;

        private BlockList blockList;
        private GameWorld world;
        private IGameWorldRenderer renderer;

        private readonly Camera camera = new Camera();

        public void Dispose()
        {
            if (renderer != null)
                renderer.Dispose();
        }

        public bool Initialize()
        {
            //camera.SetLocation(1263 * BlockInformation.BlockSize, 1747 * BlockInformation.BlockSize, 300);
            camera.SetLocation(500 * BlockInformation.BlockSize, 500 * BlockInformation.BlockSize, 300);
            camera.FoV = (float)Math.PI / 4;

            blockList = BlockList.CreateBlockList();

            WorldGenerator generator = new WorldGenerator(WorldXSize, WorldYSize, WorldZSize);
            world = generator.CreateWorld(LongRandom() /* 123456789123456789*/, blockList);

            if (world == null)
                return false;

            renderer = new WorldChunkRenderer(world, blockList);
            return true;
        }

        public void Resize(int width, int height)
        {
            camera.SetViewport(width, height);
        }

        public bool UpdateFrame(float timeSinceLastFrame, KeyboardDevice keyboard)
        {
            if (keyboard[Key.Escape])
                return false;

            Vector3 camLoc = camera.Location;

            float speed = 4;
            if (keyboard[Key.ShiftLeft])
                speed = 20;
            else if (keyboard[Key.ControlLeft])
                speed = 0.4f;
            if (keyboard[Key.A])
                camLoc.X -= speed;
            if (keyboard[Key.D])
                camLoc.X += speed;
            if (keyboard[Key.W])
                camLoc.Y += speed;
            if (keyboard[Key.S])
                camLoc.Y -= speed;

            if (keyboard[Key.KeypadPlus])
                camLoc.Z = Math.Max(camLoc.Z - 3.0f, (int)((WorldZSize + 0.2f) * BlockInformation.BlockSize));
            else if (keyboard[Key.KeypadMinus])
                camLoc.Z = Math.Min(camLoc.Z + 3.0f, 60.0f * BlockInformation.BlockSize);

            camera.SetLocation(camLoc.X, camLoc.Y, camLoc.Z);
            camera.SetLookAtLocation(camLoc.X, camLoc.Y, camLoc.Z - 100);
            camera.Update();

            renderer.Update(camera, timeSinceLastFrame);
            return true;
        }

        public RenderStatistics Render(float timeSinceLastFrame)
        {
            RenderStatistics stats;
            renderer.Draw(camera, out stats);
            return stats;
        }

        private static long LongRandom()
        {
            byte[] buf = new byte[8];
            Random random = new Random();
            random.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }
    }

    internal struct RenderStatistics
    {
        public readonly int DrawCount;
        public readonly int PolygonCount;
        public readonly int VoxelCount;
        public readonly int RenderedVoxelCount;

        public RenderStatistics(int drawCount, int polygonCount, int voxelCount, int renderedVoxelCount)
        {
            DrawCount = drawCount;
            PolygonCount = polygonCount;
            VoxelCount = voxelCount;
            RenderedVoxelCount = renderedVoxelCount;
        }
    }
}
