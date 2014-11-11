﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.TiVEPluginFramework.Particles;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class BlockInformation
    {
        /// <summary>Number of voxels that make up a block on each axis</summary>
        public const int VoxelSize = 9;

        public static readonly BlockInformation Empty = new BlockInformation("Empty");

        public readonly BlockInformation NextBlock;
        public readonly string BlockName;
        public readonly ParticleSystemInformation ParticleSystem;
        public readonly ILight Light;

        private readonly uint[] voxels = new uint[VoxelSize * VoxelSize * VoxelSize];
        private BlockInformation[] rotated;

        public BlockInformation(BlockInformation toCopy, string blockName, ParticleSystemInformation particleSystem = null, ILight light = null,
            BlockInformation nextBlock = null) : this(blockName, particleSystem, light, nextBlock)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
        }

        public BlockInformation(string blockName, ParticleSystemInformation particleSystem = null, ILight light = null, BlockInformation nextBlock = null)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            BlockName = blockName;
            ParticleSystem = particleSystem;
            Light = light;
            NextBlock = nextBlock;
        }

        internal uint[] VoxelsArray
        {
            get { return voxels; }
        }

        /// <summary>
        /// Gets/sets the voxel at the specified location
        /// </summary>
        public uint this[int x, int y, int z]
        {
            get { return voxels[GetOffset(x, y, z)]; }
            set { voxels[GetOffset(x, y, z)] = value; }
        }

        public BlockInformation Rotate(BlockRotation rotation)
        {
            if (rotation == BlockRotation.NotRotated)
                return this;

            if (rotated == null)
                rotated = new BlockInformation[4];
            int rotateIndex = (int)rotation;
            BlockInformation rotatedBlock = rotated[rotateIndex];
            if (rotatedBlock == null)
                rotated[rotateIndex] = rotatedBlock = CreateRotated(rotation);
            return rotatedBlock;
        }

        private BlockInformation CreateRotated(BlockRotation rotation)
        {
            BlockInformation rotatedBlock = new BlockInformation(this, BlockName + "R" + (int)rotation, ParticleSystem, Light);
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
