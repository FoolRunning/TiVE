using System;
using Microsoft.CSharp.RuntimeBinder;
using NLua.Exceptions;
using OpenTK;
using OpenTK.Input;
using ProdigalSoftware.TiVE.Renderer;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

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

            dynamic keyTable = gameScript.NewTable("Key");
            keyTable.Unknown = Key.Unknown;
            keyTable.LShift = Key.LShift;
            keyTable.ShiftLeft = Key.ShiftLeft;
            keyTable.RShift = Key.RShift;
            keyTable.ShiftRight = Key.ShiftRight;
            keyTable.ControlLeft = Key.ControlLeft;
            keyTable.LControl = Key.LControl;
            keyTable.ControlRight = Key.ControlRight;
            keyTable.RControl = Key.RControl;
            keyTable.AltLeft = Key.AltLeft;
            keyTable.LAlt = Key.LAlt;
            keyTable.AltRight = Key.AltRight;
            keyTable.RAlt = Key.RAlt;
            keyTable.LWin = Key.LWin;
            keyTable.WinLeft = Key.WinLeft;
            keyTable.RWin = Key.RWin;
            keyTable.WinRight = Key.WinRight;
            keyTable.Menu = Key.Menu;
            keyTable.F1 = Key.F1;
            keyTable.F2 = Key.F2;
            keyTable.F3 = Key.F3;
            keyTable.F4 = Key.F4;
            keyTable.F5 = Key.F5;
            keyTable.F6 = Key.F6;
            keyTable.F7 = Key.F7;
            keyTable.F8 = Key.F8;
            keyTable.F9 = Key.F9;
            keyTable.F10 = Key.F10;
            keyTable.F11 = Key.F11;
            keyTable.F12 = Key.F12;
            keyTable.F13 = Key.F13;
            keyTable.F14 = Key.F14;
            keyTable.F15 = Key.F15;
            keyTable.F16 = Key.F16;
            keyTable.F17 = Key.F17;
            keyTable.F18 = Key.F18;
            keyTable.F19 = Key.F19;
            keyTable.F20 = Key.F20;
            keyTable.F21 = Key.F21;
            keyTable.F22 = Key.F22;
            keyTable.F23 = Key.F23;
            keyTable.F24 = Key.F24;
            keyTable.F25 = Key.F25;
            keyTable.F26 = Key.F26;
            keyTable.F27 = Key.F27;
            keyTable.F28 = Key.F28;
            keyTable.F29 = Key.F29;
            keyTable.F30 = Key.F30;
            keyTable.F31 = Key.F31;
            keyTable.F32 = Key.F32;
            keyTable.F33 = Key.F33;
            keyTable.F34 = Key.F34;
            keyTable.F35 = Key.F35;
            keyTable.Up = Key.Up;
            keyTable.Down = Key.Down;
            keyTable.Left = Key.Left;
            keyTable.Right = Key.Right;
            keyTable.Enter = Key.Enter;
            keyTable.Escape = Key.Escape;
            keyTable.Space = Key.Space;
            keyTable.Tab = Key.Tab;
            keyTable.Back = Key.Back;
            keyTable.BackSpace = Key.BackSpace;
            keyTable.Insert = Key.Insert;
            keyTable.Delete = Key.Delete;
            keyTable.PageUp = Key.PageUp;
            keyTable.PageDown = Key.PageDown;
            keyTable.Home = Key.Home;
            keyTable.End = Key.End;
            keyTable.CapsLock = Key.CapsLock;
            keyTable.ScrollLock = Key.ScrollLock;
            keyTable.PrintScreen = Key.PrintScreen;
            keyTable.Pause = Key.Pause;
            keyTable.NumLock = Key.NumLock;
            keyTable.Clear = Key.Clear;
            keyTable.Sleep = Key.Sleep;
            keyTable.Keypad0 = Key.Keypad0;
            keyTable.Keypad1 = Key.Keypad1;
            keyTable.Keypad2 = Key.Keypad2;
            keyTable.Keypad3 = Key.Keypad3;
            keyTable.Keypad4 = Key.Keypad4;
            keyTable.Keypad5 = Key.Keypad5;
            keyTable.Keypad6 = Key.Keypad6;
            keyTable.Keypad7 = Key.Keypad7;
            keyTable.Keypad8 = Key.Keypad8;
            keyTable.Keypad9 = Key.Keypad9;
            keyTable.KeypadDivide = Key.KeypadDivide;
            keyTable.KeypadMultiply = Key.KeypadMultiply;
            keyTable.KeypadMinus = Key.KeypadMinus;
            keyTable.KeypadSubtract = Key.KeypadSubtract;
            keyTable.KeypadAdd = Key.KeypadAdd;
            keyTable.KeypadPlus = Key.KeypadPlus;
            keyTable.KeypadDecimal = Key.KeypadDecimal;
            keyTable.KeypadPeriod = Key.KeypadPeriod;
            keyTable.KeypadEnter = Key.KeypadEnter;
            keyTable.A = Key.A;
            keyTable.B = Key.B;
            keyTable.C = Key.C;
            keyTable.D = Key.D;
            keyTable.E = Key.E;
            keyTable.F = Key.F;
            keyTable.G = Key.G;
            keyTable.H = Key.H;
            keyTable.I = Key.I;
            keyTable.J = Key.J;
            keyTable.K = Key.K;
            keyTable.L = Key.L;
            keyTable.M = Key.M;
            keyTable.N = Key.N;
            keyTable.O = Key.O;
            keyTable.P = Key.P;
            keyTable.Q = Key.Q;
            keyTable.R = Key.R;
            keyTable.S = Key.S;
            keyTable.T = Key.T;
            keyTable.U = Key.U;
            keyTable.V = Key.V;
            keyTable.W = Key.W;
            keyTable.X = Key.X;
            keyTable.Y = Key.Y;
            keyTable.Z = Key.Z;
            keyTable.Number0 = Key.Number0;
            keyTable.Number1 = Key.Number1;
            keyTable.Number2 = Key.Number2;
            keyTable.Number3 = Key.Number3;
            keyTable.Number4 = Key.Number4;
            keyTable.Number5 = Key.Number5;
            keyTable.Number6 = Key.Number6;
            keyTable.Number7 = Key.Number7;
            keyTable.Number8 = Key.Number8;
            keyTable.Number9 = Key.Number9;
            keyTable.Grave = Key.Grave;
            keyTable.Tilde = Key.Tilde;
            keyTable.Minus = Key.Minus;
            keyTable.Plus = Key.Plus;
            keyTable.BracketLeft = Key.BracketLeft;
            keyTable.LBracket = Key.LBracket;
            keyTable.BracketRight = Key.BracketRight;
            keyTable.RBracket = Key.RBracket;
            keyTable.Semicolon = Key.Semicolon;
            keyTable.Quote = Key.Quote;
            keyTable.Comma = Key.Comma;
            keyTable.Period = Key.Period;
            keyTable.Slash = Key.Slash;
            keyTable.BackSlash = Key.BackSlash;
            keyTable.NonUSBackSlash = Key.NonUSBackSlash;
            keyTable.LastKey = Key.LastKey;

            if (gameScript == null)
            {
                ResourceManager.Cleanup();
                Messages.AddError("Failed to find Game script");
                return false;
            }

            gameScript.KeyPressed = new Func<Key, bool>(k => keyboard[k]);
            gameScript.UpdateCamera = new Action(() => camera.Update());
            gameScript.Vector = new Func<float, float, float, Vector3>((x, y, z) => new Vector3(x, y, z));

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

            const float minLightValue = 0.01f; // 0.002f (0.2%) produces the best result as that is less then a single light value's worth
            StaticLightingHelper lightingHelper = new StaticLightingHelper(ResourceManager.GameWorldManager.GameWorld, 10, minLightValue);
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
