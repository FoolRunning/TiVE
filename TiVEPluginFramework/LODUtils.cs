using System;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
    public static class LODUtils
    {
        public static int AdjustForDetailLevelFrom32(int val, LODLevel toDetailLevel)
        {
            switch (toDetailLevel)
            {
                case LODLevel.V32: return val;
                case LODLevel.V16: return val >> BlockLOD32.VoxelSizeBitShift - BlockLOD16.VoxelSizeBitShift;
                case LODLevel.V8: return val >> BlockLOD32.VoxelSizeBitShift - BlockLOD8.VoxelSizeBitShift;
                case LODLevel.V4: return val >> BlockLOD32.VoxelSizeBitShift - BlockLOD4.VoxelSizeBitShift;
                default: throw new ArgumentException("detailLevel invalid: " + toDetailLevel);
            }
        }

        public static int AdjustForDetailLevelTo32(int val, LODLevel fromDetailLevel)
        {
            switch (fromDetailLevel)
            {
                case LODLevel.V32: return val;
                case LODLevel.V16: return val << BlockLOD32.VoxelSizeBitShift - BlockLOD16.VoxelSizeBitShift;
                case LODLevel.V8: return val << BlockLOD32.VoxelSizeBitShift - BlockLOD8.VoxelSizeBitShift;
                case LODLevel.V4: return val << BlockLOD32.VoxelSizeBitShift - BlockLOD4.VoxelSizeBitShift;
                default: throw new ArgumentException("detailLevel invalid: " + fromDetailLevel);
            }
        }

        public static void AdjustLocationForDetailLevelFrom32(ref int x, ref int y, ref int z, LODLevel toDetailLevel)
        {
            switch (toDetailLevel)
            {
                case LODLevel.V32: break;
                case LODLevel.V16:
                    x = x >> BlockLOD32.VoxelSizeBitShift - BlockLOD16.VoxelSizeBitShift;
                    y = y >> BlockLOD32.VoxelSizeBitShift - BlockLOD16.VoxelSizeBitShift;
                    z = z >> BlockLOD32.VoxelSizeBitShift - BlockLOD16.VoxelSizeBitShift;
                    break;
                case LODLevel.V8:
                    x = x >> BlockLOD32.VoxelSizeBitShift - BlockLOD8.VoxelSizeBitShift;
                    y = y >> BlockLOD32.VoxelSizeBitShift - BlockLOD8.VoxelSizeBitShift;
                    z = z >> BlockLOD32.VoxelSizeBitShift - BlockLOD8.VoxelSizeBitShift;
                    break;
                case LODLevel.V4:
                    x = x >> BlockLOD32.VoxelSizeBitShift - BlockLOD4.VoxelSizeBitShift;
                    y = y >> BlockLOD32.VoxelSizeBitShift - BlockLOD4.VoxelSizeBitShift;
                    z = z >> BlockLOD32.VoxelSizeBitShift - BlockLOD4.VoxelSizeBitShift;
                    break;
                default: throw new ArgumentException("detailLevel invalid: " + toDetailLevel);
            }
        }

        public static void AdjustLocationForDetailLevelTo32(ref int x, ref int y, ref int z, LODLevel fromDetailLevel)
        {
            switch (fromDetailLevel)
            {
                case LODLevel.V32: break;
                case LODLevel.V16:
                    x = x << BlockLOD32.VoxelSizeBitShift - BlockLOD16.VoxelSizeBitShift;
                    y = y << BlockLOD32.VoxelSizeBitShift - BlockLOD16.VoxelSizeBitShift;
                    z = z << BlockLOD32.VoxelSizeBitShift - BlockLOD16.VoxelSizeBitShift;
                    break;
                case LODLevel.V8:
                    x = x << BlockLOD32.VoxelSizeBitShift - BlockLOD8.VoxelSizeBitShift;
                    y = y << BlockLOD32.VoxelSizeBitShift - BlockLOD8.VoxelSizeBitShift;
                    z = z << BlockLOD32.VoxelSizeBitShift - BlockLOD8.VoxelSizeBitShift;
                    break;
                case LODLevel.V4:
                    x = x << BlockLOD32.VoxelSizeBitShift - BlockLOD4.VoxelSizeBitShift;
                    y = y << BlockLOD32.VoxelSizeBitShift - BlockLOD4.VoxelSizeBitShift;
                    z = z << BlockLOD32.VoxelSizeBitShift - BlockLOD4.VoxelSizeBitShift;
                    break;
                default: throw new ArgumentException("detailLevel invalid: " + fromDetailLevel);
            }
        }

        public static Vector3i AdjustLocationForDetailLevelFrom32(Vector3i loc, LODLevel toDetailLevel)
        {
            int bitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(toDetailLevel);
            return new Vector3i(loc.X >> bitShift, loc.Y >> bitShift, loc.Z >> bitShift);
        }

        //public static void AdjustLocationForDetailLevel(ref ushort x, ref ushort y, ref ushort z, LODLevel fromDetailLevel, LODLevel toDetailLevel)
        //{
        //    int bitShift = BlockLOD.GetVoxelSizeBitShift(fromDetailLevel) - BlockLOD.GetVoxelSizeBitShift(toDetailLevel);
        //    x = (ushort)(x >> bitShift);
        //    y = (ushort)(y >> bitShift);
        //    z = (ushort)(z >> bitShift);
        //}

        public static Vector3i AdjustLocationForDetailLevelTo32(Vector3i loc, LODLevel fromDetailLevel)
        {
            int bitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(fromDetailLevel);
            return new Vector3i(loc.X << bitShift, loc.Y << bitShift, loc.Z << bitShift);
        }

        public static int GetRenderedVoxelSize(LODLevel detailLevel)
        {
            switch (detailLevel)
            {
                case LODLevel.V32: return BlockLOD32.VoxelSize / BlockLOD32.VoxelSize;
                case LODLevel.V16: return BlockLOD32.VoxelSize / BlockLOD16.VoxelSize;
                case LODLevel.V8: return BlockLOD32.VoxelSize / BlockLOD8.VoxelSize;
                case LODLevel.V4: return BlockLOD32.VoxelSize / BlockLOD4.VoxelSize;
                default: throw new ArgumentException("detailLevel invalid: " + detailLevel);
            }
        }

        public static int GetVoxelsInBlockAtDetailLevel(LODLevel detailLevel)
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

        public static int BitShiftForDetailLevel(LODLevel toDetailLevel)
        {
            switch (toDetailLevel)
            {
                case LODLevel.V32: return BlockLOD32.VoxelSizeBitShift;
                case LODLevel.V16: return BlockLOD16.VoxelSizeBitShift;
                case LODLevel.V8: return BlockLOD8.VoxelSizeBitShift;
                case LODLevel.V4: return BlockLOD4.VoxelSizeBitShift;
                default: throw new ArgumentException("detailLevel invalid: " + toDetailLevel);
            }
        }
    }
}
