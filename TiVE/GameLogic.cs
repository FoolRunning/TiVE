using System;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using NLua.Exceptions;
using OpenTK;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Scripts;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE
{
    internal sealed class GameLogic : IDisposable
    {
        private IGameWorldRenderer renderer;
        private dynamic gameScript;

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

            gameScript = ResourceManager.LuaScripts.GetScript("Game");
            if (gameScript == null)
            {
                ResourceManager.Cleanup();
                Messages.AddError("Failed to find Game script");
                return false;
            }

            LuaScripts.AddLuaTableForEnum<Key>(gameScript);

            gameScript.KeyPressed = new Func<Key, bool>(k => keyboard[k]);
            gameScript.Vector = new Func<float, float, float, Vector3>((x, y, z) => new Vector3(x, y, z));
            gameScript.Color = new Func<float, float, float, Color3f>((r, g, b) => new Color3f(r, g, b));
            gameScript.GameWorld = new Func<GameWorld>(() => ResourceManager.GameWorldManager.GameWorld);
            gameScript.ReloadLevel = new Action(() => ResourceManager.ChunkManager.ReloadAllChunks());

            gameScript.CreateWorld = new Func<int, int, int, IGameWorld>((xSize, ySize, zSize) =>
            {
                if (!ResourceManager.GameWorldManager.CreateWorld(xSize, ySize, zSize, LongRandom() /*123456789123456789*/))
                    throw new TiVEException("Failed to load resources");
                return ResourceManager.GameWorldManager.GameWorld;
            });

            try
            {
                gameScript.Initialize(camera);
            }
            catch (RuntimeBinderException)
            {
                ResourceManager.Cleanup();
                Messages.AddError("Can not find Initialize(camera) function in Game script");
                return false;
            }
            catch (LuaScriptException e)
            {
                ResourceManager.Cleanup();
                Messages.AddStackTrace(e);
                return false;
            }

            // Calculate static lighting

            const float minLightValue = 0.002f; // 0.002f (0.2%) produces the best result as that is less then a single light value's worth
            StaticLightingHelper lightingHelper = new StaticLightingHelper(ResourceManager.GameWorldManager.GameWorld, 50, minLightValue);
            lightingHelper.Calculate();

            renderer = new WorldChunkRenderer();
            return true;
        }

        public void Resize(int width, int height)
        {
            camera.AspectRatio = width / (float)height;
        }

        KeyboardDevice keyboard;
        public bool UpdateFrame(float timeSinceLastFrame, KeyboardDevice keyboard)
        {
            this.keyboard = keyboard;
            if (keyboard[Key.Escape])
                return false;

            try
            {
                gameScript.Update(camera, keyboard);
            }
            catch (RuntimeBinderException)
            {
                Messages.AddError("Can not find Update(camera, keyboard) function in Game script");
                return false;
            }
            catch (LuaScriptException e)
            {
                Messages.AddStackTrace(e);
                return false;
            }

            camera.Update();
            renderer.Update(camera, timeSinceLastFrame);
            return true;
        }

        public RenderStatistics Render(float timeSinceLastFrame)
        {
            return renderer.Draw(camera);
        }

        private static long LongRandom()
        {
            byte[] buf = new byte[8];
            Random random = new Random();
            random.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }
    }
}
