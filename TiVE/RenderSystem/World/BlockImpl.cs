using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class BlockImpl : Block
    {
        #region Member variables/constants
        private const byte SerializedFileVersion = 1;

        public static readonly Guid ID = new Guid("105FC0BF-E194-46BC-8ED8-61942721CC7F");
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

        public BlockImpl(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNew("Block");

            name = reader.ReadString();

            for (int i = 0; i < voxels.Length; i++)
                voxels[i] = new Voxel(reader);

            int componentCount = reader.ReadInt32();
            for (int i = 0; i < componentCount; i++)
                components.Add(TiVESerializer.Deserialize<IBlockComponent>(reader));
        }

        public BlockImpl(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
            
            for (int i = 0; i < voxels.Length; i++)
                voxels[i] = Voxel.Empty;
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
                    totalVoxels = voxels.Count(v => v != Voxel.Empty);
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

        public override void SaveTo(BinaryWriter writer)
        {
            writer.Write(SerializedFileVersion);
            writer.Write(name);
            for (int i = 0; i < voxels.Length; i++)
                voxels[i].SaveTo(writer);

            writer.Write(components.Count);
            foreach (IBlockComponent component in components)
                TiVESerializer.Serialize(component, writer);
        }
        #endregion

        #region Other public methods
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
