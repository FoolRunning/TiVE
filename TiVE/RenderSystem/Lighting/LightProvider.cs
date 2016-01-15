using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem.Voxels;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    #region LightComplexity enum
    internal enum LightComplexity
    {
        Simple,
        Realistic,
        RealisticWithShadows,
        Debug
    }
    #endregion

    internal abstract class LightProvider
    {
        #region Member variables
        private readonly Scene scene;
        private readonly LightingModel lightingModel;
        #endregion

        #region Constructor/singleton getter
        private LightProvider(Scene scene)
        {
            this.scene = scene;
            AmbientLight = Color3f.Empty;
            lightingModel = LightingModel.Get(scene.GameWorld.LightingModelType);
        }

        /// <summary>
        /// Gets a light provider for the specified game world using the current user settings to determine the complexity
        /// </summary>
        public static LightProvider Get(Scene scene)
        {
            switch ((LightComplexity)(int)TiVEController.UserSettings.Get(UserSettings.LightingComplexityKey))
            {
                case LightComplexity.Realistic: return new RealisticLightProvider(scene);
                case LightComplexity.RealisticWithShadows: return new RealisticWithShadowsLightProvider(scene);
                case LightComplexity.Debug: return new DebugLightProvider(scene);
                default: return new SimpleLightProvider(scene);
            }
        }
        #endregion

        #region Properties
        public Color3f AmbientLight { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the light value at the specified voxel
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        public abstract Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ);

        /// <summary>
        /// Gets the light value at the specified voxel. This version is faster if the caller already has the other parameters
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        protected abstract Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides);

        public virtual Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
        {
            Color3f lightColorPercentage = GetLightAt(voxelX, voxelY, voxelZ, voxelSize, worldBlockX, worldBlockY, worldBlockZ, visibleSides);
            byte a = voxel.A;
            byte r = (byte)Math.Min(255, (int)(voxel.R * lightColorPercentage.R));
            byte g = (byte)Math.Min(255, (int)(voxel.G * lightColorPercentage.G));
            byte b = (byte)Math.Min(255, (int)(voxel.B * lightColorPercentage.B));
            return new Color4b(r, g, b, a);
        }
        #endregion

        #region Private helper methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3f GetVoxelNormal(VoxelSides visibleSides)
        {
            if (visibleSides == VoxelSides.All)
                return Vector3f.Zero;

            Vector3f vector = new Vector3f();
            if ((visibleSides & VoxelSides.Left) != 0)
                vector.X -= 1.0f;
            if ((visibleSides & VoxelSides.Bottom) != 0)
                vector.Y -= 1.0f;
            if ((visibleSides & VoxelSides.Back) != 0)
                vector.Z -= 1.0f;

            if ((visibleSides & VoxelSides.Right) != 0)
                vector.X += 1.0f;
            if ((visibleSides & VoxelSides.Top) != 0)
                vector.Y += 1.0f;
            if ((visibleSides & VoxelSides.Front) != 0)
                vector.Z += 1.0f;

            vector.NormalizeFast();
            return vector;
        }
        #endregion

        #region DebugLightProvider class
        private sealed class DebugLightProvider : LightProvider
        {
            private static readonly Color3f[] lightCountColors = 
            {
                new Color3f(0.0f, 0.2f, 0.0f),
                new Color3f(0.0f, 1.0f, 0.0f),
                new Color3f(0.2f, 1.0f, 0.0f),
                new Color3f(0.4f, 1.0f, 0.0f),
                new Color3f(0.6f, 1.0f, 0.0f),
                new Color3f(0.8f, 1.0f, 0.0f),
                new Color3f(1.0f, 1.0f, 0.0f),
                new Color3f(1.0f, 0.9f, 0.0f),
                new Color3f(1.0f, 0.6f, 0.0f),
                new Color3f(1.0f, 0.3f, 0.0f),
                new Color3f(1.0f, 0.0f, 0.0f), // 10
                new Color3f(1.0f, 0.0f, 1.0f),
                new Color3f(1.0f, 0.1f, 1.0f),
                new Color3f(1.0f, 0.2f, 1.0f),
                new Color3f(1.0f, 0.3f, 1.0f),
                new Color3f(1.0f, 0.4f, 1.0f),
                new Color3f(1.0f, 0.5f, 1.0f),
                new Color3f(1.0f, 0.6f, 1.0f),
                new Color3f(1.0f, 0.8f, 1.0f),
                new Color3f(1.0f, 1.0f, 1.0f),
                new Color3f(1.0f, 1.0f, 1.0f) // 20
            };

            public DebugLightProvider(Scene scene) : base(scene)
            {
            }

            public override Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY,
                int worldBlockZ, VoxelSides visibleSides)
            {
                Color3f lightColor = GetLightAt(voxelX, voxelY, voxelZ, voxelSize, worldBlockX, worldBlockY, worldBlockZ, visibleSides);
                return new Color4b(lightColor.R, lightColor.G, lightColor.B, 1.0f);
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                int worldBlockX = voxelX >> Block.VoxelSizeBitShift;
                int worldBlockY = voxelY >> Block.VoxelSizeBitShift;
                int worldBlockZ = voxelZ >> Block.VoxelSizeBitShift;
                return GetLightAt(voxelX, voxelY, voxelZ, 1, worldBlockX, worldBlockY, worldBlockZ, VoxelSides.All);
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                int lightCount = 0;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex > 0)
                        lightCount++;
                }
                return lightCountColors[lightCount];
            }
        }
        #endregion

        #region SimpleLightProvider class
        private sealed class SimpleLightProvider : LightProvider
        {
            public SimpleLightProvider(Scene scene) : base(scene)
            {
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(voxelX >> Block.VoxelSizeBitShift, voxelY >> Block.VoxelSizeBitShift, voxelZ >> Block.VoxelSizeBitShift);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                Color3f color = AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    color += lightInfo.LightColor * lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel);
                }
                return color;
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                bool availableMinusX = (visibleSides & VoxelSides.Left) != 0;
                bool availableMinusY = (visibleSides & VoxelSides.Bottom) != 0;
                bool availableMinusZ = (visibleSides & VoxelSides.Back) != 0;
                bool availablePlusX = (visibleSides & VoxelSides.Right) != 0;
                bool availablePlusY = (visibleSides & VoxelSides.Top) != 0;
                bool availablePlusZ = (visibleSides & VoxelSides.Front) != 0;

                Color3f color = AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];

                    int lx = lightInfo.VoxelLocX;
                    int ly = lightInfo.VoxelLocY;
                    int lz = lightInfo.VoxelLocZ;
                    if ((availableMinusX && lx <= voxelX) || (availableMinusY && ly <= voxelY) || (availableMinusZ && lz <= voxelZ) ||
                        (availablePlusX && lx >= voxelX) || (availablePlusY && ly >= voxelY) || (availablePlusZ && lz >= voxelZ))
                    {
                        color += lightInfo.LightColor * (lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel) * 0.5f);
                    }
                }
                return color;
            }
        }
        #endregion

        #region RealisticLightProvider class
        private sealed class RealisticLightProvider : LightProvider
        {
            public RealisticLightProvider(Scene scene) : base(scene)
            {
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(voxelX >> Block.VoxelSizeBitShift, voxelY >> Block.VoxelSizeBitShift, voxelZ >> Block.VoxelSizeBitShift);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                Color3f color = AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    color += lightInfo.LightColor * lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel);
                }
                return color;
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                Vector3f voxelNormal = GetVoxelNormal(visibleSides);
                Color3f color = AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];

                    int lx = lightInfo.VoxelLocX;
                    int ly = lightInfo.VoxelLocY;
                    int lz = lightInfo.VoxelLocZ;

                    float brightness;
                    if (voxelNormal == Vector3f.Zero)
                        brightness = 1.0f;
                    else
                    {
                        Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                        float dot = voxelNormal.X * surfaceToLight.X + voxelNormal.Y * surfaceToLight.Y + voxelNormal.Z * surfaceToLight.Z;
                        brightness = MathHelper.Clamp(dot / surfaceToLight.LengthFast, 0.0f, 1.0f);
                    }
                    color += lightInfo.LightColor * (brightness * lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel));
                }
                return color;
            }
        }
        #endregion

        #region RealisticWithShadowsLightProvider class
        private sealed class RealisticWithShadowsLightProvider : LightProvider
        {
            public RealisticWithShadowsLightProvider(Scene scene) : base(scene)
            {
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(voxelX >> Block.VoxelSizeBitShift, voxelY >> Block.VoxelSizeBitShift, voxelZ >> Block.VoxelSizeBitShift);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                GameWorld world = scene.GameWorld;
                Color3f color = AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    if (world.NoVoxelInLine(voxelX, voxelY, voxelZ, lightInfo.VoxelLocX, lightInfo.VoxelLocY, lightInfo.VoxelLocZ))
                        color += lightInfo.LightColor * lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel);
                    else
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageShadow(voxelX, voxelY, voxelZ, lightingModel);
                }
                return color;
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, 
                int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the scene while loading the chunk

                bool availableMinusX = (visibleSides & VoxelSides.Left) != 0;
                bool availableMinusY = (visibleSides & VoxelSides.Bottom) != 0;
                bool availableMinusZ = (visibleSides & VoxelSides.Back) != 0;
                bool availablePlusX = (visibleSides & VoxelSides.Right) != 0;
                bool availablePlusY = (visibleSides & VoxelSides.Top) != 0;
                bool availablePlusZ = (visibleSides & VoxelSides.Front) != 0;

                // For thread-safety copy all member variables
                Vector3f voxelNormal = GetVoxelNormal(visibleSides);
                GameWorld world = scene.GameWorld;
                Color3f color = AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    int lx = lightInfo.VoxelLocX;
                    int ly = lightInfo.VoxelLocY;
                    int lz = lightInfo.VoxelLocZ;

                    float lightPercentage;
                    if (voxelNormal == Vector3f.Zero)
                        lightPercentage = 1.0f;
                    else
                    {
                        Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                        float dot = voxelNormal.X * surfaceToLight.X + voxelNormal.Y * surfaceToLight.Y + voxelNormal.Z * surfaceToLight.Z;
                        lightPercentage = MathHelper.Clamp(dot / surfaceToLight.LengthFast, 0.0f, 1.0f);
                    }

                    if (lightPercentage > 0.0f && 
                        ((availableMinusX && world.NoVoxelInLine(voxelX - 1, voxelY, voxelZ, lx, ly, lz)) ||
                        (availableMinusY && world.NoVoxelInLine(voxelX, voxelY - 1, voxelZ, lx, ly, lz)) ||
                        (availableMinusZ && world.NoVoxelInLine(voxelX, voxelY, voxelZ - 1, lx, ly, lz)) ||
                        (availablePlusX && world.NoVoxelInLine(voxelX + voxelSize, voxelY, voxelZ, lx, ly, lz)) ||
                        (availablePlusY && world.NoVoxelInLine(voxelX, voxelY + voxelSize, voxelZ, lx, ly, lz)) ||
                        (availablePlusZ && world.NoVoxelInLine(voxelX, voxelY, voxelZ + voxelSize, lx, ly, lz))))
                    {
                        color += lightInfo.LightColor * (lightPercentage * lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel));
                    }

                    lightPercentage = 1.0f - lightPercentage;
                    if (lightPercentage > 0.0f)
                        color += lightInfo.LightColor * (lightPercentage * lightInfo.GetLightPercentageShadow(voxelX, voxelY, voxelZ, lightingModel));
                }
                return color;
            }
        }
        #endregion
    }
}
