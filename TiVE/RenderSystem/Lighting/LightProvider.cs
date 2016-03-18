using System;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.VoxelMeshSystem;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    #region LightType enum
    internal enum LightType
    {
        Realistic,
        Debug
    }
    #endregion

    internal enum ShadowType
    {
        None,
        Fast,
        Nice
    }

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
            LightType lightType = (LightType)(int)TiVEController.UserSettings.Get(UserSettings.LightingTypeKey);
            if (lightType == LightType.Debug)
                return new DebugLightProvider(scene);

            ShadowType shadowType = (ShadowType)(int)TiVEController.UserSettings.Get(UserSettings.ShadowTypeKey);
            return shadowType == ShadowType.None ? (LightProvider)new NoShadowsLightProvider(scene) : new WithShadowsLightProvider(scene, shadowType == ShadowType.Fast);
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
        protected abstract Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ,
            VoxelSides visibleSides, bool skipVoxelNormalCalc);

        public virtual Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
        {
            Color3f lightColorPercentage = GetLightAt(voxelX, voxelY, voxelZ, voxelSize, worldBlockX, worldBlockY, worldBlockZ, visibleSides, voxel.SkipVoxelNormalCalc);
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
                new Color3f(1.0f, 0.7f, 1.0f),
                new Color3f(1.0f, 0.8f, 1.0f),
                new Color3f(1.0f, 1.0f, 1.0f) // 20
                //new Color3f(0.0f, 0.2f, 0.0f),
                //new Color3f(0.0f, 1.0f, 0.0f),
                //new Color3f(0.1f, 1.0f, 0.0f),
                //new Color3f(0.2f, 1.0f, 0.0f),
                //new Color3f(0.3f, 1.0f, 0.0f),
                //new Color3f(0.4f, 1.0f, 0.0f),
                //new Color3f(0.5f, 1.0f, 0.0f),
                //new Color3f(0.6f, 1.0f, 0.0f),
                //new Color3f(0.7f, 1.0f, 0.0f),
                //new Color3f(0.8f, 1.0f, 0.0f),
                //new Color3f(0.9f, 1.0f, 0.0f), // 10
                //new Color3f(1.0f, 1.0f, 0.0f),
                //new Color3f(1.0f, 0.9f, 0.0f),
                //new Color3f(1.0f, 0.8f, 0.0f),
                //new Color3f(1.0f, 0.7f, 0.0f),
                //new Color3f(1.0f, 0.6f, 0.0f),
                //new Color3f(1.0f, 0.5f, 0.0f),
                //new Color3f(1.0f, 0.4f, 0.0f),
                //new Color3f(1.0f, 0.2f, 0.0f),
                //new Color3f(1.0f, 0.0f, 0.0f),
                //new Color3f(1.0f, 1.0f, 1.0f) // 20
            };

            public DebugLightProvider(Scene scene) : base(scene)
            {
            }

            public override Color4b GetFinalColor(Voxel voxel, int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY,
                int worldBlockZ, VoxelSides visibleSides)
            {
                Color3f lightColor = GetLightAt(voxelX, voxelY, voxelZ, voxelSize, worldBlockX, worldBlockY, worldBlockZ, visibleSides, false);
                return new Color4b(lightColor.R, lightColor.G, lightColor.B, 1.0f);
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                int worldBlockX = voxelX >> Block.VoxelSizeBitShift;
                int worldBlockY = voxelY >> Block.VoxelSizeBitShift;
                int worldBlockZ = voxelZ >> Block.VoxelSizeBitShift;
                return GetLightAt(voxelX, voxelY, voxelZ, 1, worldBlockX, worldBlockY, worldBlockZ, VoxelSides.All, false);
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ,
                VoxelSides visibleSides, bool skipVoxelNormalCalc)
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

        #region NoShadowsLightProvider class
        private sealed class NoShadowsLightProvider : LightProvider
        {
            public NoShadowsLightProvider(Scene scene) : base(scene)
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
                    color += lightInfo.LightColor * lightInfo.GetLightPercentageDiffuseAndAmbient(voxelX, voxelY, voxelZ, lightingModel);
                }
                return color;
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, 
                VoxelSides visibleSides, bool skipVoxelNormalCalc)
            {
                ushort[] lightsInBlock = scene.LightData.GetLightsForBlock(worldBlockX, worldBlockY, worldBlockZ);
                if (lightsInBlock == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                Vector3f voxelNormal;
                bool calculateSurfaceAngle;
                if (skipVoxelNormalCalc)
                {
                    voxelNormal = Vector3f.Zero;
                    calculateSurfaceAngle = false;
                }
                else
                {
                    voxelNormal = GetVoxelNormal(visibleSides);
                    calculateSurfaceAngle = voxelNormal != Vector3f.Zero;
                }

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
                    if (!calculateSurfaceAngle)
                        brightness = 1.0f;
                    else
                    {
                        Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                        float dot = voxelNormal.X * surfaceToLight.X + voxelNormal.Y * surfaceToLight.Y + voxelNormal.Z * surfaceToLight.Z;
                        brightness = MathHelper.Clamp(dot / surfaceToLight.LengthFast, 0.0f, 1.0f);
                    }
                    color += lightInfo.LightColor * (brightness * lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel) +
                        lightInfo.GetLightPercentageAmbient(voxelX, voxelY, voxelZ, lightingModel));
                }
                return color;
            }
        }
        #endregion

        #region WithShadowsLightProvider class
        private sealed class WithShadowsLightProvider : LightProvider
        {
            private readonly bool useFastShadowCalc;

            public WithShadowsLightProvider(Scene scene, bool useFastShadowCalc) : base(scene)
            {
                this.useFastShadowCalc = useFastShadowCalc;
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
                    if (world.NoVoxelInLineFast(voxelX, voxelY, voxelZ, lightInfo.VoxelLocX, lightInfo.VoxelLocY, lightInfo.VoxelLocZ))
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageDiffuseAndAmbient(voxelX, voxelY, voxelZ, lightingModel);
                    else
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageAmbient(voxelX, voxelY, voxelZ, lightingModel);
                }
                return color;
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, 
                VoxelSides visibleSides, bool skipVoxelNormalCalc)
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

                Vector3f voxelNormal = !skipVoxelNormalCalc ? GetVoxelNormal(visibleSides) : Vector3f.Zero;
                bool calculateSurfaceAngle = voxelNormal != Vector3f.Zero;

                // For thread-safety copy all member variables
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

                    float brightness;
                    if (!calculateSurfaceAngle)
                        brightness = 1.0f;
                    else
                    {
                        Vector3f surfaceToLight = new Vector3f(lx - voxelX, ly - voxelY, lz - voxelZ);
                        float dot = voxelNormal.X * surfaceToLight.X + voxelNormal.Y * surfaceToLight.Y + voxelNormal.Z * surfaceToLight.Z;
                        brightness = MathHelper.Clamp(dot / surfaceToLight.LengthFast, 0.0f, 1.0f);
                    }

                    if ((availableMinusX && NoVoxelInLine(world, voxelX - 1, voxelY, voxelZ, lx, ly, lz)) ||
                        (availableMinusY && NoVoxelInLine(world, voxelX, voxelY - 1, voxelZ, lx, ly, lz)) ||
                        (availableMinusZ && NoVoxelInLine(world, voxelX, voxelY, voxelZ - 1, lx, ly, lz)) ||
                        (availablePlusX && NoVoxelInLine(world, voxelX + voxelSize, voxelY, voxelZ, lx, ly, lz)) ||
                        (availablePlusY && NoVoxelInLine(world, voxelX, voxelY + voxelSize, voxelZ, lx, ly, lz)) ||
                        (availablePlusZ && NoVoxelInLine(world, voxelX, voxelY, voxelZ + voxelSize, lx, ly, lz)))
                    {
                        color += lightInfo.LightColor * (brightness * lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel) +
                            lightInfo.GetLightPercentageAmbient(voxelX, voxelY, voxelZ, lightingModel));
                    }
                    else
                        color += lightInfo.LightColor * lightInfo.GetLightPercentageAmbient(voxelX, voxelY, voxelZ, lightingModel);
                }
                return color;
            }

            private bool NoVoxelInLine(GameWorld world, int x, int y, int z, int endX, int endY, int endZ)
            {
                return useFastShadowCalc ? world.NoVoxelInLineFast(x, y, z, endX, endY, endZ) : world.NoVoxelInLine(x, y, z, endX, endY, endZ);
            }
        }
        #endregion
    }
}
