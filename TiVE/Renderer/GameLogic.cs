using System;
using OpenTK;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal sealed class GameLogic
    {
        public const int WorldXSize = 2000;
        public const int WorldYSize = 2000;
        public const int WorldZSize = 2;

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
            camera.SetLocation(20000, (WorldYSize - 75) * BlockInformation.BlockSize, 300);
            camera.FoV = (float)Math.PI / 6;

            blockList = BlockList.CreateBlockList();

            WorldGenerator generator = new WorldGenerator(WorldXSize, WorldYSize, WorldZSize);
            world = generator.CreateWorld(LongRandom(), blockList);

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

            bool shift = keyboard[Key.ShiftLeft];
            if (keyboard[Key.A])
                camLoc.X -= shift ? 5 : 1;
            if (keyboard[Key.D])
                camLoc.X += shift ? 5 : 1;
            if (keyboard[Key.W])
                camLoc.Y += shift ? 5 : 1;
            if (keyboard[Key.S])
                camLoc.Y -= shift ? 5 : 1;

            if (keyboard[Key.KeypadPlus])
                camLoc.Z = Math.Max(camLoc.Z - 2.0f, 4.0f * BlockInformation.BlockSize);
            else if (keyboard[Key.KeypadMinus])
                camLoc.Z = Math.Min(camLoc.Z + 2.0f, 50.0f * BlockInformation.BlockSize);

            camera.SetLocation(camLoc.X, camLoc.Y, camLoc.Z);
            camera.SetLookAtLocation(camLoc.X, camLoc.Y, camLoc.Z - 100);
            camera.Update();
            return true;
        }

        public RenderStatistics Render(double timeSinceLastFrame)
        {
            int drawCount, polygonCount;

            renderer.Draw(camera, out drawCount, out polygonCount);

            return new RenderStatistics(drawCount, polygonCount);
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

        public RenderStatistics(int drawCount, int polygonCount)
        {
            DrawCount = drawCount;
            PolygonCount = polygonCount;
        }
    }
}
