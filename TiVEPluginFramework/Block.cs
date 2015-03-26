using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class Block
    {
        /// <summary>Number of voxels that make up a block on each axis</summary>
        public const int VoxelSize = 16;

        public static readonly Block Empty = new Block("Empty");

        private readonly List<IBlockComponent> components = new List<IBlockComponent>();
        private readonly uint[] voxels = new uint[VoxelSize * VoxelSize * VoxelSize];
        private int totalVoxels = -1;
    
        private Block(Block toCopy, string newBlockName) : this(newBlockName)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
            components.AddRange(toCopy.components);
        }

        public Block(string blockName)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            BlockName = blockName;
        }

        internal uint[] VoxelsArray
        {
            get { return voxels; }
        }

        public string BlockName  { get; internal set; }

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

        public void AddComponent(IBlockComponent component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            if (components.Exists(c => c.GetType() == component.GetType()))
                throw new ArgumentException(string.Format("Component of type {0} can not be added more then once", component.GetType()));

            components.Add(component);
        }

        public void RemoveComponent<T>() where T : IBlockComponent
        {
            components.RemoveAll(c => c.GetType() == typeof(T));
        }

        public bool HasComponent<T>() where T : IBlockComponent
        {
            Type tType = typeof(T);
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == tType)
                    return true;
            }
            return false;
        }

        public T GetComponent<T>() where T : IBlockComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T)
                    return (T)components[i];
            }
            return default(T);
        }

        [PublicAPI]
        public Block CreateRotated(BlockRotation rotation)
        {
            if (rotation == BlockRotation.NotRotated)
                return this;

            Block rotatedBlock = new Block(this, BlockName + "R" + (int)rotation);
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

        public override string ToString()
        {
            return BlockName;
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

    public interface IBlockComponent
    {
    }
}
