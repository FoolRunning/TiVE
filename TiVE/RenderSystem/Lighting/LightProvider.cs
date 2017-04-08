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
            if ((visibleSides & CubeSides.XMinus) != 0)
                vector.X -= 1.0f;
            if ((visibleSides & CubeSides.YMinus) != 0)
                vector.Y -= 1.0f;
            if ((visibleSides & CubeSides.ZMinus) != 0)
                vector.Z -= 1.0f;

            if ((visibleSides & CubeSides.XPlus) != 0)
                vector.X += 1.0f;
            if ((visibleSides & CubeSides.YPlus) != 0)
                vector.Y += 1.0f;
            if ((visibleSides & CubeSides.ZPlus) != 0)
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
                return Color3f.Empty; // Probably unloaded the chunk while loading
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
                return Color3f.Empty; // Probably unloaded the chunk while loading
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel,
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides, bool skipVoxelNormalCalc)
            {
                return Color3f.Empty; // Probably unloaded the chunk while loading
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
                return Color3f.Empty; // Probably unloaded the chunk while loading
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel, 
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides, bool skipVoxelNormalCalc)
            {
                return Color3f.Empty; // Probably unloaded the chunk while loading
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
                return Color3f.Empty; // Probably unloaded the chunk while loading
            }

            protected override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, LODLevel detailLevel, LODLevel shadowDetailLevel, 
                int worldBlockX, int worldBlockY, int worldBlockZ, CubeSides visibleSides, bool skipVoxelNormalCalc)
            {
                return Color3f.Empty; // Probably unloaded the scene while loading the chunk
            }
        }
        #endregion
    }
}
