using NUnit.Framework;
using ProdigalSoftware.TiVEPluginFramework;

namespace TiVEPluginFrameworkTests
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

            gameWorld[0, 0, 0] = 1;
            gameWorld[2, 0, 0] = 2;
            gameWorld[0, 2, 0] = 3;
            gameWorld[0, 0, 2] = 4;
            gameWorld[2, 2, 0] = 5;
            gameWorld[0, 2, 2] = 6;
            gameWorld[2, 0, 2] = 7;
            gameWorld[2, 2, 2] = 8;

            Assert.That(gameWorld[0, 0, 0], Is.EqualTo(1));
            Assert.That(gameWorld[1, 0, 0], Is.EqualTo(0));
            Assert.That(gameWorld[2, 0, 0], Is.EqualTo(2));
            Assert.That(gameWorld[0, 1, 0], Is.EqualTo(0));
            Assert.That(gameWorld[1, 1, 0], Is.EqualTo(0));
            Assert.That(gameWorld[2, 1, 0], Is.EqualTo(0));
            Assert.That(gameWorld[0, 2, 0], Is.EqualTo(3));
            Assert.That(gameWorld[1, 2, 0], Is.EqualTo(0));
            Assert.That(gameWorld[2, 2, 0], Is.EqualTo(5));
            Assert.That(gameWorld[0, 0, 1], Is.EqualTo(0));
            Assert.That(gameWorld[1, 0, 1], Is.EqualTo(0));
            Assert.That(gameWorld[2, 0, 1], Is.EqualTo(0));
            Assert.That(gameWorld[0, 1, 1], Is.EqualTo(0));
            Assert.That(gameWorld[1, 1, 1], Is.EqualTo(0));
            Assert.That(gameWorld[2, 1, 1], Is.EqualTo(0));
            Assert.That(gameWorld[0, 2, 1], Is.EqualTo(0));
            Assert.That(gameWorld[1, 2, 1], Is.EqualTo(0));
            Assert.That(gameWorld[2, 2, 1], Is.EqualTo(0));
            Assert.That(gameWorld[0, 0, 2], Is.EqualTo(4));
            Assert.That(gameWorld[1, 0, 2], Is.EqualTo(0));
            Assert.That(gameWorld[2, 0, 2], Is.EqualTo(7));
            Assert.That(gameWorld[0, 1, 2], Is.EqualTo(0));
            Assert.That(gameWorld[1, 1, 2], Is.EqualTo(0));
            Assert.That(gameWorld[2, 1, 2], Is.EqualTo(0));
            Assert.That(gameWorld[0, 2, 2], Is.EqualTo(6));
            Assert.That(gameWorld[1, 2, 2], Is.EqualTo(0));
            Assert.That(gameWorld[2, 2, 2], Is.EqualTo(8));
        }
    }
}
