﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class BlockImpl : Block
    {
        public static readonly BlockImpl Empty = new BlockImpl("Empty");

        private readonly List<IBlockComponent> components = new List<IBlockComponent>();
        private readonly uint[] voxels = new uint[VoxelSize * VoxelSize * VoxelSize];
        private string name;
        private int totalVoxels = -1;
    
        private BlockImpl(BlockImpl toCopy, string newBlockName) : this(newBlockName)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
            components.AddRange(toCopy.components);
        }

        public BlockImpl(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
        }

        internal uint[] VoxelsArray
        {
            get { return voxels; }
        }

        public override string BlockName  
        {
            get { return name; }
        }

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
        public override uint this[int x, int y, int z]
        {
            get { return voxels[GetOffset(x, y, z)]; }
            set 
            { 
                voxels[GetOffset(x, y, z)] = value;
                totalVoxels = -1; // Need to recalculate
            }
        }

        public override void AddComponent(IBlockComponent component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            if (components.Exists(c => c.GetType() == component.GetType()))
                throw new ArgumentException(string.Format("Component of type {0} can not be added more then once", component.GetType()));

            components.Add(component);
        }

        public override void RemoveComponent<T>()
        {
            components.RemoveAll(c => c.GetType() == typeof(T));
        }

        public override bool HasComponent(IBlockComponent component)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] == component)
                    return true;
            }
            return false;
        }

        public override bool HasComponent<T>()
        {
            Type tType = typeof(T);
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == tType)
                    return true;
            }
            return false;
        }

        public override T GetComponent<T>()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T)
                    return (T)components[i];
            }
            return default(T);
        }

        public override Block CreateRotated(BlockRotation rotation)
        {
            if (rotation == BlockRotation.NotRotated)
                return this;

            BlockImpl rotatedBlock = new BlockImpl(this, BlockName + "R" + (int)rotation);
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

        internal void SetName(string newName)
        {
            name = newName;
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
