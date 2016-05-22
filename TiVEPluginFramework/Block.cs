using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
    public sealed class Block : ITiVESerializable
    {
        #region Member variables/constants
        public static readonly Guid ID = new Guid("105FC0BF-E194-46BC-8ED8-61942721CC7F");

        /// <summary>Number of voxels that make up a block on each axis (must be a power-of-two)</summary>
        public const int VoxelSize = 32;
        /// <summary>Number of bit shifts neccessary on a number to produce the number of voxels on each axis.
        /// This allows us to quickly multiply or divide a value by the voxel size by bitshifting</summary>
        public const int VoxelSizeBitShift = 5;
        private const byte SerializedFileVersion = 1;

        public static readonly Block Empty = new Block("3ptEe");

        private readonly List<IBlockComponent> components = new List<IBlockComponent>();
        private readonly Voxel[] voxels = new Voxel[VoxelSize * VoxelSize * VoxelSize];
        private readonly string name;
        private int totalVoxels = -1;
        #endregion

        #region Constructors
        private Block(Block toCopy, string newBlockName) : this(newBlockName)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
            components.AddRange(toCopy.components);
        }

        public Block(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNewException("Block");

            name = reader.ReadString();

            for (int i = 0; i < voxels.Length; i++)
                voxels[i] = new Voxel(reader);

            int componentCount = reader.ReadInt32();
            for (int i = 0; i < componentCount; i++)
                components.Add(TiVESerializer.Deserialize<IBlockComponent>(reader));
        }

        public Block(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;
            
            for (int i = 0; i < voxels.Length; i++)
                voxels[i] = Voxel.Empty;
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets/sets the voxel at the specified location
        /// </summary>
        public Voxel this[int x, int y, int z]
        {
            get { return voxels[GetOffset(x, y, z)]; }
            set
            {
                voxels[GetOffset(x, y, z)] = value;
                totalVoxels = -1; // Need to recalculate
            }
        }

        internal Voxel[] VoxelsArray
        {
            get { return voxels; }
        }

        internal int TotalVoxels
        {
            get
            {
                if (totalVoxels == -1)
                    totalVoxels = GetCountOfNonEmptyVoxels();
                return totalVoxels;
            }
        }
        #endregion

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
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
        public void AddComponent(IBlockComponent component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            if (components.Exists(c => c.GetType() == component.GetType()))
                throw new ArgumentException(string.Format("Component of type {0} can not be added more then once", component.GetType()));

            components.Add(component);
        }

        public void RemoveComponent<T>() where T : class, IBlockComponent
        {
            components.RemoveAll(c => c is T);
        }

        public bool HasComponent<T>() where T : class, IBlockComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T)
                    return true;
            }
            return false;
        }

        public T GetComponent<T>() where T : class, IBlockComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                T component = components[i] as T;
                if (component != null)
                    return component;
            }
            return null;
        }

        public Block CreateRotated(BlockRotation rotation)
        {
            if (rotation == BlockRotation.NotRotated)
                return this;

            Block rotatedBlock = new Block(this, Name + "R" + (int)rotation);
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

        #region Overrides of Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Private helper methods
        private int GetCountOfNonEmptyVoxels()
        {
            int count = 0;
            for (int i = 0; i < voxels.Length; i++) // For loop for speed
            {
                if (voxels[i] != Voxel.Empty)
                    count++;
            }
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetOffset(int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < VoxelSize);
            Debug.Assert(y >= 0 && y < VoxelSize);
            Debug.Assert(z >= 0 && z < VoxelSize);

            return (((z << VoxelSizeBitShift) + x) << VoxelSizeBitShift) + y; // y-axis major for speed
        }
        #endregion
    }

    public interface IBlockComponent : ITiVESerializable
    {
    }
}
