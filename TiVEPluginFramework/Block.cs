using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
    public sealed class Block : ITiVESerializable
    {
        #region Member variables/constants
        public static readonly Guid ID = new Guid("105FC0BF-E194-46BC-8ED8-61942721CC7F");

        private const byte SerializedFileVersion = 1;

        public static readonly Block Empty = new Block("3ptEe");
        public static readonly Block Missing = new Block("M1s51Ng");

        public readonly BlockLOD32 LOD32;
        public readonly BlockLOD16 LOD16;
        public readonly BlockLOD8 LOD8;
        public readonly BlockLOD4 LOD4;

        private readonly List<IBlockComponent> components = new List<IBlockComponent>();
        private readonly string name;
        #endregion

        #region Constructors
        static Block()
        {
            Voxel missingVoxel = new Voxel(255, 0, 0, 255, VoxelSettings.IgnoreLighting);
            for (int level = (int)LODLevel.V32; level < (int)LODLevel.NumOfLevels; level++)
            {
                BlockLOD voxelsLOD = Missing.GetLOD((LODLevel)level);
                for (int i = 0; i < voxelsLOD.VoxelsArray.Length; i++)
                    voxelsLOD.VoxelsArray[i] = missingVoxel;
            }
        }

        private Block(Block toCopy, string newBlockName) : this(newBlockName)
        {
            LOD32 = new BlockLOD32(toCopy.LOD32);
            LOD16 = new BlockLOD16(toCopy.LOD16);
            LOD8 = new BlockLOD8(toCopy.LOD8);
            LOD4 = new BlockLOD4(toCopy.LOD4);
            components.AddRange(toCopy.components);
        }

        public Block(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNewException("Block");

            name = reader.ReadString();

            LOD32 = new BlockLOD32(reader);
            LOD16 = new BlockLOD16(reader);
            LOD8 = new BlockLOD8(reader);
            LOD4 = new BlockLOD4(reader);

            int componentCount = reader.ReadInt32();
            for (int i = 0; i < componentCount; i++)
                components.Add(TiVESerializer.Deserialize<IBlockComponent>(reader));
        }

        public Block(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.name = name;

            LOD32 = new BlockLOD32();
            LOD16 = new BlockLOD16();
            LOD8 = new BlockLOD8();
            LOD4 = new BlockLOD4();
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the number of visible (non-empty) voxels when rendered at the highest detail level
        /// </summary>
        internal int TotalVoxels
        {
            get { return ((BlockLOD32)GetLOD(LODLevel.V32)).TotalVoxels; }
        }
        #endregion

        #region Implementation of ITiVESerializable
        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(SerializedFileVersion);
            writer.Write(name);

            GetLOD(LODLevel.V32).SaveTo(writer);
            GetLOD(LODLevel.V16).SaveTo(writer);
            GetLOD(LODLevel.V8).SaveTo(writer);
            GetLOD(LODLevel.V4).SaveTo(writer);

            writer.Write(components.Count);
            foreach (IBlockComponent component in components)
                TiVESerializer.Serialize(component, writer);
        }
        #endregion

        #region Other public methods
        public BlockLOD GetLOD(LODLevel detailLevel)
        {
            switch (detailLevel)
            {
                case LODLevel.V32: return LOD32;
                case LODLevel.V16: return LOD16;
                case LODLevel.V8: return LOD8;
                case LODLevel.V4: return LOD4;
                default: throw new ArgumentException("detailLevel invalid: " + detailLevel);
            }
        }

        public void GenerateLODLevels(LODLevel sourceLevel = LODLevel.V32, LODLevel start = LODLevel.V16)
        {
            BlockLOD sourceLOD = GetLOD(sourceLevel);
            for (int i = (int)start; i < (int)LODLevel.NumOfLevels; i++)
            {
                int voxelMult = 1 << (i - (int)start + 1);
                BlockLOD voxelsLOD = GetLOD((LODLevel)i);
                int voxelAxisSize = voxelsLOD.VoxelAxisSize;
                for (int z = 0; z < voxelAxisSize; z++)
                {
                    for (int x = 0; x < voxelAxisSize; x++)
                    {
                        for (int y = 0; y < voxelAxisSize; y++)
                            voxelsLOD.VoxelAt(x, y, z, GetLODVoxel(sourceLOD, x * voxelMult, y * voxelMult, z * voxelMult, voxelMult));
                    }
                }
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

        //public Block CreateRotated(BlockRotation rotation)
        //{
        //    if (rotation == BlockRotation.NotRotated)
        //        return this;

        //    Block rotatedBlock = new Block(this, Name + "R" + (int)rotation);
        //    switch (rotation)
        //    {
        //        case BlockRotation.NinetyCCW:
        //            for (int z = 0; z < VoxelSize; z++)
        //            {
        //                for (int x = 0; x < VoxelSize; x++)
        //                {
        //                    for (int y = 0; y < VoxelSize; y++)
        //                        rotatedBlock[x, y, z] = this[y, VoxelSize - x - 1, z];
        //                }
        //            }
        //            break;
        //        case BlockRotation.OneEightyCCW:
        //            for (int z = 0; z < VoxelSize; z++)
        //            {
        //                for (int x = 0; x < VoxelSize; x++)
        //                {
        //                    for (int y = 0; y < VoxelSize; y++)
        //                        rotatedBlock[x, y, z] = this[VoxelSize - x - 1, VoxelSize - y - 1, z];
        //                }
        //            }
        //            break;
        //        case BlockRotation.TwoSeventyCCW:
        //            for (int z = 0; z < VoxelSize; z++)
        //            {
        //                for (int x = 0; x < VoxelSize; x++)
        //                {
        //                    for (int y = 0; y < VoxelSize; y++)
        //                        rotatedBlock[x, y, z] = this[VoxelSize - y - 1, x, z];
        //                }
        //            }
        //            break;
        //    }
        //    return rotatedBlock;
        //}
        #endregion

        #region Overrides of Object
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Private helper methods
        private static Voxel GetLODVoxel(BlockLOD sourceLOD, int bvx, int bvy, int bvz, int voxelSize)
        {
            int voxelsFound = 0;
            int totalA = 0;
            int totalR = 0;
            int totalG = 0;
            int totalB = 0;
            int maxX = bvx + voxelSize;
            int maxY = bvy + voxelSize;
            int maxZ = bvz + voxelSize;
            VoxelSettings settings = VoxelSettings.None;
            for (int z = bvz; z < maxZ; z++)
            {
                for (int x = bvx; x < maxX; x++)
                {
                    for (int y = bvy; y < maxY; y++)
                    {
                        Voxel otherColor = sourceLOD.VoxelAt(x, y, z);
                        if (otherColor == Voxel.Empty)
                            continue;

                        voxelsFound++;
                        totalA += otherColor.A;
                        totalR += otherColor.R;
                        totalG += otherColor.G;
                        totalB += otherColor.B;
                        settings |= otherColor.Settings;
                    }
                }
            }

            if (voxelsFound == 0) // Prevent divide-by-zero
                return Voxel.Empty;

            if (voxelsFound < (voxelSize * voxelSize) / 4) // Less than 1/4 of the potential voxels on a single plane actually were set so make it empty
                return Voxel.Empty;
            return new Voxel((byte)(totalR / voxelsFound), (byte)(totalG / voxelsFound), (byte)(totalB / voxelsFound), (byte)(totalA / voxelsFound), settings);
        }
        #endregion
    }
}
