using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Lighting;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    /// <summary>
    /// Contains the information about the game world.
    /// </summary>
    internal sealed class GameWorld : IGameWorld, IDisposable
    {
        private readonly Vector3i voxelSize;
        private readonly Vector3i blockSize;
        private readonly Vector3i chunkSize;

        private readonly Block[] worldBlocks;
        private readonly GameWorldVoxelChunk[] worldChunks;
        private readonly Color3f ambientLight;
        private readonly BlockList blockList;
        private readonly GameWorldBox renderBox;

        internal GameWorld(int blockSizeX, int blockSizeY, int blockSizeZ, BlockList blockList)
        {
            this.blockList = blockList;

            blockSize = new Vector3i(blockSizeX, blockSizeY, blockSizeZ);
            voxelSize = new Vector3i(blockSizeX * BlockInformation.BlockSize, blockSizeY * BlockInformation.BlockSize, blockSizeZ * BlockInformation.BlockSize);
            chunkSize = new Vector3i((int)Math.Ceiling(blockSizeX / (float)GameWorldVoxelChunk.TileSize),
                (int)Math.Ceiling(blockSizeY / (float)GameWorldVoxelChunk.TileSize),
                (int)Math.Ceiling(blockSizeZ / (float)GameWorldVoxelChunk.TileSize));

            worldBlocks = new Block[blockSizeX * blockSizeY * blockSizeZ];
            for (int i = 0; i < worldBlocks.Length; i++)
                worldBlocks[i] = new Block(BlockInformation.Empty);

            worldChunks = new GameWorldVoxelChunk[chunkSize.X * chunkSize.Y * chunkSize.Z];
            for (int z = 0; z < chunkSize.Z; z++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    for (int y = 0; y < chunkSize.Y; y++)
                        worldChunks[GetChunkOffset(x, y, z)] = new GameWorldVoxelChunk(x, y, z);
                }
            }
            
            ambientLight = new Color3f(0.01f, 0.01f, 0.01f);

            renderBox = new GameWorldBox(this);
        }

        public void Dispose()
        {
            renderBox.Dispose();
        }

        /// <summary>
        /// Gets the voxel size of the game world
        /// </summary>
        public Vector3i VoxelSize
        {
            get { return voxelSize; }
        }

        /// <summary>
        /// Gets the size of the game world in chunks
        /// </summary>
        public Vector3i ChunkSize
        {
            get { return chunkSize; }
        }

        #region Implementation of IGameWorld
        /// <summary>
        /// Gets the size of the game world in blocks
        /// </summary>
        public Vector3i BlockSize
        {
            get { return blockSize; }
        }

        /// <summary>
        /// Gets/sets the block in the game world at the specified block location
        /// </summary>
        public BlockInformation this[int blockX, int blockY, int blockZ]
        {
            get { return worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockInfo; }
            set { worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockInfo = value ?? BlockInformation.Empty; }
        }
        #endregion

        public Color3f AmbientLight
        {
            get { return ambientLight; }
        }

        /// <summary>
        /// Gets the light value at the specified voxel
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color3f GetLightAt(int voxelX, int voxelY, int voxelZ)
        {
            Color3f color = ambientLight;

            int blockX = voxelX / BlockInformation.BlockSize;
            int blockY = voxelY / BlockInformation.BlockSize;
            int blockZ = voxelZ / BlockInformation.BlockSize;

            //return ambientLight + worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;
            List<LightInfo> blockLights = worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].Lights;
            for (int i = 0; i < blockLights.Count; i++)
            {
                LightInfo lightInfo = blockLights[i];
                color += lightInfo.Light.Color * LightUtils.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
            }
            return color;
        }

        public GameWorldVoxelChunk GetChunk(int chunkX, int chunkY, int chunkZ)
        {
            return worldChunks[GetChunkOffset(chunkX, chunkY, chunkZ)];
        }

        public List<LightInfo> GetLights(int blockX, int blockY, int blockZ)
        {
            return worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].Lights;
        }

        public Color3f GetBlockLight(int blockX, int blockY, int blockZ)
        {
            return worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;
        }

        public void SetBlockLight(int blockX, int blockY, int blockZ, Color3f light)
        {
            worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockLight = light;
        }

        public BlockState GetBlockState(int blockX, int blockY, int blockZ)
        {
            return worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].State;
        }

        public void SetBlockState(int blockX, int blockY, int blockZ, BlockState state)
        {
            worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].State = state;
        }

        /// <summary>
        /// Gets the voxel in the game world at the specified absolute voxel location
        /// </summary>
        /// <remarks>Very performance-critical method</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVoxel(int voxelX, int voxelY, int voxelZ)
        {
            CheckConstraints(voxelX, voxelY, voxelZ, voxelSize);

            int blockX = voxelX / BlockInformation.BlockSize;
            int blockY = voxelY / BlockInformation.BlockSize;
            int blockZ = voxelZ / BlockInformation.BlockSize;

            int blockVoxelX = voxelX % BlockInformation.BlockSize;
            int blockVoxelY = voxelY % BlockInformation.BlockSize;
            int blockVoxelZ = voxelZ % BlockInformation.BlockSize;

            BlockInformation block = worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockInfo;
            return blockList.BelongsToAnimation(block) ? 0 : block[blockVoxelX, blockVoxelY, blockVoxelZ];
        }

        public RenderStatistics RenderChunks(ref Matrix4 viewProjectionMatrix, Camera camera)
        {
            return renderBox.Render(ref viewProjectionMatrix, camera, -1);
        }

        #region Private helper methods
        /// <summary>
        /// Gets the offset into the game world blocks array for the block at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlockOffset(int x, int y, int z)
        {
            CheckConstraints(x, y, z, blockSize);
            return (x * blockSize.Z + z) * blockSize.Y + y; // y-axis major for speed
        }

        /// <summary>
        /// Gets the offset into the game world chunks array for the chunk at the specified location
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetChunkOffset(int x, int y, int z)
        {
            CheckConstraints(x, y, z, chunkSize);
            return (x * chunkSize.Z + z) * chunkSize.Y + y; // y-axis major for speed
        }

        /// <summary>
        /// Throws an ArgumentOutOfRangeException if the specified location is outside the bounds of the specified size.
        /// This method is not compiled into release builds.
        /// </summary>
        [Conditional("DEBUG")]
        private static void CheckConstraints(int x, int y, int z, Vector3i size)
        {
            if (x < 0 || x >= size.X)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= size.Y)
                throw new ArgumentOutOfRangeException("y");
            if (z < 0 || z >= size.Z)
                throw new ArgumentOutOfRangeException("z");
        }
        #endregion

        #region Block class
        /// <summary>
        /// Represents one block in the game world
        /// </summary>
        private struct Block
        {
            /// <summary>
            /// Information about the block
            /// </summary>
            public BlockInformation BlockInfo;
            
            /// <summary>
            /// List of lights that affect this block
            /// </summary>
            public readonly List<LightInfo> Lights;

            public Color3f BlockLight;

            /// <summary>
            /// Information about the state of the block
            /// </summary>
            public BlockState State;

            public Block(BlockInformation blockInfo)
            {
                BlockInfo = blockInfo;
                Lights = new List<LightInfo>(10);
                State = new BlockState();
                BlockLight = new Color3f();
            }
        }
        #endregion

        private sealed class GameWorldBox : WorldBoundingBox, IDisposable
        {
            private const int ChunkVoxelSize = GameWorldVoxelChunk.ChunkVoxelSize;
            private const int FarTopLeft = 0;
            private const int FarTopRight = 1;
            private const int FarBottomLeft = 2;
            private const int FarBottomRight = 3;
            private const int NearTopLeft = 4;
            private const int NearTopRight = 5;
            private const int NearBottomLeft = 6;
            private const int NearBottomRight = 7;

            private readonly GameWorldBox[] children = new GameWorldBox[8];
            private readonly GameWorldVoxelChunk chunk;
            private readonly int depth;

            private IVertexDataCollection debugBoxOutLine;

            public GameWorldBox(GameWorld gameWorld) :
                this(gameWorld, Vector3.Zero, new Vector3(gameWorld.ChunkSize.X * GameWorldVoxelChunk.ChunkVoxelSize,
                gameWorld.ChunkSize.Y * GameWorldVoxelChunk.ChunkVoxelSize, gameWorld.ChunkSize.Z * GameWorldVoxelChunk.ChunkVoxelSize), 0)
            {
            }

            private GameWorldBox(GameWorld gameWorld, Vector3 minPoint, Vector3 maxPoint, int depth) : base(minPoint, maxPoint)
            {
                this.depth = depth;
                Debug.Assert((int)(maxPoint.X - minPoint.X) % ChunkVoxelSize == 0, "Game world box should fit an even number of chunks inside on the x-axis");
                Debug.Assert((int)(maxPoint.Y - minPoint.Y) % ChunkVoxelSize == 0, "Game world box should fit an even number of chunks inside on the y-axis");
                Debug.Assert((int)(maxPoint.Z - minPoint.Z) % ChunkVoxelSize == 0, "Game world box should fit an even number of chunks inside on the z-axis");

                Debug.Assert(maxPoint.X - minPoint.X > 0, "X-axis: min is greater than or equal to max");
                Debug.Assert(maxPoint.Y - minPoint.Y > 0, "Y-axis: min is greater than or equal to max");
                Debug.Assert(maxPoint.Z - minPoint.Z > 0, "Z-axis: min is greater than or equal to max");

                int boxSizeX = (int)(maxPoint.X - minPoint.X);
                int boxSizeY = (int)(maxPoint.Y - minPoint.Y);
                int boxSizeZ = (int)(maxPoint.Z - minPoint.Z);
                if (boxSizeX < ChunkVoxelSize * 2 && boxSizeY < ChunkVoxelSize * 2 && boxSizeZ < ChunkVoxelSize * 2)
                {
                    chunk = gameWorld.GetChunk((int)(minPoint.X / ChunkVoxelSize), (int)(minPoint.Y / ChunkVoxelSize), (int)(minPoint.Z / ChunkVoxelSize));
                    return;
                }

                Vector3 boxCenter = new Vector3((int)(minPoint.X + maxPoint.X) / 2 / ChunkVoxelSize * ChunkVoxelSize,
                    (int)(minPoint.Y + maxPoint.Y) / 2 / ChunkVoxelSize * ChunkVoxelSize,
                    (int)(minPoint.Z + maxPoint.Z) / 2 / ChunkVoxelSize * ChunkVoxelSize);

                bool hasAvailableX = true;
                bool hasAvailableY = true;
                bool hasAvailableZ = true;
                if (boxCenter.X - minPoint.X < ChunkVoxelSize)
                {
                    boxCenter.X = maxPoint.X;
                    hasAvailableX = false;
                }

                if (boxCenter.Y - minPoint.Y < ChunkVoxelSize)
                {
                    boxCenter.Y = maxPoint.Y;
                    hasAvailableY = false;
                }

                if (boxCenter.Z - minPoint.Z < ChunkVoxelSize)
                {
                    boxCenter.Z = maxPoint.Z;
                    hasAvailableZ = false;
                }

                int childDepth = depth + 1;
                children[FarBottomLeft] = new GameWorldBox(gameWorld, minPoint, boxCenter, childDepth);
                if (hasAvailableX)
                    children[FarBottomRight] = new GameWorldBox(gameWorld, new Vector3(boxCenter.X, minPoint.Y, minPoint.Z), new Vector3(maxPoint.X, boxCenter.Y, boxCenter.Z), childDepth);
                if (hasAvailableY)
                    children[FarTopLeft] = new GameWorldBox(gameWorld, new Vector3(minPoint.X, boxCenter.Y, minPoint.Z), new Vector3(boxCenter.X, maxPoint.Y, boxCenter.Z), childDepth);
                if (hasAvailableX && hasAvailableY)
                    children[FarTopRight] = new GameWorldBox(gameWorld, new Vector3(boxCenter.X, boxCenter.Y, minPoint.Z), new Vector3(maxPoint.X, maxPoint.Y, boxCenter.Z), childDepth);

                if (hasAvailableZ)
                {
                    children[NearBottomLeft] = new GameWorldBox(gameWorld, new Vector3(minPoint.X, minPoint.Y, boxCenter.Z), new Vector3(boxCenter.X, boxCenter.Y, maxPoint.Z), childDepth);
                    if (hasAvailableX)
                        children[NearBottomRight] = new GameWorldBox(gameWorld, new Vector3(boxCenter.X, minPoint.Y, boxCenter.Z), new Vector3(maxPoint.X, boxCenter.Y, maxPoint.Z), childDepth);
                    if (hasAvailableY)
                        children[NearTopLeft] = new GameWorldBox(gameWorld, new Vector3(minPoint.X, boxCenter.Y, boxCenter.Z), new Vector3(boxCenter.X, maxPoint.Y, maxPoint.Z), childDepth);
                    if (hasAvailableX && hasAvailableY)
                        children[NearTopRight] = new GameWorldBox(gameWorld, boxCenter, maxPoint, depth + 1);
                }
            }

            public void Dispose()
            {
                if (debugBoxOutLine != null)
                    debugBoxOutLine.Dispose();

                if (chunk != null)
                    chunk.Dispose();

                GameWorldBox[] childrenLocal = children;
                for (int i = 0; i < childrenLocal.Length; i++)
                {
                    GameWorldBox childBox = childrenLocal[i];
                    if (childBox != null)
                        childBox.Dispose();
                }
            }

            public RenderStatistics Render(ref Matrix4 viewProjectionMatrix, Camera camera, int locationInParent)
            {
                if (debugBoxOutLine == null)
                {
                    Color4b color = GetColorForLocation(locationInParent);
                    LargeMeshBuilder builder = new LargeMeshBuilder(8, 24);
                    builder.StartNewMesh();
                    int v1 = builder.Add((short)(minPoint.X + depth), (short)(minPoint.Y + depth), (short)(minPoint.Z + depth), color);
                    int v2 = builder.Add((short)(maxPoint.X - depth), (short)(minPoint.Y + depth), (short)(minPoint.Z + depth), color);
                    int v3 = builder.Add((short)(maxPoint.X - depth), (short)(maxPoint.Y - depth), (short)(minPoint.Z + depth), color);
                    int v4 = builder.Add((short)(minPoint.X + depth), (short)(maxPoint.Y - depth), (short)(minPoint.Z + depth), color);
                    int v5 = builder.Add((short)(minPoint.X + depth), (short)(minPoint.Y + depth), (short)(maxPoint.Z - depth), color);
                    int v6 = builder.Add((short)(maxPoint.X - depth), (short)(minPoint.Y + depth), (short)(maxPoint.Z - depth), color);
                    int v7 = builder.Add((short)(maxPoint.X - depth), (short)(maxPoint.Y - depth), (short)(maxPoint.Z - depth), color);
                    int v8 = builder.Add((short)(minPoint.X + depth), (short)(maxPoint.Y - depth), (short)(maxPoint.Z - depth), color);

                    // far plane outline
                    builder.AddIndex(v1);
                    builder.AddIndex(v2);
                    builder.AddIndex(v2);
                    builder.AddIndex(v3);
                    builder.AddIndex(v3);
                    builder.AddIndex(v4);
                    builder.AddIndex(v4);
                    builder.AddIndex(v1);

                    // near plane outline
                    builder.AddIndex(v5);
                    builder.AddIndex(v6);
                    builder.AddIndex(v6);
                    builder.AddIndex(v7);
                    builder.AddIndex(v7);
                    builder.AddIndex(v8);
                    builder.AddIndex(v8);
                    builder.AddIndex(v5);

                    // Other outline lines
                    builder.AddIndex(v1);
                    builder.AddIndex(v5);
                    builder.AddIndex(v2);
                    builder.AddIndex(v6);
                    builder.AddIndex(v3);
                    builder.AddIndex(v7);
                    builder.AddIndex(v4);
                    builder.AddIndex(v8);


                    debugBoxOutLine = builder.GetMesh();
                    builder.DropMesh();
                    debugBoxOutLine.Initialize();
                }

                IShaderProgram shader = ResourceManager.ShaderManager.GetShaderProgram("MainWorld");
                shader.Bind();

                shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionMatrix);

                TiVEController.Backend.Draw(PrimitiveType.Lines, debugBoxOutLine);

                RenderStatistics stats = new RenderStatistics(1, 12, 0, 0);
                GameWorldBox[] childrenLocal = children;
                for (int i = 0; i < childrenLocal.Length; i++)
                {
                    GameWorldBox childBox = childrenLocal[i];
                    if (childBox != null)
                    {
                        if (camera.BoxInView(childBox, depth <= 10))
                            stats = stats + childBox.Render(ref viewProjectionMatrix, camera, i);
                    }
                }

                return stats;
            }

            private static Color4b GetColorForLocation(int location)
            {
                switch (location)
                {
                    case FarTopLeft: return new Color4b(255, 0, 0, 255);
                    case FarTopRight: return new Color4b(0, 255, 0, 255);
                    case FarBottomLeft: return new Color4b(0, 0, 255, 255);
                    case FarBottomRight: return new Color4b(255, 255, 0, 255);
                    case NearTopLeft: return new Color4b(0, 255, 255, 255);
                    case NearTopRight: return new Color4b(255, 0, 255, 255);
                    case NearBottomLeft: return new Color4b(100, 255, 50, 255);
                    case NearBottomRight: return new Color4b(50, 100, 255, 255);
                    default: return new Color4b(255, 255, 255, 255);
                }
            }
        }
    }
}
