using System;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    #region LightType enum
    internal enum LightType
    {
        Normal,
        Debug1,
        Debug2
    }
    #endregion

    internal abstract class LightProvider
    {
        #region Member variables
        private readonly Scene scene;
        private readonly LightingModel lightingModel;
        private readonly Vector3f[] voxelSidesNormalCache = new Vector3f[64];
        #endregion

        #region Constructor/singleton getter
        private LightProvider(Scene scene)
        {
            this.scene = scene;
            lightingModel = LightingModel.Get(scene.GameWorld.LightingModelType);
            for (int s = (int)VoxelSides.None; s <= (int)VoxelSides.All; s++)
                voxelSidesNormalCache[s] = GetVoxelNormal((VoxelSides)s);
        }

        /// <summary>
        /// Gets a light provider for the specified game world using the current user settings to determine the complexity
        /// </summary>
        public static LightProvider Create(Scene scene, bool withShadows)
        {
            LightType lightType = (LightType)(int)TiVEController.UserSettings.Get(UserSettings.LightingTypeKey);
            if (lightType == LightType.Debug1)
                return new Debug1LightProvider(scene);
            if (lightType == LightType.Debug2)
                return new Debug2LightProvider(scene);
            if (!withShadows)
                return new NoShadowsLightProvider(scene);
            if ((ShadowAccuracyType)(int)TiVEController.UserSettings.Get(UserSettings.ShadowAccuracyKey) == ShadowAccuracyType.Fast)
                return new WithShadowsLightProviderFast(scene);
            return new WithShadowsLightProviderPerfect(scene);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the light value at the specified voxel
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        public abstract Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel);

        /// <summary>
        /// Gets the light value at the specified voxel. This version is faster if the caller already has the other parameters
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        protected abstract Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel,
            int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides, bool skipVoxelNormalCalc);

        public virtual Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel,
            int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
        {
            Color3f lightColorPercentage = GetLightAt(voxelX, voxelY, voxelZ, detailLevel, shadowDetailLevel, 
                worldBlockX, worldBlockY, worldBlockZ, visibleSides, voxel.SkipVoxelNormalCalc);
            byte a = voxel.A;
            byte r = (byte)Math.Min(255, (int)(voxel.R * lightColorPercentage.R));
            byte g = (byte)Math.Min(255, (int)(voxel.G * lightColorPercentage.G));
            byte b = (byte)Math.Min(255, (int)(voxel.B * lightColorPercentage.B));
            return new Color4b(r, g, b, a);
        }
        #endregion

        #region Private helper methods
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

        #region Debug1LightProvider class
        private sealed class Debug1LightProvider : LightProvider
        {
            private static readonly Color3f[] lightCountColors = 
            {
                new Color3f(0.0f, 0.2f, 0.0f),
                new Color3f(0.0f, 1.0f, 0.0f),
                new Color3f(0.1f, 1.0f, 0.0f),
                new Color3f(0.2f, 1.0f, 0.0f),
                new Color3f(0.3f, 1.0f, 0.0f),
                new Color3f(0.4f, 1.0f, 0.0f),
                new Color3f(0.5f, 1.0f, 0.0f),
                new Color3f(0.6f, 1.0f, 0.0f),
                new Color3f(0.7f, 1.0f, 0.0f),
                new Color3f(0.8f, 1.0f, 0.0f),
                new Color3f(0.9f, 1.0f, 0.0f), // 10
                new Color3f(1.0f, 1.0f, 0.0f),
                new Color3f(1.0f, 0.9f, 0.0f),
                new Color3f(1.0f, 0.8f, 0.0f),
                new Color3f(1.0f, 0.7f, 0.0f),
                new Color3f(1.0f, 0.6f, 0.0f),
                new Color3f(1.0f, 0.5f, 0.0f),
                new Color3f(1.0f, 0.4f, 0.0f),
                new Color3f(1.0f, 0.3f, 0.0f),
                new Color3f(1.0f, 0.15f, 0.0f),
                new Color3f(1.0f, 0.0f, 0.0f), // 20
                new Color3f(1.0f, 0.0f, 1.0f),
                new Color3f(1.0f, 0.1f, 1.0f),
                new Color3f(1.0f, 0.2f, 1.0f),
                new Color3f(1.0f, 0.3f, 1.0f),
                new Color3f(1.0f, 0.4f, 1.0f),
                new Color3f(1.0f, 0.5f, 1.0f),
                new Color3f(1.0f, 0.6f, 1.0f),
                new Color3f(1.0f, 0.7f, 1.0f),
                new Color3f(1.0f, 0.8f, 1.0f),
                new Color3f(1.0f, 1.0f, 1.0f) // 30
            };

            public Debug1LightProvider(Scene scene) : base(scene)
            {
            }

            public override Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel,
                int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                Color3f lightColor = GetLightAt(voxelX, voxelY, voxelZ, detailLevel, shadowDetailLevel, worldBlockX, worldBlockY, worldBlockZ, visibleSides, false);
                return new Color4b(lightColor.R, lightColor.G, lightColor.B, 1.0f);
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel)
            {
                int bitShift = BlockLOD.GetVoxelSizeBitShift(detailLevel);
                int worldBlockX = voxelX >> bitShift;
                int worldBlockY = voxelY >> bitShift;
                int worldBlockZ = voxelZ >> bitShift;
                return GetLightAt(voxelX, voxelY, voxelZ, detailLevel, shadowDetailLevel, worldBlockX, worldBlockY, worldBlockZ, VoxelSides.All, false);
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel, 
                int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides, bool skipVoxelNormalCalc)
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

        #region Debug2LightProvider class
        private sealed class Debug2LightProvider : LightProvider
        {
            public Debug2LightProvider(Scene scene) : base(scene)
            {
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel)
            {
                int bitShift = BlockLOD.GetVoxelSizeBitShift(detailLevel);
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(voxelX >> bitShift, voxelY >> bitShift, voxelZ >> bitShift);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                float lightPercentage = 0.0f;
                LightInfo[] lights = scene.LightData.LightList;
                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);

                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    int lx, ly, lz;
                    lightInfo.VoxelLocation(detailLevel, out lx, out ly, out lz);
                    lightPercentage += lightInfo.GetLightPercentageDiffuseAndAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                }
                return new Color3f(lightPercentage, lightPercentage, lightPercentage);
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel,
                int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides, bool skipVoxelNormalCalc)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                Vector3f voxelNormal = !skipVoxelNormalCalc ? voxelSidesNormalCache[(int)visibleSides] : Vector3f.Zero;
                bool calculateSurfaceAngle = voxelNormal != Vector3f.Zero;
                LightInfo[] lights = scene.LightData.LightList;
                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);

                float lightPercentage = 0.0f;
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];

                    int lx, ly, lz;
                    lightInfo.VoxelLocation(detailLevel, out lx, out ly, out lz);

                    float brightness;
                    if (!calculateSurfaceAngle)
                        brightness = 1.0f;
                    else
                    {
                        Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                        brightness = MathHelper.Clamp(Vector3f.Dot(ref voxelNormal, ref surfaceToLight) / surfaceToLight.LengthFast, 0.0f, 1.0f);
                    }
                    lightPercentage += (brightness * lightInfo.GetLightPercentageDiffuse(voxelX32, voxelY32, voxelZ32, lightingModel) +
                        lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel));
                }
                return new Color3f(lightPercentage, lightPercentage, lightPercentage);
            }

            public override Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel,
                int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                Color3f lightColor = GetLightAt(voxelX, voxelY, voxelZ, detailLevel, shadowDetailLevel, worldBlockX, worldBlockY, worldBlockZ, visibleSides, voxel.SkipVoxelNormalCalc);
                return new Color4b(lightColor.R, lightColor.G, lightColor.B, 1.0f);
            }
        }
        #endregion

        #region NoShadowsLightProvider class
        private class NoShadowsLightProvider : LightProvider
        {
            public NoShadowsLightProvider(Scene scene) : base(scene)
            {
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel)
            {
                int bitShift = BlockLOD.GetVoxelSizeBitShift(detailLevel);
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(voxelX >> bitShift, voxelY >> bitShift, voxelZ >> bitShift);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                Color3f color = scene.AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);

                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    int lx, ly, lz;
                    lightInfo.VoxelLocation(detailLevel, out lx, out ly, out lz);
                    color += lightInfo.LightColor * lightInfo.GetLightPercentageDiffuseAndAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                }
                return color;
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel, 
                int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides, bool skipVoxelNormalCalc)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                Vector3f voxelNormal = !skipVoxelNormalCalc ? voxelSidesNormalCache[(int)visibleSides] : Vector3f.Zero;
                bool calculateSurfaceAngle = voxelNormal != Vector3f.Zero;
                Color3f color = scene.AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);

                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];

                    int lx, ly, lz;
                    lightInfo.VoxelLocation(detailLevel, out lx, out ly, out lz);

                    float brightness;
                    if (!calculateSurfaceAngle)
                        brightness = 1.0f;
                    else
                    {
                        Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                        brightness = MathHelper.Clamp(Vector3f.Dot(ref voxelNormal, ref surfaceToLight) / surfaceToLight.LengthFast, 0.0f, 1.0f);
                    }
                    color += lightInfo.LightColor * (brightness * lightInfo.GetLightPercentageDiffuse(voxelX32, voxelY32, voxelZ32, lightingModel) +
                        lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel));
                }
                return color;
            }
        }
        #endregion

        #region WithShadowsLightProviderBase class
        private abstract class WithShadowsLightProviderBase : LightProvider
        {
            protected WithShadowsLightProviderBase(Scene scene) : base(scene)
            {
            }

            protected abstract bool NoVoxelInLine(int x, int y, int z, int endX, int endY, int endZ, LODLevel detailLevel);

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel)
            {
                int bitShift = BlockLOD.GetVoxelSizeBitShift(detailLevel);
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(voxelX >> bitShift, voxelY >> bitShift, voxelZ >> bitShift);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                GameWorld world = scene.GameWorldInternal;
                Color3f color = scene.AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);

                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    int lx, ly, lz;
                    lightInfo.VoxelLocation(detailLevel, out lx, out ly, out lz);
                    if (world.NoVoxelInLine(voxelX, voxelY, voxelZ, lx, ly, lz, detailLevel))
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageDiffuseAndAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                    else
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                }
                return color;
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel, 
                int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides, bool skipVoxelNormalCalc)
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

                Vector3f voxelNormal = !skipVoxelNormalCalc ? voxelSidesNormalCache[(int)visibleSides] : Vector3f.Zero;
                bool calculateSurfaceAngle = voxelNormal != Vector3f.Zero;

                // For thread-safety copy all member variables
                Color3f color = scene.AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);
                int lightBitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(detailLevel);
                int shadowBitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(shadowDetailLevel);
                int voxelShadowX = voxelX32 >> shadowBitShift;
                int voxelShadowY = voxelY32 >> shadowBitShift;
                int voxelShadowZ = voxelZ32 >> shadowBitShift;

                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    int lx = lightInfo.VoxelLocX >> lightBitShift;
                    int ly = lightInfo.VoxelLocY >> lightBitShift;
                    int lz = lightInfo.VoxelLocZ >> lightBitShift;
                    int lxShadow = lightInfo.VoxelLocX >> shadowBitShift;
                    int lyShadow = lightInfo.VoxelLocY >> shadowBitShift;
                    int lzShadow = lightInfo.VoxelLocZ >> shadowBitShift;

                    float brightness;
                    if (!calculateSurfaceAngle)
                        brightness = 1.0f;
                    else
                    {
                        Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                        brightness = MathHelper.Clamp(Vector3f.Dot(ref voxelNormal, ref surfaceToLight) / surfaceToLight.LengthFast, 0.0f, 1.0f);
                    }

                    if ((availableMinusX && lx <= voxelX && NoVoxelInLine(voxelShadowX - 1, voxelShadowY, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                        (availableMinusY && ly <= voxelY && NoVoxelInLine(voxelShadowX, voxelShadowY - 1, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                        (availableMinusZ && lz <= voxelZ && NoVoxelInLine(voxelShadowX, voxelShadowY, voxelShadowZ - 1, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                        (availablePlusX && lx >= voxelX && NoVoxelInLine(voxelShadowX + 1, voxelShadowY, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                        (availablePlusY && ly >= voxelY && NoVoxelInLine(voxelShadowX, voxelShadowY + 1, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                        (availablePlusZ && lz >= voxelZ && NoVoxelInLine(voxelShadowX, voxelShadowY, voxelShadowZ + 1, lxShadow, lyShadow, lzShadow, shadowDetailLevel)))
                    {
                        color += lightInfo.LightColor * (brightness * lightInfo.GetLightPercentageDiffuse(voxelX32, voxelY32, voxelZ32, lightingModel) +
                            lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel));
                    }
                    else
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                }
                return color;
            }
        }
        #endregion

        #region WithShadowsLightProviderFast class
        private sealed class WithShadowsLightProviderFast : WithShadowsLightProviderBase
        {
            public WithShadowsLightProviderFast(Scene scene) : base(scene)
            {
            }
            
            protected override bool NoVoxelInLine(int x, int y, int z, int endX, int endY, int endZ, LODLevel detailLevel)
            {
                return scene.GameWorldInternal.NoVoxelInLineFast(x, y, z, endX, endY, endZ, detailLevel);
            }
        }
        #endregion

        #region WithShadowsLightProviderPerfect class
        private sealed class WithShadowsLightProviderPerfect : WithShadowsLightProviderBase
        {
            public WithShadowsLightProviderPerfect(Scene scene) : base(scene)
            {
            }
            
            protected override bool NoVoxelInLine(int x, int y, int z, int endX, int endY, int endZ, LODLevel detailLevel)
            {
                return scene.GameWorldInternal.NoVoxelInLine(x, y, z, endX, endY, endZ, detailLevel);
            }
        }
        #endregion
    }
}
