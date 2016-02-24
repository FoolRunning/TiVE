using NUnit.Framework;
using ProdigalSoftware.TiVE.RenderSystem.World;
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

            Block block1 = new Block("Block1");
            Block block2 = new Block("Block2");
            Block block3 = new Block("Block3");
            Block block4 = new Block("Block4");
            Block block5 = new Block("Block5");
            Block block6 = new Block("Block6");
            Block block7 = new Block("Block7");
            Block block8 = new Block("Block8");
            gameWorld[0, 0, 0] = block1;
            gameWorld[2, 0, 0] = block2;
            gameWorld[0, 2, 0] = block3;
            gameWorld[0, 0, 2] = block4;
            gameWorld[2, 2, 0] = block5;
            gameWorld[0, 2, 2] = block6;
            gameWorld[2, 0, 2] = block7;
            gameWorld[2, 2, 2] = block8;

            Assert.That(gameWorld[0, 0, 0], Is.EqualTo(block1));
            Assert.That(gameWorld[1, 0, 0], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 0, 0], Is.EqualTo(block2));
            Assert.That(gameWorld[0, 1, 0], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[1, 1, 0], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 1, 0], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[0, 2, 0], Is.EqualTo(block3));
            Assert.That(gameWorld[1, 2, 0], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 2, 0], Is.EqualTo(block5));
            Assert.That(gameWorld[0, 0, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[1, 0, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 0, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[0, 1, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[1, 1, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 1, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[0, 2, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[1, 2, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 2, 1], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[0, 0, 2], Is.EqualTo(block4));
            Assert.That(gameWorld[1, 0, 2], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 0, 2], Is.EqualTo(block7));
            Assert.That(gameWorld[0, 1, 2], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[1, 1, 2], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 1, 2], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[0, 2, 2], Is.EqualTo(block6));
            Assert.That(gameWorld[1, 2, 2], Is.EqualTo(Block.Empty));
            Assert.That(gameWorld[2, 2, 2], Is.EqualTo(block8));
        }
    }
}
