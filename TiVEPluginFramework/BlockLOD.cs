using System;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    #region CubeSides enumeration
    [Flags]
    public enum CubeSides : byte
    {
        None = 0,
        YPlus = 1 << 0,
        XMinus = 1 << 1,
        XPlus = 1 << 2,
        YMinus = 1 << 3,
        ZPlus = 1 << 4,
        ZMinus = 1 << 5,
        All = YPlus | XMinus | XPlus | YMinus | ZPlus | ZMinus,
    }
    #endregion

    #region LODLevel enum
    [PublicAPI]
    public enum LODLevel
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
    public abstract class BlockLOD : IVoxelProvider
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

        public int RenderedVoxelSize => BlockLOD32.VoxelSize / VoxelAxisSize;

        internal Voxel[] VoxelsArray => voxels;
        #endregion

        #region Implementation of IVoxelProvider
        public Vector3i VoxelCount => new Vector3i(VoxelAxisSize, VoxelAxisSize, VoxelAxisSize);
        
        public abstract Voxel this[int x, int y, int z] { get; set; }
        #endregion

        #region Public methods
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
        public static readonly Guid ID = new Guid("239552B8-F4DB-42FE-9B3F-CCF43B87E1E2");
        public const int VoxelSize = 32;
        public const int VoxelSizeBitShift = 5;
        public const int MagicModulusNumber = VoxelSize - 1;

        private int totalVoxels = -1;
        
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

        public override int VoxelAxisSize => VoxelSize;

        public override int VoxelAxisSizeBitShift => VoxelSizeBitShift;

        public override Voxel this[int x, int y, int z]
        {
            get { return voxels[GetArrayOffset(x, y, z)]; }
            set
            {
                voxels[GetArrayOffset(x, y, z)] = value;
                totalVoxels = -1;
            }
        }
        
        /// <summary>
        /// Gets the number of non-empty voxels
        /// </summary>
        internal int TotalVoxels
        {
            get
            {
                if (totalVoxels == -1)
                    totalVoxels = TiVEUtils.GetCountOfNonEmptyVoxels(voxels);
                return totalVoxels;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetArrayOffset(int x, int y, int z)
        {
            TiVEUtils.DebugCheckConstraints(x, y, z, VoxelSize);
            return (((z << VoxelSizeBitShift) + x) << VoxelSizeBitShift) + y; // y-axis major for speed
        }
    }
    #endregion

    #region BlockLOD16 class
    [PublicAPI]
    public sealed class BlockLOD16 : BlockLOD, ITiVESerializable
    {
        public static readonly Guid ID = new Guid("113737C6-3E87-44A3-9190-90406D71DF6F");
        public const int VoxelSize = 16;
        public const int VoxelSizeBitShift = 4;
        public const int MagicModulusNumber = VoxelSize - 1;

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

        public override int VoxelAxisSize => VoxelSize;

        public override int VoxelAxisSizeBitShift => VoxelSizeBitShift;

        public override Voxel this[int x, int y, int z]
        {
            get { return voxels[GetArrayOffset(x, y, z)]; }
            set { voxels[GetArrayOffset(x, y, z)] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetArrayOffset(int x, int y, int z)
        {
            TiVEUtils.DebugCheckConstraints(x, y, z, VoxelSize);
            return (((z << VoxelSizeBitShift) + x) << VoxelSizeBitShift) + y; // y-axis major for speed
        }
    }
    #endregion

    #region BlockLOD8 class
    [PublicAPI]
    public sealed class BlockLOD8 : BlockLOD, ITiVESerializable
    {
        public static readonly Guid ID = new Guid("9AFDDACC-04A2-4742-A7E1-0BD17AEF6A36");
        public const int VoxelSize = 8;
        public const int VoxelSizeBitShift = 3;
        public const int MagicModulusNumber = VoxelSize - 1;
        
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

        public override int VoxelAxisSize => VoxelSize;

        public override int VoxelAxisSizeBitShift => VoxelSizeBitShift;

        public override Voxel this[int x, int y, int z]
        {
            get { return voxels[GetArrayOffset(x, y, z)]; }
            set { voxels[GetArrayOffset(x, y, z)] = value; }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetArrayOffset(int x, int y, int z)
        {
            TiVEUtils.DebugCheckConstraints(x, y, z, VoxelSize);
            return (((z << VoxelSizeBitShift) + x) << VoxelSizeBitShift) + y; // y-axis major for speed
        }
    }
    #endregion

    #region BlockLOD4 class
    [PublicAPI]
    public sealed class BlockLOD4 : BlockLOD, ITiVESerializable
    {
        public static readonly Guid ID = new Guid("EEB67B04-5116-4919-95AD-D6B52AAC1E27");
        public const int VoxelSize = 4;
        public const int VoxelSizeBitShift = 2;
        public const int MagicModulusNumber = VoxelSize - 1;

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

        public override int VoxelAxisSize => VoxelSize;

        public override int VoxelAxisSizeBitShift => VoxelSizeBitShift;

        public override Voxel this[int x, int y, int z]
        {
            get { return voxels[GetArrayOffset(x, y, z)]; }
            set { voxels[GetArrayOffset(x, y, z)] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetArrayOffset(int x, int y, int z)
        {
            TiVEUtils.DebugCheckConstraints(x, y, z, VoxelSize);
            return (((z << VoxelSizeBitShift) + x) << VoxelSizeBitShift) + y; // y-axis major for speed
        }
    }
    #endregion
}
