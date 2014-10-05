﻿using System;
using OpenTK;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE
{
    internal sealed class GameLogic : IDisposable
    {
        private const int WorldXSize = 500;
        private const int WorldYSize = 500;
        private const int WorldZSize = 17;

        private IGameWorldRenderer renderer;

        private readonly Camera camera = new Camera();

        public void Dispose()
        {
            ResourceManager.Cleanup();
        }

        public bool Initialize()
        {
            if (!ResourceManager.Initialize())
            {
                ResourceManager.Cleanup();
                return false;
            }

            //camera.SetLocation(1263 * BlockInformation.BlockSize, 1747 * BlockInformation.BlockSize, 300);
            camera.SetLocation(250 * BlockInformation.BlockSize, 250 * BlockInformation.BlockSize, 300);
            camera.FoV = (float)Math.PI / 4;

            if (!ResourceManager.GameWorldManager.CreateWorld(WorldXSize, WorldYSize, WorldZSize, LongRandom() /* 123456789123456789*/))
                return false;

            // Calculate static lighting
            ResourceManager.LightManager.CalcualteLightsForGameWorld(ResourceManager.GameWorldManager.GameWorld);

            renderer = new WorldChunkRenderer();
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
                camLoc.Z = Math.Max(camLoc.Z - 3.0f, 2 * BlockInformation.BlockSize);
                //camLoc.Z = Math.Max(camLoc.Z - 3.0f, (int)((WorldZSize + 0.5f) * BlockInformation.BlockSize));
            else if (keyboard[Key.KeypadMinus])
                camLoc.Z = Math.Min(camLoc.Z + 3.0f, 60.0f * BlockInformation.BlockSize);

            camera.SetLocation(camLoc.X, camLoc.Y, camLoc.Z);
            camera.SetLookAtLocation(camLoc.X, camLoc.Y + 150, -20);
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

        public static RenderStatistics operator +(RenderStatistics r1, RenderStatistics r2)
        {
            return new RenderStatistics(r1.DrawCount + r2.DrawCount, r1.PolygonCount + r2.PolygonCount,
                r1.VoxelCount + r2.VoxelCount, r1.RenderedVoxelCount + r2.RenderedVoxelCount);
        }
    }
}
