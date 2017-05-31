using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Internal;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal sealed class VisibleVoxelCache
    {
        private const int CacheSize = 1000;

        private readonly List<VisibleVoxel> visibleVoxelsList = new List<VisibleVoxel>(5000);
        private readonly MostRecentlyUsedCache<IVoxelProvider, VisibleVoxel[]> visibleVoxels = new MostRecentlyUsedCache<IVoxelProvider, VisibleVoxel[]>(CacheSize);

        public VisibleVoxel[] GetVisibleVoxels(IVoxelProvider provider)
        {
            lock (visibleVoxels)
                return visibleVoxels.GetFromCache(provider, CalculateVisibleVoxels);
        }

        private VisibleVoxel[] CalculateVisibleVoxels(IVoxelProvider provider)
        {
            visibleVoxelsList.Clear();

            int maxVoxelX = provider.VoxelCount.X - 1;
            int maxVoxelY = provider.VoxelCount.X - 1;
            int maxVoxelZ = provider.VoxelCount.X - 1;
            for (int bvz = 0; bvz <= maxVoxelZ; bvz++)
            {
                for (int bvx = 0; bvx <= maxVoxelX; bvx++)
                {
                    for (int bvy = 0; bvy <= maxVoxelY; bvy++)
                    {
                        Voxel vox = provider[bvx, bvy, bvz];
                        if (vox == Voxel.Empty)
                            continue;

                        CubeSides sides = CubeSides.None;

                        if (bvz == 0 || provider[bvx, bvy, bvz - 1] == Voxel.Empty)
                            sides |= CubeSides.ZMinus;

                        if (bvz == maxVoxelZ || provider[bvx, bvy, bvz + 1] == Voxel.Empty)
                            sides |= CubeSides.ZPlus;

                        if (bvx == 0 || provider[bvx - 1, bvy, bvz] == Voxel.Empty)
                            sides |= CubeSides.XMinus;

                        if (bvx == maxVoxelX || provider[bvx + 1, bvy, bvz] == Voxel.Empty)
                            sides |= CubeSides.XPlus;

                        if (bvy == 0 || provider[bvx, bvy - 1, bvz] == Voxel.Empty)
                            sides |= CubeSides.YMinus;

                        if (bvy == maxVoxelY || provider[bvx, bvy + 1, bvz] == Voxel.Empty)
                            sides |= CubeSides.YPlus;

                        if (sides != CubeSides.None)
                        {
                            bool checkSurrounding = (bvz == 0 || bvz == maxVoxelZ || bvx == 0 || bvx == maxVoxelX || bvy == 0 || bvy == maxVoxelY);
                            visibleVoxelsList.Add(new VisibleVoxel(vox, bvx, bvy, bvz, sides, checkSurrounding));
                        }
                    }
                }
            }
            return visibleVoxelsList.ToArray();
        }
    }
}
