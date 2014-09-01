﻿using NUnit.Framework;
using ProdigalSoftware.TiVEPluginFramework;

namespace TiVEPluginFrameworkTests
{
    /// <summary>
    /// Tests for the BlockInformation class
    /// </summary>
    [TestFixture]
    public class BlockInformationTests
    {
        /// <summary>
        /// Tests getting/setting voxels via the BlockInformation indexer
        /// </summary>
        [Test]
        public void Indexer()
        {
            BlockInformation block = new BlockInformation("Monkey");
            uint index = 0;
            for (int x = 0; x < BlockInformation.BlockSize; x++)
            {
                for (int y = 0; y < BlockInformation.BlockSize; y++)
                {
                    for (int z = 0; z < BlockInformation.BlockSize; z++)
                        block[x, y, z] = index++;
                }
            }

            uint testIndex = 0;
            for (int x = 0; x < BlockInformation.BlockSize; x++)
            {
                for (int y = 0; y < BlockInformation.BlockSize; y++)
                {
                    for (int z = 0; z < BlockInformation.BlockSize; z++)
                        Assert.That(block[x, y, z], Is.EqualTo(testIndex++));
                }
            }
        }
    }
}