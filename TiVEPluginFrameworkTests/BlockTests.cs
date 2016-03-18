﻿using NUnit.Framework;

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