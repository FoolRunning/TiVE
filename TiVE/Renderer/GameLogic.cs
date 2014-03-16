using System;
using OpenTK;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class GameLogic
    {
        public const int WorldXSize = 2048;
        public const int WorldYSize = 2048;
        public const int WorldZSize = 8;

        private BlockList blockList;
        private GameWorld world;
        private IGameWorldRenderer renderer;

        private readonly Camera camera = new Camera();

        public void Cleanup()
        {
            if (renderer != null)
                renderer.CleanUp();
        }

        public bool Initialize()
        {
            camera.SetLocation(1263 * BlockInformation.BlockSize, 1747 * BlockInformation.BlockSize, 300);
            camera.FoV = (float)Math.PI / 6;

            blockList = BlockList.CreateBlockList();

            WorldGenerator generator = new WorldGenerator(WorldXSize, WorldYSize, WorldZSize);
            world = generator.CreateWorld(123456789123456789, blockList);

            if (world == null)
                return false;

            renderer = new WorldChunkRenderer(world, blockList);
            return true;
        }

        public void Resize(int width, int height)
        {
            camera.SetViewport(width, height);
        }

        public bool UpdateFrame(double timeSinceLastFrame, KeyboardDevice keyboard)
        {
            if (keyboard[Key.Escape])
                return false;

            Vector3 camLoc = camera.Location;

            float speed = 1;
            if (keyboard[Key.ShiftLeft])
                speed = 5;
            else if (keyboard[Key.ControlLeft])
                speed = 0.2f;
            if (keyboard[Key.A])
                camLoc.X -= speed;
            if (keyboard[Key.D])
                camLoc.X += speed;
            if (keyboard[Key.W])
                camLoc.Y += speed;
            if (keyboard[Key.S])
                camLoc.Y -= speed;

            if (keyboard[Key.KeypadPlus])
                camLoc.Z = Math.Max(camLoc.Z - 2.0f, 4.0f * BlockInformation.BlockSize);
            else if (keyboard[Key.KeypadMinus])
                camLoc.Z = Math.Min(camLoc.Z + 2.0f, 60.0f * BlockInformation.BlockSize);

            camera.SetLocation(camLoc.X, camLoc.Y, camLoc.Z);
            camera.SetLookAtLocation(camLoc.X, camLoc.Y, camLoc.Z - 100);
            camera.Update();
            return true;
        }

        public RenderStatistics Render(double timeSinceLastFrame)
        {
            int drawCount, polygonCount, voxelCount, renderedVoxelCount;

            renderer.Draw(camera, out drawCount, out polygonCount, out voxelCount, out renderedVoxelCount);

            return new RenderStatistics(drawCount, polygonCount, voxelCount, renderedVoxelCount);
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
