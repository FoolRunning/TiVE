using System;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    #region LODLevel enum
    [PublicAPI]
    public enum LODLevel : byte
    {
        V32 = 0,
        V16 = 1,
        V8 = 2,
        V4 = 3,
        NumOfLevels = 4,
        NotSet = 255
    }
    #endregion

    [PublicAPI]
    public abstract class BlockLOD
    {
        #region Constants/Member variables
        private const byte SerializedFileVersion = 1;

        protected readonly Voxel[] voxels;
        #endregion

        #region Constructors
        protected BlockLOD(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNewException("BlockLOD");

            voxels = new Voxel[VoxelAxisSize * VoxelAxisSize * VoxelAxisSize];
            for (int i = 0; i < voxels.Length; i++)
                voxels[i] = new Voxel(reader);
        }

        protected BlockLOD(bool initializeVoxels)
        {
            voxels = new Voxel[VoxelAxisSize * VoxelAxisSize * VoxelAxisSize];
            
            if (initializeVoxels)
            {
                for (int i = 0; i < voxels.Length; i++)
                    voxels[i] = Voxel.Empty;
            }
        }
        #endregion

        #region Properties
        public abstract int VoxelAxisSize { get; }

        public abstract int VoxelAxisSizeBitShift { get; }

        //public int MagicModulusNumber
        //{
        //    get { return (1 << VoxelAxisSizeBitShift) - 1; }
        //}

        public int RenderedVoxelSize
        {
            get { return BlockLOD32.VoxelSize / VoxelAxisSize; }
        }

        internal Voxel[] VoxelsArray
        {
            get { return voxels; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the voxel at the specified location
        /// </summary>
        public abstract Voxel VoxelAt(int x, int y, int z);

        /// <summary>
        /// Sets the voxel at the specified location
        /// </summary>
        public abstract void VoxelAt(int x, int y, int z, Voxel vox);

        public static int GetMagicModulusNumber(LODLevel detailLevel)
        {
            switch (detailLevel)
            {
                case LODLevel.V32: return (1 << BlockLOD32.VoxelSizeBitShift) - 1;
                case LODLevel.V16: return (1 << BlockLOD16.VoxelSizeBitShift) - 1;
                case LODLevel.V8: return (1 << BlockLOD8.VoxelSizeBitShift) - 1;
                case LODLevel.V4: return (1 << BlockLOD4.VoxelSizeBitShift) - 1;
                default: throw new ArgumentException("detailLevel invalid: " + detailLevel);
            }
        }

        public static int GetVoxelSize(LODLevel detailLevel)
        {
            switch (detailLevel)
            {
                case LODLevel.V32: return BlockLOD32.VoxelSize;
                case LODLevel.V16: return BlockLOD16.VoxelSize;
                case LODLevel.V8: return BlockLOD8.VoxelSize;
                case LODLevel.V4: return BlockLOD4.VoxelSize;
                default: throw new ArgumentException("detailLevel invalid: " + detailLevel);
            }
        }

        public static int GetVoxelSizeBitShift(LODLevel detailLevel)
        {
            switch (detailLevel)
            {
                case LODLevel.V32: return BlockLOD32.VoxelSizeBitShift;
                case LODLevel.V16: return BlockLOD16.VoxelSizeBitShift;
                case LODLevel.V8: return BlockLOD8.VoxelSizeBitShift;
                case LODLevel.V4: return BlockLOD4.VoxelSizeBitShift;
                default: throw new ArgumentException("detailLevel invalid: " + detailLevel);
            }
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(SerializedFileVersion);
            for (int i = 0; i < voxels.Length; i++)
                voxels[i].SaveTo(writer);
        }
        #endregion
    }

    #region BlockLOD32 class
    [PublicAPI]
    public sealed class BlockLOD32 : BlockLOD, ITiVESerializable
    {
        #region Member variables
        public static readonly Guid ID = new Guid("239552B8-F4DB-42FE-9B3F-CCF43B87E1E2");
        public const int VoxelSize = 32;
        public const int VoxelSizeBitShift = 5;
        public const int MagicModulusNumber = VoxelSize - 1;

        private int totalVoxels = -1;
        #endregion

        #region Constructors
        public BlockLOD32(BinaryReader reader) : base(reader)
        {
        }

        public BlockLOD32(BlockLOD32 toCopy) : base(false)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
        }

        public BlockLOD32() : base(true)
        {
        }
        #endregion

        #region Implementation of BlockLOD
        public override int VoxelAxisSize
        {
            get { return VoxelSize; }
        }

        public override int VoxelAxisSizeBitShift
        {
            get { return VoxelSizeBitShift; }
        }

        public override Voxel VoxelAt(int x, int y, int z)
        {
            return voxels[GetArrayOffset(x, y, z)];
        }

        public override void VoxelAt(int x, int y, int z, Voxel vox)
        {
            voxels[GetArrayOffset(x, y, z)] = vox;
        }
        #endregion

        public Voxel this[int x, int y, int z]
        {
            get { return voxels[GetArrayOffset(x, y, z)]; }
            set
            {
                voxels[GetArrayOffset(x, y, z)] = value;
                totalVoxels = -1;
            }
        }

        #region Other properties
        /// <summary>
        /// Gets the number of visible (non-empty) voxels
        /// </summary>
        internal int TotalVoxels
        {
            get
            {
                if (totalVoxels == -1)
                    totalVoxels = MiscUtils.GetCountOfNonEmptyVoxels(voxels);
                return totalVoxels;
            }
        }
        #endregion

        #region Private helper methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetArrayOffset(int x, int y, int z)
        {
            MiscUtils.CheckConstraints(x, y, z, VoxelSize);
            return (((z * VoxelSize) + x) * VoxelSize) + y; // y-axis major for speed
        }
        #endregion
    }
    #endregion

    #region BlockLOD16 class
    [PublicAPI]
    public sealed class BlockLOD16 : BlockLOD, ITiVESerializable
    {
        #region Member variables
        public static readonly Guid ID = new Guid("113737C6-3E87-44A3-9190-90406D71DF6F");
        public const int VoxelSize = 16;
        public const int VoxelSizeBitShift = 4;
        public const int MagicModulusNumber = VoxelSize - 1;
        #endregion

        #region Constructors
        public BlockLOD16(BinaryReader reader) : base(reader)
        {
        }

        public BlockLOD16(BlockLOD16 toCopy) : base(false)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
        }

        public BlockLOD16() : base(true)
        {
        }
        #endregion

        #region Implementation of BlockLOD
        public override int VoxelAxisSize
        {
            get { return VoxelSize; }
        }

        public override int VoxelAxisSizeBitShift
        {
            get { return VoxelSizeBitShift; }
        }

        public override Voxel VoxelAt(int x, int y, int z)
        {
            return voxels[GetArrayOffset(x, y, z)];
        }

        public override void VoxelAt(int x, int y, int z, Voxel vox)
        {
            voxels[GetArrayOffset(x, y, z)] = vox;
        }
        #endregion

        public Voxel this[int x, int y, int z]
        {
            get { return voxels[GetArrayOffset(x, y, z)]; }
            set { voxels[GetArrayOffset(x, y, z)] = value; }
        }

        #region Private helper methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetArrayOffset(int x, int y, int z)
        {
            MiscUtils.CheckConstraints(x, y, z, VoxelSize);
            return (((z * VoxelSize) + x) * VoxelSize) + y; // y-axis major for speed
        }
        #endregion
    }
    #endregion

    #region BlockLOD8 class
    [PublicAPI]
    public sealed class BlockLOD8 : BlockLOD, ITiVESerializable
    {
        #region Member variables
        public static readonly Guid ID = new Guid("9AFDDACC-04A2-4742-A7E1-0BD17AEF6A36");
        public const int VoxelSize = 8;
        public const int VoxelSizeBitShift = 3;
        public const int MagicModulusNumber = VoxelSize - 1;
        #endregion

        #region Constructors
        public BlockLOD8(BinaryReader reader) : base(reader)
        {
        }

        public BlockLOD8(BlockLOD8 toCopy) : base(false)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
        }

        public BlockLOD8() : base(true)
        {
        }
        #endregion

        #region Implementation of BlockLOD
        public override int VoxelAxisSize
        {
            get { return VoxelSize; }
        }

        public override int VoxelAxisSizeBitShift
        {
            get { return VoxelSizeBitShift; }
        }

        public override Voxel VoxelAt(int x, int y, int z)
        {
            return voxels[GetArrayOffset(x, y, z)];
        }

        public override void VoxelAt(int x, int y, int z, Voxel vox)
        {
            voxels[GetArrayOffset(x, y, z)] = vox;
        }
        #endregion

        public Voxel this[int x, int y, int z]
        {
            get { return voxels[GetArrayOffset(x, y, z)]; }
            set { voxels[GetArrayOffset(x, y, z)] = value; }
        }

        #region Private helper methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetArrayOffset(int x, int y, int z)
        {
            MiscUtils.CheckConstraints(x, y, z, VoxelSize);
            return (((z * VoxelSize) + x) * VoxelSize) + y; // y-axis major for speed
        }
        #endregion
    }
    #endregion

    #region BlockLOD4 class
    [PublicAPI]
    public sealed class BlockLOD4 : BlockLOD, ITiVESerializable
    {
        #region Member variables
        public static readonly Guid ID = new Guid("EEB67B04-5116-4919-95AD-D6B52AAC1E27");
        public const int VoxelSize = 4;
        public const int VoxelSizeBitShift = 2;
        public const int MagicModulusNumber = VoxelSize - 1;
        #endregion

        #region Constructors
        public BlockLOD4(BinaryReader reader) : base(reader)
        {
        }

        public BlockLOD4(BlockLOD4 toCopy) : base(false)
        {
            Array.Copy(toCopy.voxels, voxels, voxels.Length);
        }

        public BlockLOD4() : base(true)
        {
        }
        #endregion

        #region Implementation of BlockLOD
        public override int VoxelAxisSize
        {
            get { return VoxelSize; }
        }

        public override int VoxelAxisSizeBitShift
        {
            get { return VoxelSizeBitShift; }
        }

        public override Voxel VoxelAt(int x, int y, int z)
        {
            return voxels[GetArrayOffset(x, y, z)];
        }

        public override void VoxelAt(int x, int y, int z, Voxel vox)
        {
            voxels[GetArrayOffset(x, y, z)] = vox;
        }
        #endregion

        public Voxel this[int x, int y, int z]
        {
            get { return voxels[GetArrayOffset(x, y, z)]; }
            set { voxels[GetArrayOffset(x, y, z)] = value; }
        }

        #region Private helper methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetArrayOffset(int x, int y, int z)
        {
            MiscUtils.CheckConstraints(x, y, z, VoxelSize);
            return (((z * VoxelSize) + x) * VoxelSize) + y; // y-axis major for speed
        }
        #endregion
    }
    #endregion
}
