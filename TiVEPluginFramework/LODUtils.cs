using System;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [PublicAPI]
    public static class LODUtils
    {
        public static int AdjustForDetailLevelFrom32(int val, LODLevel toDetailLevel)
        {
            return val >> (BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(toDetailLevel));
        }

        public static void AdjustLocationForDetailLevelFrom32(ref int x, ref int y, ref int z, LODLevel toDetailLevel)
        {
            int bitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(toDetailLevel);
            x = x >> bitShift;
            y = y >> bitShift;
            z = z >> bitShift;
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

        public static int AdjustForDetailLevelTo32(int val, LODLevel fromDetailLevel)
        {
            return val << (BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(fromDetailLevel));
        }

        public static void AdjustLocationForDetailLevelTo32(ref int x, ref int y, ref int z, LODLevel fromDetailLevel)
        {
            int bitShift = BlockLOD32.VoxelSizeBitShift - BlockLOD.GetVoxelSizeBitShift(fromDetailLevel);
            x = x << bitShift;
            y = y << bitShift;
            z = z << bitShift;
        }

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
    }
}
