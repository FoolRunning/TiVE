using NUnit.Framework;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Tests for the BlockInformation class
    /// </summary>
    [TestFixture]
    public class BlockTests
    {
        /// <summary>
        /// Tests getting/setting voxels via the BlockInformation indexer
        /// </summary>
        [Test]
        public void Indexer()
        {
            Block block = new Block("Monkey");
            uint index = 0;
            BlockLOD32 lod32 = block.LOD32;
            for (int x = 0; x < lod32.VoxelAxisSize; x++)
            {
                for (int y = 0; y < lod32.VoxelAxisSize; y++)
                {
                    for (int z = 0; z < lod32.VoxelAxisSize; z++)
                        lod32[x, y, z] = (Voxel)index++;
                }
            }

            uint testIndex = 0;
            for (int x = 0; x < lod32.VoxelAxisSize; x++)
            {
                for (int y = 0; y < lod32.VoxelAxisSize; y++)
                {
                    for (int z = 0; z < lod32.VoxelAxisSize; z++)
                        Assert.That(lod32[x, y, z], Is.EqualTo((Voxel)testIndex++));
                }
            }
        }
    }
}
