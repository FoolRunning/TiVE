using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.TiVEPluginFramework.Particles;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class BlockInformation
    {
        /// <summary>Number of voxels that make up a block on each axis</summary>
        public const int VoxelSize = 16;

        public static readonly BlockInformation Empty = new BlockInformation("Empty");

        private readonly uint[] voxels = new uint[VoxelSize * VoxelSize * VoxelSize];
        private BlockInformation[] rotated;
        private int totalVoxels = -1;

        private BlockInformation(BlockInformation toCopy, string newBlockName) : 
            this(newBlockName, toCopy.ParticleSystem, toCopy.Light)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
        }

        public BlockInformation(string blockName, ParticleSystemInformation particleSystem = null, ILight light = null, BlockInformation nextBlock = null, 
            bool isLit = true)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            BlockName = blockName;
            ParticleSystem = particleSystem;
            Light = light;
            NextBlock = nextBlock;
            IsLIt = isLit;
        }

        internal uint[] VoxelsArray
        {
            get { return voxels; }
        }

        public bool IsLIt { get; internal set; }
        public BlockInformation NextBlock { get; internal set; }
        public string BlockName  { get; internal set; }
        public ParticleSystemInformation ParticleSystem { get; internal set; }
        public ILight Light { get; internal set; }

        public int TotalVoxels
        {
            get
            {
                if (totalVoxels == -1)
                    totalVoxels = voxels.Count(v => v != 0);
                return totalVoxels;
            }
        }

        /// <summary>
        /// Gets/sets the voxel at the specified location
        /// </summary>
        public uint this[int x, int y, int z]
        {
            get { return voxels[GetOffset(x, y, z)]; }
            set 
            { 
                voxels[GetOffset(x, y, z)] = value;
                totalVoxels = -1; // Need to recalculate
            }
        }

        internal BlockInformation Rotate(BlockRotation rotation)
        {
            if (rotation == BlockRotation.NotRotated)
                return this;

            if (rotated == null)
                rotated = new BlockInformation[4];
            BlockInformation rotatedBlock = rotated[(int)rotation];
            if (rotatedBlock == null)
                rotated[(int)rotation] = rotatedBlock = CreateRotated(rotation);
            return rotatedBlock;
        }

        public override string ToString()
        {
            return BlockName;
        }

        private BlockInformation CreateRotated(BlockRotation rotation)
        {
            if (rotation == BlockRotation.NotRotated)
                return this;

            BlockInformation rotatedBlock = new BlockInformation(this, BlockName + "R" + (int)rotation);
            switch (rotation)
            {
                case BlockRotation.NinetyCCW:
                    for (int z = 0; z < VoxelSize; z++)
                    {
                        for (int x = 0; x < VoxelSize; x++)
                        {
                            for (int y = 0; y < VoxelSize; y++)
                                rotatedBlock[x, y, z] = this[y, VoxelSize - x - 1, z];
                        }
                    }
                    break;
                case BlockRotation.OneEightyCCW:
                    for (int z = 0; z < VoxelSize; z++)
                    {
                        for (int x = 0; x < VoxelSize; x++)
                        {
                            for (int y = 0; y < VoxelSize; y++)
                                rotatedBlock[x, y, z] = this[VoxelSize - x - 1, VoxelSize - y - 1, z];
                        }
                    }
                    break;
                case BlockRotation.TwoSeventyCCW:
                    for (int z = 0; z < VoxelSize; z++)
                    {
                        for (int x = 0; x < VoxelSize; x++)
                        {
                            for (int y = 0; y < VoxelSize; y++)
                                rotatedBlock[x, y, z] = this[VoxelSize - y - 1, x, z];
                        }
                    }
                    break;
            }
            return rotatedBlock;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetOffset(int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < VoxelSize);
            Debug.Assert(y >= 0 && y < VoxelSize);
            Debug.Assert(z >= 0 && z < VoxelSize);

            return (z * VoxelSize + x) * VoxelSize + y; // y-axis major for speed
        }
    }
}
