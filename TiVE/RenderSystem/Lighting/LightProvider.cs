using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVE.RenderSystem.Voxels;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Lighting
{
    #region LightComplexity enum
    internal enum LightComplexity
    {
        Simple,
        Realistic,
        RealisticWithShadows
    }
    #endregion

    internal abstract class LightProvider
    {
        #region Constants
        private const int HalfBlockVoxelSize = Block.VoxelSize / 2;
        #endregion

        #region Member variables
        private readonly Scene scene;
        private readonly Vector3i blockSize;
        private readonly Vector3i chunkSize;
        private readonly LightingModel lightingModel;
        private readonly ChunkLights[] chunkLightInfo;
        private LightInfo[] lightInfoList;
        private volatile int totalComplete;
        private volatile int lastPercentComplete;
        #endregion

        #region Constructor/singleton getter
        private LightProvider(Scene scene)
        {
            this.scene = scene;

            GameWorld gameWorld = scene.GameWorld;
            blockSize = new Vector3i(gameWorld.BlockSize.X, gameWorld.BlockSize.Y, gameWorld.BlockSize.Z);
            chunkSize = new Vector3i((int)Math.Ceiling(gameWorld.BlockSize.X / (float)ChunkComponent.BlockSize),
                (int)Math.Ceiling(gameWorld.BlockSize.Y / (float)ChunkComponent.BlockSize),
                (int)Math.Ceiling(gameWorld.BlockSize.Z / (float)ChunkComponent.BlockSize));

            chunkLightInfo = new ChunkLights[chunkSize.X * chunkSize.Y * chunkSize.Z];

            AmbientLight = Color3f.Empty;
            lightingModel = LightingModel.Get(gameWorld.LightingModelType);
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
                default: return new SimpleLightProvider(scene);
            }
        }
        #endregion

        #region Properties
        protected abstract int MaxBlocksBeforeCull { get; }

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
        public abstract Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides);

        /// <summary>
        /// Caches lighting information for the scene
        /// </summary>
        public void Calculate()
        {
            Messages.Print("Calculating static lighting...");
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < chunkLightInfo.Length; i++)
                chunkLightInfo[i] = new ChunkLights(100);
            
            int numThreads = Environment.ProcessorCount > 3 ? Environment.ProcessorCount / 2 : 1;
            Thread[] threads = new Thread[numThreads];
            List<LightInfo> lightInfos = new List<LightInfo>(20000);
            lightInfos.Add(new LightInfo());
            for (int i = 0; i < numThreads; i++)
                threads[i] = StartLightCalculationThread("Light " + (i + 1), lightInfos, i * blockSize.X / numThreads, (i + 1) * blockSize.X / numThreads);

            foreach (Thread thread in threads)
                thread.Join();

            for (int i = 0; i < chunkLightInfo.Length; i++)
                chunkLightInfo[i].LightsAffectingChunks.TrimExcess();

            lightInfoList = lightInfos.ToArray();

            sw.Stop();
            Messages.AddDoneText();
            Messages.AddDebug(string.Format("Lighting for {0} lights took {1}ms", lightInfos.Count - 1, sw.ElapsedMilliseconds));
        }

        public void CacheLightsInBlocksForChunk(ChunkComponent chunkData)
        {
            GameWorld gameWorld = scene.GameWorld;
            int arrayOffset = GetLightsAffectingChunksOffset(chunkData.ChunkLoc.X, chunkData.ChunkLoc.Y, chunkData.ChunkLoc.Z);
            if (chunkLightInfo[arrayOffset].BlockLights != null)
                return; // Chunk data is already loaded

            List<ushort> lightsAffectingChunk = chunkLightInfo[arrayOffset].LightsAffectingChunks;

            int maxLightsPerBlock = TiVEController.UserSettings.Get(UserSettings.LightsPerBlockKey);
            ushort[][] blocksLights = new ushort[ChunkComponent.BlockSize * ChunkComponent.BlockSize * ChunkComponent.BlockSize][];
            for (int i = 0; i < blocksLights.Length; i++)
                blocksLights[i] = new ushort[maxLightsPerBlock];

            int startX = chunkData.ChunkBlockLoc.X;
            int startY = chunkData.ChunkBlockLoc.Y;
            int startZ = chunkData.ChunkBlockLoc.Z;
            int sizeX = gameWorld.BlockSize.X;
            int sizeY = gameWorld.BlockSize.Y;
            int sizeZ = gameWorld.BlockSize.Z;
            int endX = Math.Min(sizeX, startX + ChunkComponent.BlockSize);
            int endY = Math.Min(sizeY, startY + ChunkComponent.BlockSize);
            int endZ = Math.Min(sizeZ, startZ + ChunkComponent.BlockSize);
            int maxBlockCount = MaxBlocksBeforeCull;
            for (int lightIndex = 0; lightIndex < lightsAffectingChunk.Count; lightIndex++)
            {
                ushort lightId = lightsAffectingChunk[lightIndex];
                LightInfo lightInfo = lightInfoList[lightId];
                int blockX = lightInfo.BlockX;
                int blockY = lightInfo.BlockY;
                int blockZ = lightInfo.BlockZ;
                for (int bz = startZ; bz < endZ; bz++)
                {
                    int vz = bz * Block.VoxelSize + HalfBlockVoxelSize;
                    for (int bx = startX; bx < endX; bx++)
                    {
                        int vx = bx * Block.VoxelSize + HalfBlockVoxelSize;
                        for (int by = startY; by < endY; by++)
                        {
                            if (!gameWorld.LessThanBlockCountInLine(blockX, blockY, blockZ, bx, by, bz, maxBlockCount) &&
                                !gameWorld.LessThanBlockCountInLine(bx, by, bz, blockX, blockY, blockZ, maxBlockCount))
                            {
                                continue; // Unlikely the light will actually hit the block at all
                            }

                            int vy = by * Block.VoxelSize + HalfBlockVoxelSize;
                            ushort[] blockLights = blocksLights[GetBlockLightOffset(bx - startX, by - startY, bz - startZ)];

                            // Calculate lighting information
                            // Sort lights by highest percentage to lowest
                            float newLightPercentage = lightInfo.GetLightPercentage(vx, vy, vz, lightingModel);
                            int leastLightIndex = blockLights.Length;
                            for (int i = 0; i < blockLights.Length; i++)
                            {
                                if (blockLights[i] == 0 || lightInfoList[blockLights[i]].GetLightPercentage(vx, vy, vz, lightingModel) < newLightPercentage)
                                {
                                    leastLightIndex = i;
                                    break;
                                }
                            }

                            if (leastLightIndex < blockLights.Length)
                            {
                                for (int i = blockLights.Length - 1; i > leastLightIndex; i--)
                                    blockLights[i] = blockLights[i - 1];

                                blockLights[leastLightIndex] = lightId;
                            }
                        }
                    }
                }
            }

            chunkLightInfo[arrayOffset].BlockLights = blocksLights;
        }

        public void RemoveLightsForChunk(ChunkComponent chunkData)
        {
            chunkLightInfo[GetLightsAffectingChunksOffset(chunkData.ChunkLoc.X, chunkData.ChunkLoc.Y, chunkData.ChunkLoc.Z)].BlockLights = null;
        }
        #endregion

        #region Private helper methods
        private Thread StartLightCalculationThread(string threadName, List<LightInfo> lightInfos, int startX, int endX)
        {
            Thread thread = new Thread(() =>
            {
                int sizeX = blockSize.X;
                int sizeY = blockSize.Y;
                int sizeZ = blockSize.Z;
                BlockList blockList = scene.BlockList;
                GameWorld gameWorld = scene.GameWorld;

                for (int x = startX; x < endX; x++)
                {
                    int percentComplete = totalComplete * 100 / (sizeX - 1);
                    if (percentComplete != lastPercentComplete)
                    {
                        Console.WriteLine("Calculating lighting: {0}%", percentComplete);
                        lastPercentComplete = percentComplete;
                    }

                    for (int z = 0; z < sizeZ; z++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            LightComponent light = blockList[gameWorld[x, y, z]].GetComponent<LightComponent>();
                            if (light != null)
                                FillChunksWithLight(light, lightInfos, x, y, z);
                        }
                    }
                    totalComplete++;
                }
            });

            thread.Name = threadName;
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
            return thread;
        }

        private void FillChunksWithLight(LightComponent light, List<LightInfo> lightInfos, int blockX, int blockY, int blockZ)
        {
            int startX = Math.Max(0, (blockX - light.LightBlockDist) / ChunkComponent.BlockSize);
            int startY = Math.Max(0, (blockY - light.LightBlockDist) / ChunkComponent.BlockSize);
            int startZ = Math.Max(0, (blockZ - light.LightBlockDist) / ChunkComponent.BlockSize);
            int endX = Math.Min(chunkSize.X, (int)Math.Ceiling((blockX + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));
            int endY = Math.Min(chunkSize.Y, (int)Math.Ceiling((blockY + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));
            int endZ = Math.Min(chunkSize.Z, (int)Math.Ceiling((blockZ + light.LightBlockDist + 1) / (float)ChunkComponent.BlockSize));

            ushort lightIndex;
            lock (lightInfos)
            {
                if (lightInfos.Count == ushort.MaxValue)
                {
                    Messages.AddWarning("Too many lights in game world");
                    return; // Too many lights in the world
                }
                lightIndex = (ushort)lightInfos.Count;
                lightInfos.Add(new LightInfo(blockX, blockY, blockZ, light, lightingModel.GetCacheLightCalculation(light),
                    lightingModel.GetCacheLightCalculationForShadow(light)));
            }

            for (int cz = startZ; cz < endZ; cz++)
            {
                for (int cx = startX; cx < endX; cx++)
                {
                    for (int cy = startY; cy < endY; cy++)
                    {
                        List<ushort> lightsInChunk = chunkLightInfo[GetLightsAffectingChunksOffset(cx, cy, cz)].LightsAffectingChunks;
                        lock (lightsInChunk)
                            lightsInChunk.Add(lightIndex);
                    }
                }
            }
        }

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

        /// <summary>
        /// Gets the offset into the block lights array of a chunk for the block at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBlockLightOffset(int blockX, int blockY, int blockZ)
        {
            return (blockX * ChunkComponent.BlockSize + blockZ) * ChunkComponent.BlockSize + blockY;
        }

        /// <summary>
        /// Gets the offset into the lights affecting chunks array for the chunk at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetLightsAffectingChunksOffset(int chunkX, int chunkY, int chunkZ)
        {
            Vector3i size = chunkSize;
            MiscUtils.CheckConstraints(chunkX, chunkY, chunkZ, size);
            return (chunkZ * size.X + chunkX) * size.Y + chunkY;
        }
        #endregion

        #region SimpleLightProvider class
        private sealed class SimpleLightProvider : LightProvider
        {
            public SimpleLightProvider(Scene scene) : base(scene)
            {
            }

            protected override int MaxBlocksBeforeCull
            {
                get { return 2; }
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                int worldBlockX = voxelX / Block.VoxelSize;
                int worldBlockY = voxelY / Block.VoxelSize;
                int worldBlockZ = voxelZ / Block.VoxelSize;
                int chunkX = worldBlockX / ChunkComponent.BlockSize;
                int chunkY = worldBlockY / ChunkComponent.BlockSize;
                int chunkZ = worldBlockZ / ChunkComponent.BlockSize;

                ushort[][] chunkLights = chunkLightInfo[GetLightsAffectingChunksOffset(chunkX, chunkY, chunkZ)].BlockLights;
                if (chunkLights == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                int chunkBlockX = worldBlockX % ChunkComponent.BlockSize;
                int chunkBlockY = worldBlockY % ChunkComponent.BlockSize;
                int chunkBlockZ = worldBlockZ % ChunkComponent.BlockSize;
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
                ushort[] lightsInBlock = chunkLights[GetBlockLightOffset(chunkBlockX, chunkBlockY, chunkBlockZ)];
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

            public override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                int chunkX = worldBlockX / ChunkComponent.BlockSize;
                int chunkY = worldBlockY / ChunkComponent.BlockSize;
                int chunkZ = worldBlockZ / ChunkComponent.BlockSize;

                ushort[][] chunkLights = chunkLightInfo[GetLightsAffectingChunksOffset(chunkX, chunkY, chunkZ)].BlockLights;
                if (chunkLights == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                bool availableMinusX = (visibleSides & VoxelSides.Left) != 0;
                bool availableMinusY = (visibleSides & VoxelSides.Bottom) != 0;
                bool availableMinusZ = (visibleSides & VoxelSides.Back) != 0;
                bool availablePlusX = (visibleSides & VoxelSides.Right) != 0;
                bool availablePlusY = (visibleSides & VoxelSides.Top) != 0;
                bool availablePlusZ = (visibleSides & VoxelSides.Front) != 0;

                int chunkBlockX = worldBlockX % ChunkComponent.BlockSize;
                int chunkBlockY = worldBlockY % ChunkComponent.BlockSize;
                int chunkBlockZ = worldBlockZ % ChunkComponent.BlockSize;
                ushort[] lightsInBlock = chunkLights[GetBlockLightOffset(chunkBlockX, chunkBlockY, chunkBlockZ)];
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
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
                        color += lightInfo.LightColor * (lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel) * 0.9f);
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

            protected override int MaxBlocksBeforeCull
            {
                get { return 2; }
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                int worldBlockX = voxelX / Block.VoxelSize;
                int worldBlockY = voxelY / Block.VoxelSize;
                int worldBlockZ = voxelZ / Block.VoxelSize;
                int chunkX = worldBlockX / ChunkComponent.BlockSize;
                int chunkY = worldBlockY / ChunkComponent.BlockSize;
                int chunkZ = worldBlockZ / ChunkComponent.BlockSize;

                ushort[][] chunkLights = chunkLightInfo[GetLightsAffectingChunksOffset(chunkX, chunkY, chunkZ)].BlockLights;
                if (chunkLights == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                int chunkBlockX = worldBlockX % ChunkComponent.BlockSize;
                int chunkBlockY = worldBlockY % ChunkComponent.BlockSize;
                int chunkBlockZ = worldBlockZ % ChunkComponent.BlockSize;
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
                ushort[] lightsInBlock = chunkLights[GetBlockLightOffset(chunkBlockX, chunkBlockY, chunkBlockZ)];
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

            public override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                Vector3f voxelNormal = GetVoxelNormal(visibleSides);
                int chunkX = worldBlockX / ChunkComponent.BlockSize;
                int chunkY = worldBlockY / ChunkComponent.BlockSize;
                int chunkZ = worldBlockZ / ChunkComponent.BlockSize;

                ushort[][] chunkLights = chunkLightInfo[GetLightsAffectingChunksOffset(chunkX, chunkY, chunkZ)].BlockLights;
                if (chunkLights == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                int chunkBlockX = worldBlockX % ChunkComponent.BlockSize;
                int chunkBlockY = worldBlockY % ChunkComponent.BlockSize;
                int chunkBlockZ = worldBlockZ % ChunkComponent.BlockSize;
                ushort[] lightsInBlock = chunkLights[GetBlockLightOffset(chunkBlockX, chunkBlockY, chunkBlockZ)];
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
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

            protected override int MaxBlocksBeforeCull
            {
                get { return 3; }
            }

            public override Color3f GetLightAtFast(int voxelX, int voxelY, int voxelZ)
            {
                int worldBlockX = voxelX / Block.VoxelSize;
                int worldBlockY = voxelY / Block.VoxelSize;
                int worldBlockZ = voxelZ / Block.VoxelSize;
                int chunkX = worldBlockX / ChunkComponent.BlockSize;
                int chunkY = worldBlockY / ChunkComponent.BlockSize;
                int chunkZ = worldBlockZ / ChunkComponent.BlockSize;

                ushort[][] chunkLights = chunkLightInfo[GetLightsAffectingChunksOffset(chunkX, chunkY, chunkZ)].BlockLights;
                if (chunkLights == null)
                    return Color3f.Empty; // Probably unloaded the chunk while loading

                int chunkBlockX = worldBlockX % ChunkComponent.BlockSize;
                int chunkBlockY = worldBlockY % ChunkComponent.BlockSize;
                int chunkBlockZ = worldBlockZ % ChunkComponent.BlockSize;
                GameWorld world = scene.GameWorld;
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;
                ushort[] lightsInBlock = chunkLights[GetBlockLightOffset(chunkBlockX, chunkBlockY, chunkBlockZ)];
                for (int i = 0; i < lightsInBlock.Length; i++)
                {
                    ushort lightIndex = lightsInBlock[i];
                    if (lightIndex == 0)
                        break;

                    LightInfo lightInfo = lights[lightIndex];
                    if (world.NoVoxelInLine(voxelX, voxelY, voxelZ, lightInfo.VoxelLocX, lightInfo.VoxelLocY, lightInfo.VoxelLocZ))
                        color += lightInfo.LightColor * lightInfo.GetLightPercentage(voxelX, voxelY, voxelZ, lightingModel);

                    color += lightInfo.LightColor * lightInfo.GetLightPercentageShadow(voxelX, voxelY, voxelZ, lightingModel);
                }
                return color;
            }

            public override Color3f GetLightAt(int voxelX, int voxelY, int voxelZ, 
                int voxelSize, int worldBlockX, int worldBlockY, int worldBlockZ, VoxelSides visibleSides)
            {
                bool availableMinusX = (visibleSides & VoxelSides.Left) != 0;
                bool availableMinusY = (visibleSides & VoxelSides.Bottom) != 0;
                bool availableMinusZ = (visibleSides & VoxelSides.Back) != 0;
                bool availablePlusX = (visibleSides & VoxelSides.Right) != 0;
                bool availablePlusY = (visibleSides & VoxelSides.Top) != 0;
                bool availablePlusZ = (visibleSides & VoxelSides.Front) != 0;

                int chunkX = worldBlockX / ChunkComponent.BlockSize;
                int chunkY = worldBlockY / ChunkComponent.BlockSize;
                int chunkZ = worldBlockZ / ChunkComponent.BlockSize;
                int chunkBlockX = worldBlockX % ChunkComponent.BlockSize;
                int chunkBlockY = worldBlockY % ChunkComponent.BlockSize;
                int chunkBlockZ = worldBlockZ % ChunkComponent.BlockSize;

                // For thread-safety copy all member variables
                ushort[][] chunkLights = chunkLightInfo[GetLightsAffectingChunksOffset(chunkX, chunkY, chunkZ)].BlockLights;
                if (chunkLights == null)
                    return Color3f.Empty; // Probably unloaded the scene while loading the chunk

                ushort[] lightsInBlock = chunkLights[GetBlockLightOffset(chunkBlockX, chunkBlockY, chunkBlockZ)];
                GameWorld world = scene.GameWorld;
                Color3f color = AmbientLight;
                LightInfo[] lights = lightInfoList;

                Vector3f voxelNormal = GetVoxelNormal(visibleSides);
                
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

                    if (lightPercentage > 0.0f && ((availableMinusX && world.NoVoxelInLine(voxelX - 1, voxelY, voxelZ, lx, ly, lz)) ||
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

        #region ChunkLights structure
        /// <summary>
        /// Holds lighting information for a single chunk
        /// </summary>
        private struct ChunkLights
        {
            public readonly List<ushort> LightsAffectingChunks;

            public ushort[][] BlockLights;

            public ChunkLights(int maxLightsAffectingChunk)
            {
                LightsAffectingChunks = new List<ushort>(maxLightsAffectingChunk);
                BlockLights = null;
            }
        }
        #endregion
    }
}
