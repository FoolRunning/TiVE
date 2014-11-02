using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Lighting;
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

        private readonly Block[] worldBlocks;
        private readonly Color3f ambientLight;
        private readonly BlockList blockList;
        private readonly ChunkRenderTree renderTree;

        internal GameWorld(int blockSizeX, int blockSizeY, int blockSizeZ, BlockList blockList)
        {
            this.blockList = blockList;

            blockSize = new Vector3i(blockSizeX, blockSizeY, blockSizeZ);
            voxelSize = new Vector3i(blockSizeX * BlockInformation.VoxelSize, blockSizeY * BlockInformation.VoxelSize, blockSizeZ * BlockInformation.VoxelSize);

            worldBlocks = new Block[blockSizeX * blockSizeY * blockSizeZ];
            for (int i = 0; i < worldBlocks.Length; i++)
                worldBlocks[i] = new Block(BlockInformation.Empty);
            
            ambientLight = new Color3f(0.01f, 0.01f, 0.01f);

            renderTree = new ChunkRenderTree(this);
        }

        public void Dispose()
        {
            renderTree.Dispose();
        }

        /// <summary>
        /// Gets the voxel size of the game world
        /// </summary>
        public Vector3i VoxelSize
        {
            get { return voxelSize; }
        }

        public ChunkRenderTree RenderTree
        {
            get { return renderTree; }
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

            int blockX = voxelX / BlockInformation.VoxelSize;
            int blockY = voxelY / BlockInformation.VoxelSize;
            int blockZ = voxelZ / BlockInformation.VoxelSize;

            //return ambientLight + worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockLight;
            List<LightInfo> blockLights = worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].Lights;
            for (int i = 0; i < blockLights.Count; i++)
            {
                LightInfo lightInfo = blockLights[i];
                color += lightInfo.Light.Color * LightUtils.GetLightPercentage(lightInfo, voxelX, voxelY, voxelZ);
            }
            return color;
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

            int blockX = voxelX / BlockInformation.VoxelSize;
            int blockY = voxelY / BlockInformation.VoxelSize;
            int blockZ = voxelZ / BlockInformation.VoxelSize;

            int blockVoxelX = voxelX % BlockInformation.VoxelSize;
            int blockVoxelY = voxelY % BlockInformation.VoxelSize;
            int blockVoxelZ = voxelZ % BlockInformation.VoxelSize;

            BlockInformation block = worldBlocks[GetBlockOffset(blockX, blockY, blockZ)].BlockInfo;
            return blockList.BelongsToAnimation(block) ? 0 : block[blockVoxelX, blockVoxelY, blockVoxelZ];
        }

        public RenderStatistics RenderChunks(ref Matrix4 viewProjectionMatrix, Camera camera)
        {
            return renderTree.Render(ref viewProjectionMatrix, camera);
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
    }
}
