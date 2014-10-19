﻿using NUnit.Framework;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVEPluginFramework;

namespace TiVETests.World
{
    [TestFixture]
    public class GameWorldTests
    {
        [Test]
        public void Indexer()
        {
            GameWorld gameWorld = new GameWorld(3, 3, 3);
            Assert.That(gameWorld.BlockSize.X, Is.EqualTo(3));
            Assert.That(gameWorld.BlockSize.Y, Is.EqualTo(3));
            Assert.That(gameWorld.BlockSize.Z, Is.EqualTo(3));

            gameWorld[0, 0, 0] = new BlockInformation("0,0,0");
            gameWorld[2, 0, 0] = new BlockInformation("2,0,0");
            gameWorld[0, 2, 0] = new BlockInformation("0,2,0");
            gameWorld[0, 0, 2] = new BlockInformation("0,0,2");
            gameWorld[2, 2, 0] = new BlockInformation("2,2,0");
            gameWorld[0, 2, 2] = new BlockInformation("0,2,2");
            gameWorld[2, 0, 2] = new BlockInformation("2,0,2");
            gameWorld[2, 2, 2] = new BlockInformation("2,2,2");

            VerifyBlock(gameWorld[0, 0, 0], "0,0,0");
            VerifyBlock(gameWorld[1, 0, 0], "Empty");
            VerifyBlock(gameWorld[2, 0, 0], "2,0,0");
            VerifyBlock(gameWorld[0, 1, 0], "Empty");
            VerifyBlock(gameWorld[1, 1, 0], "Empty");
            VerifyBlock(gameWorld[2, 1, 0], "Empty");
            VerifyBlock(gameWorld[0, 2, 0], "0,2,0");
            VerifyBlock(gameWorld[1, 2, 0], "Empty");
            VerifyBlock(gameWorld[2, 2, 0], "2,2,0");
            VerifyBlock(gameWorld[0, 0, 1], "Empty");
            VerifyBlock(gameWorld[1, 0, 1], "Empty");
            VerifyBlock(gameWorld[2, 0, 1], "Empty");
            VerifyBlock(gameWorld[0, 1, 1], "Empty");
            VerifyBlock(gameWorld[1, 1, 1], "Empty");
            VerifyBlock(gameWorld[2, 1, 1], "Empty");
            VerifyBlock(gameWorld[0, 2, 1], "Empty");
            VerifyBlock(gameWorld[1, 2, 1], "Empty");
            VerifyBlock(gameWorld[2, 2, 1], "Empty");
            VerifyBlock(gameWorld[0, 0, 2], "0,0,2");
            VerifyBlock(gameWorld[1, 0, 2], "Empty");
            VerifyBlock(gameWorld[2, 0, 2], "2,0,2");
            VerifyBlock(gameWorld[0, 1, 2], "Empty");
            VerifyBlock(gameWorld[1, 1, 2], "Empty");
            VerifyBlock(gameWorld[2, 1, 2], "Empty");
            VerifyBlock(gameWorld[0, 2, 2], "0,2,2");
            VerifyBlock(gameWorld[1, 2, 2], "Empty");
            VerifyBlock(gameWorld[2, 2, 2], "2,2,2");
        }

        [Test]
        public void GetChunk()
        {
            GameWorld gameWorld = new GameWorld(3, 7, 13);
            Assert.That(gameWorld.ChunkSize.X, Is.EqualTo(1));
            Assert.That(gameWorld.ChunkSize.Y, Is.EqualTo(2));
            Assert.That(gameWorld.ChunkSize.Z, Is.EqualTo(3));
            Assert.That(gameWorld.GetChunk(0, 0, 0), Is.Not.Null);
            Assert.That(gameWorld.GetChunk(0, 1, 0), Is.Not.Null);
            Assert.That(gameWorld.GetChunk(0, 0, 1), Is.Not.Null);
            Assert.That(gameWorld.GetChunk(0, 1, 1), Is.Not.Null);
            Assert.That(gameWorld.GetChunk(0, 0, 2), Is.Not.Null);
            Assert.That(gameWorld.GetChunk(0, 1, 2), Is.Not.Null);
        }

        private static void VerifyBlock(BlockInformation block, string expectedBlockName)
        {
            Assert.That(block.BlockName, Is.EqualTo(expectedBlockName));
        }
    }
}