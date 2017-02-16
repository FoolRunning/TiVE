using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;
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
            for (int s = (int)CubeSides.None; s <= (int)CubeSides.All; s++)
                voxelSidesNormalCache[s] = GetVoxelNormal((CubeSides)s);
        }

        /// <summary>
        /// Gets a light provider for the specified game world using the current user settings to determine the complexity
        /// </summary>
        public static LightProvider Create(Scene scene, bool withShadows)
        {
            LightType lightType = (LightType)(int)TiVEController.UserSettings.Get(UserSettings.LightingTypeKey);
            if (lightType == LightType.Debug1)
                return new DebugLightCountProvider(scene);
            if (lightType == LightType.Debug2)
                return new DebugLightCoverageProvider(scene);
            return withShadows ? new WithShadowsLightProvider(scene) : (LightProvider)new NoShadowsLightProvider(scene);
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
            int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides, bool skipVoxelNormalCalc);

        public virtual Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel,
            int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides)
        {
            Color3f lightColorPercentage = GetLightAt(voxelX, voxelY, voxelZ, detailLevel, shadowDetailLevel, 
                worldBlockX, worldBlockY, worldBlockZ, visibleSides, voxel.SkipVoxelNormalCalc);
            return new Color4b(RestrainToByte(voxel.R * lightColorPercentage.R), 
                RestrainToByte(voxel.G * lightColorPercentage.G), RestrainToByte(voxel.B * lightColorPercentage.B), voxel.A);
        }
        #endregion

        #region Private helper methods
        private static Vector3f GetVoxelNormal(CubeSides visibleSides)
        {
            if (visibleSides == CubeSides.All)
                return Vector3f.Zero;

            Vector3f vector = new Vector3f();
            if ((visibleSides & CubeSides.Left) != 0)
                vector.X -= 1.0f;
            if ((visibleSides & CubeSides.Bottom) != 0)
                vector.Y -= 1.0f;
            if ((visibleSides & CubeSides.Back) != 0)
                vector.Z -= 1.0f;

            if ((visibleSides & CubeSides.Right) != 0)
                vector.X += 1.0f;
            if ((visibleSides & CubeSides.Top) != 0)
                vector.Y += 1.0f;
            if ((visibleSides & CubeSides.Front) != 0)
                vector.Z += 1.0f;

            vector.NormalizeFast();
            return vector;
        }

        private static byte RestrainToByte(float value)
        {
            return value > 255.0f ? (byte)255 : (byte)value;
        }
        #endregion

        #region DebugLightCountProvider class
        private sealed class DebugLightCountProvider : LightProvider
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
                new Color3f(1.0f, 0.5f, 1.0f),
                new Color3f(1.0f, 0.6f, 1.0f),
                new Color3f(1.0f, 0.7f, 1.0f),
                new Color3f(1.0f, 0.8f, 1.0f),
                new Color3f(1.0f, 0.9f, 1.0f),
                new Color3f(1.0f, 1.0f, 1.0f) // 20
            };

            public DebugLightCountProvider(Scene scene) : base(scene)
            {
            }

            public override Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel,
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides)
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
                return GetLightAt(voxelX, voxelY, voxelZ, detailLevel, shadowDetailLevel, worldBlockX, worldBlockY, worldBlockZ, CubeSides.All, false);
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel, 
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides, bool skipVoxelNormalCalc)
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

        #region DebugLightProvider class
        private sealed class DebugLightCoverageProvider : LightProvider
        {
            public DebugLightCoverageProvider(Scene scene) : base(scene)
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
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides, bool skipVoxelNormalCalc)
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
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides)
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
                    int lightIndex = lightsInBlock[i];
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
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides, bool skipVoxelNormalCalc)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                Color3f color = scene.AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);

                Vector3f voxelNormal = !skipVoxelNormalCalc ? voxelSidesNormalCache[(int)visibleSides] : Vector3f.Zero;
                if (voxelNormal == Vector3f.Zero)
                {
                    for (int i = 0; i < lightsInBlock.Length; i++)
                    {
                        int lightIndex = lightsInBlock[i];
                        if (lightIndex == 0)
                            break;

                        LightInfo lightInfo = lights[lightIndex];
                        color += lightInfo.LightColor * (lightInfo.GetLightPercentageDiffuse(voxelX32, voxelY32, voxelZ32, lightingModel) +
                            lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel));
                    }
                }
                else
                {
                    for (int i = 0; i < lightsInBlock.Length; i++)
                    {
                        int lightIndex = lightsInBlock[i];
                        if (lightIndex == 0)
                            break;

                        LightInfo lightInfo = lights[lightIndex];

                        int lx, ly, lz;
                        lightInfo.VoxelLocation(detailLevel, out lx, out ly, out lz);
                        Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                        float brightness = MathHelper.Clamp(Vector3f.Dot(ref voxelNormal, ref surfaceToLight) / surfaceToLight.LengthFast, 0.0f, 1.0f);

                        color += lightInfo.LightColor * (brightness * lightInfo.GetLightPercentageDiffuse(voxelX32, voxelY32, voxelZ32, lightingModel) +
                            lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel));
                    }
                }
                return color;
            }
        }
        #endregion

        #region WithShadowsLightProvider class
        private class WithShadowsLightProvider : LightProvider
        {
            private int shadowsPerBlock;
            public WithShadowsLightProvider(Scene scene) : base(scene)
            {
                shadowsPerBlock = TiVEController.UserSettings.Get(UserSettings.ShadowsPerBlockKey);
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel)
            {
                int bitShift = BlockLOD.GetVoxelSizeBitShift(detailLevel);
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(voxelX >> bitShift, voxelY >> bitShift, voxelZ >> bitShift);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                GameWorld world = scene.GameWorldInternal;
                Color3f color = scene.AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                int maxShadowCount = shadowsPerBlock;
                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);
                int shadowBitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(shadowDetailLevel);
                int voxelShadowX = voxelX32 >> shadowBitShift;
                int voxelShadowY = voxelY32 >> shadowBitShift;
                int voxelShadowZ = voxelZ32 >> shadowBitShift;

                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    int lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    int lxShadow = lightInfo.VoxelLocX >> shadowBitShift;
                    int lyShadow = lightInfo.VoxelLocY >> shadowBitShift;
                    int lzShadow = lightInfo.VoxelLocZ >> shadowBitShift;

                    int lx, ly, lz;
                    lightInfo.VoxelLocation(detailLevel, out lx, out ly, out lz);
                    if (i >= maxShadowCount || world.NoVoxelInLine(voxelShadowX - 1, voxelShadowY, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel))
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageDiffuseAndAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                    else
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                }
                return color;
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel, 
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides, bool skipVoxelNormalCalc)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the scene while loading the chunk

                int voxelX32 = voxelX;
                int voxelY32 = voxelY;
                int voxelZ32 = voxelZ;
                LODUtils.AdjustLocationForDetailLevelTo32(ref voxelX32, ref voxelY32, ref voxelZ32, detailLevel);
                int shadowBitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(shadowDetailLevel);
                int voxelShadowX = voxelX32 >> shadowBitShift;
                int voxelShadowY = voxelY32 >> shadowBitShift;
                int voxelShadowZ = voxelZ32 >> shadowBitShift;

                Vector3i shadowWorldSize = LODUtils.AdjustLocationForDetailLevelFrom32(scene.GameWorld.VoxelSize32, shadowDetailLevel);
                bool availableMinusX = (visibleSides & CubeSides.Left) != 0 && voxelShadowX > 0;
                bool availableMinusY = (visibleSides & CubeSides.Bottom) != 0 && voxelShadowY > 0;
                bool availableMinusZ = (visibleSides & CubeSides.Back) != 0 && voxelShadowZ > 0;
                bool availablePlusX = (visibleSides & CubeSides.Right) != 0 && voxelShadowX < shadowWorldSize.X - 1;
                bool availablePlusY = (visibleSides & CubeSides.Top) != 0 && voxelShadowY < shadowWorldSize.Y - 1;
                bool availablePlusZ = (visibleSides & CubeSides.Front) != 0 && voxelShadowZ < shadowWorldSize.Z - 1;

                // For thread-safety copy all member variables
                Color3f color = scene.AmbientLight;
                LightInfo[] lights = scene.LightData.LightList;
                Vector3f voxelNormal = !skipVoxelNormalCalc ? voxelSidesNormalCache[(int)visibleSides] : Vector3f.Zero;
                GameWorld gameWorld = scene.GameWorldInternal;
                int maxShadowCount = shadowsPerBlock;
                if (voxelNormal == Vector3f.Zero)
                {
                    for (int i = 0; i < lightsInBlock.Length; i++)
                    {
                        int lightIndex = lightsInBlock[i];
                        if (lightIndex == 0)
                            break;

                        LightInfo lightInfo = lights[lightIndex];
                        int lxShadow = lightInfo.VoxelLocX >> shadowBitShift;
                        int lyShadow = lightInfo.VoxelLocY >> shadowBitShift;
                        int lzShadow = lightInfo.VoxelLocZ >> shadowBitShift;

                        if (i >= maxShadowCount || 
                            (availableMinusX && gameWorld.NoVoxelInLine(voxelShadowX - 1, voxelShadowY, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availableMinusY && gameWorld.NoVoxelInLine(voxelShadowX, voxelShadowY - 1, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availableMinusZ && gameWorld.NoVoxelInLine(voxelShadowX, voxelShadowY, voxelShadowZ - 1, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availablePlusX && gameWorld.NoVoxelInLine(voxelShadowX + 1, voxelShadowY, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availablePlusY && gameWorld.NoVoxelInLine(voxelShadowX, voxelShadowY + 1, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availablePlusZ && gameWorld.NoVoxelInLine(voxelShadowX, voxelShadowY, voxelShadowZ + 1, lxShadow, lyShadow, lzShadow, shadowDetailLevel)))
                        {
                            color += lightInfo.LightColor * (lightInfo.GetLightPercentageDiffuse(voxelX32, voxelY32, voxelZ32, lightingModel) +
                                lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel));
                        }
                        else
                            color += lightInfo.LightColor * lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                    }
                }
                else
                {
                    int lightBitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(detailLevel);
                    for (int i = 0; i < lightsInBlock.Length; i++)
                    {
                        int lightIndex = lightsInBlock[i];
                        if (lightIndex == 0)
                            break;

                        LightInfo lightInfo = lights[lightIndex];
                        int lx = lightInfo.VoxelLocX >> lightBitShift;
                        int ly = lightInfo.VoxelLocY >> lightBitShift;
                        int lz = lightInfo.VoxelLocZ >> lightBitShift;
                        int lxShadow = lightInfo.VoxelLocX >> shadowBitShift;
                        int lyShadow = lightInfo.VoxelLocY >> shadowBitShift;
                        int lzShadow = lightInfo.VoxelLocZ >> shadowBitShift;

                        if (i >= maxShadowCount || 
                            (availableMinusX && lx <= voxelX && gameWorld.NoVoxelInLine(voxelShadowX - 1, voxelShadowY, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availableMinusY && ly <= voxelY && gameWorld.NoVoxelInLine(voxelShadowX, voxelShadowY - 1, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availableMinusZ && lz <= voxelZ && gameWorld.NoVoxelInLine(voxelShadowX, voxelShadowY, voxelShadowZ - 1, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availablePlusX && lx >= voxelX && gameWorld.NoVoxelInLine(voxelShadowX + 1, voxelShadowY, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availablePlusY && ly >= voxelY && gameWorld.NoVoxelInLine(voxelShadowX, voxelShadowY + 1, voxelShadowZ, lxShadow, lyShadow, lzShadow, shadowDetailLevel)) ||
                            (availablePlusZ && lz >= voxelZ && gameWorld.NoVoxelInLine(voxelShadowX, voxelShadowY, voxelShadowZ + 1, lxShadow, lyShadow, lzShadow, shadowDetailLevel)))
                        {
                            Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                            float brightness = MathHelper.Clamp(Vector3f.Dot(ref voxelNormal, ref surfaceToLight) / surfaceToLight.LengthFast, 0.0f, 1.0f);
                            color += lightInfo.LightColor * (brightness * lightInfo.GetLightPercentageDiffuse(voxelX32, voxelY32, voxelZ32, lightingModel) +
                                lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel));
                        }
                        else
                            color += lightInfo.LightColor * lightInfo.GetLightPercentageAmbient(voxelX32, voxelY32, voxelZ32, lightingModel);
                    }
                }
                return color;
            }
        }
        #endregion
    }
}
