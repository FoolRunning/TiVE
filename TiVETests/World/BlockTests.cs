using NUnit.Framework;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace TiVETests.World
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
            BlockImpl block = new BlockImpl("Monkey");
            uint index = 0;
            for (int x = 0; x < Block.VoxelSize; x++)
            {
                for (int y = 0; y < Block.VoxelSize; y++)
                {
                    for (int z = 0; z < Block.VoxelSize; z++)
                        block[x, y, z] = (Voxel)index++;
                }
            }

            uint testIndex = 0;
            for (int x = 0; x < Block.VoxelSize; x++)
            {
                for (int y = 0; y < Block.VoxelSize; y++)
                {
                    for (int z = 0; z < Block.VoxelSize; z++)
                        Assert.That(block[x, y, z], Is.EqualTo((Voxel)testIndex++));
                }
            }
        }
    }
}
