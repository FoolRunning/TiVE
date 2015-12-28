using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class BlockImpl : Block
    {
        #region Member variables/constants
        public static readonly BlockImpl Empty = new BlockImpl("Empty");

        private readonly List<IBlockComponent> components = new List<IBlockComponent>();
        private readonly Voxel[] voxels = new Voxel[VoxelSize * VoxelSize * VoxelSize];
        private string name;
        private int totalVoxels = -1;
        #endregion

        #region Constructors
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
        #endregion

        #region Properties
        public Voxel[] VoxelsArray
        {
            get { return voxels; }
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
        #endregion

        #region Implementation of Block
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets/sets the voxel at the specified location
        /// </summary>
        public override Voxel this[int x, int y, int z]
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
            Type tType = typeof(T);
            components.RemoveAll(c => c.GetType() == tType);
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
                T component = components[i] as T;
                if (component != null)
                    return component;
            }
            return null;
        }

        public override Block CreateRotated(BlockRotation rotation)
        {
            if (rotation == BlockRotation.NotRotated)
                return this;

            BlockImpl rotatedBlock = new BlockImpl(this, Name + "R" + (int)rotation);
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
        #endregion

        #region Public methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Voxel GetVoxelFast(int x, int y, int z)
        {
            return voxels[GetOffset(x, y, z)];
        }

        public void SetName(string newName)
        {
            name = newName;
        }
        #endregion

        #region Overrides of Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Private helper methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetOffset(int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < VoxelSize);
            Debug.Assert(y >= 0 && y < VoxelSize);
            Debug.Assert(z >= 0 && z < VoxelSize);

            return (z * VoxelSize + x) * VoxelSize + y; // y-axis major for speed
        }
        #endregion
    }
}
