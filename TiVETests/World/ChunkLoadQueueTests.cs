using System;
using NUnit.Framework;
using ProdigalSoftware.TiVE.RenderSystem.World;

namespace TiVETests.World
{
    /// <summary>
    /// Test fixture for testing the ChunkLoadQueue class
    /// </summary>
    [TestFixture]
    public class ChunkLoadQueueTests
    {
        private ChunkLoadQueue queue;

        [SetUp]
        public void Setup()
        {
            queue = new ChunkLoadQueue(5);
        }

        [Test]
        public void Enqueue_PastCapacity()
        {
            GameWorldVoxelChunk chunk1 = new GameWorldVoxelChunk(0, 0, 1);
            GameWorldVoxelChunk chunk2 = new GameWorldVoxelChunk(1, 0, 2);
            GameWorldVoxelChunk chunk3 = new GameWorldVoxelChunk(1, 2, 3);
            GameWorldVoxelChunk chunk4 = new GameWorldVoxelChunk(1, 2, 4);
            GameWorldVoxelChunk chunk5 = new GameWorldVoxelChunk(1, 2, 5);
            GameWorldVoxelChunk chunk6 = new GameWorldVoxelChunk(4, 5, 6);

            // Fill to capacity
            queue.Enqueue(chunk1, 1);
            queue.Enqueue(chunk2, 1);
            queue.Enqueue(chunk3, 1);
            queue.Enqueue(chunk4, 1);
            queue.Enqueue(chunk5, 1);

            Assert.That(queue.Size, Is.EqualTo(5));
            Assert.That(() => queue.Enqueue(chunk6, 1), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void EnqueueDequeue_NoItems()
        {
            Assert.That(queue.Size, Is.EqualTo(0));
            VerifyDequeue(null, -1);
        }

        [Test]
        public void EnqueueDequeue_OneItem()
        {
            GameWorldVoxelChunk chunk = new GameWorldVoxelChunk(0, 1, 0);

            queue.Enqueue(chunk, 1);

            Assert.That(queue.Size, Is.EqualTo(1));
            VerifyDequeue(chunk, 1);
            VerifyDequeue(null, -1);
            Assert.That(queue.Size, Is.EqualTo(0));
        }

        [Test]
        public void EnqueueDequeue_EnqueueItemAfterDequeueAllItems()
        {
            GameWorldVoxelChunk chunk1 = new GameWorldVoxelChunk(0, 0, 1);
            GameWorldVoxelChunk chunk2 = new GameWorldVoxelChunk(1, 0, 2);
            GameWorldVoxelChunk chunk3 = new GameWorldVoxelChunk(1, 2, 3);
            GameWorldVoxelChunk chunk4 = new GameWorldVoxelChunk(1, 2, 4);
            GameWorldVoxelChunk chunk5 = new GameWorldVoxelChunk(1, 2, 5);
            GameWorldVoxelChunk chunk6 = new GameWorldVoxelChunk(4, 5, 6);

            // Fill to capacity
            queue.Enqueue(chunk1, 1);
            queue.Enqueue(chunk2, 1);
            queue.Enqueue(chunk3, 1);
            queue.Enqueue(chunk4, 1);
            queue.Enqueue(chunk5, 1);

            Assert.That(queue.Size, Is.EqualTo(5));
            VerifyDequeue(chunk1, 1);
            VerifyDequeue(chunk2, 1);
            VerifyDequeue(chunk3, 1);
            VerifyDequeue(chunk4, 1);
            VerifyDequeue(chunk5, 1);
            VerifyDequeue(null, -1);
            Assert.That(queue.Size, Is.EqualTo(0));

            queue.Enqueue(chunk6, 1);

            Assert.That(queue.Size, Is.EqualTo(1));
            VerifyDequeue(chunk6, 1);
            VerifyDequeue(null, -1);
            Assert.That(queue.Size, Is.EqualTo(0));
        }

        [Test]
        public void EnqueueDequeue_MultipleItems_SameDetailLevel()
        {
            GameWorldVoxelChunk chunk1 = new GameWorldVoxelChunk(0, 0, 1);
            GameWorldVoxelChunk chunk2 = new GameWorldVoxelChunk(1, 0, 2);
            GameWorldVoxelChunk chunk3 = new GameWorldVoxelChunk(1, 2, 3);
            GameWorldVoxelChunk chunk4 = new GameWorldVoxelChunk(1, 2, 4);

            queue.Enqueue(chunk4, 1);
            queue.Enqueue(chunk2, 1);
            queue.Enqueue(chunk1, 1);
            queue.Enqueue(chunk3, 1);

            Assert.That(queue.Size, Is.EqualTo(4));
            VerifyDequeue(chunk4, 1);
            VerifyDequeue(chunk2, 1);
            VerifyDequeue(chunk1, 1);
            VerifyDequeue(chunk3, 1);
            VerifyDequeue(null, -1);
            Assert.That(queue.Size, Is.EqualTo(0));
        }

        [Test]
        public void EnqueueDequeue_MultipleItems_DifferentDetailLevels()
        {
            GameWorldVoxelChunk chunk1 = new GameWorldVoxelChunk(0, 0, 1);
            GameWorldVoxelChunk chunk2 = new GameWorldVoxelChunk(1, 0, 2);
            GameWorldVoxelChunk chunk3 = new GameWorldVoxelChunk(1, 2, 3);
            GameWorldVoxelChunk chunk4 = new GameWorldVoxelChunk(1, 2, 4);

            queue.Enqueue(chunk4, 0);
            queue.Enqueue(chunk2, 1);
            queue.Enqueue(chunk1, 2);
            queue.Enqueue(chunk3, 1);

            Assert.That(queue.Size, Is.EqualTo(4));
            VerifyDequeue(chunk1, 2);
            VerifyDequeue(chunk2, 1);
            VerifyDequeue(chunk3, 1);
            VerifyDequeue(chunk4, 0);
            VerifyDequeue(null, -1);
            Assert.That(queue.Size, Is.EqualTo(0));
        }

        [Test]
        public void EnqueueDequeue_MultipleItems_EnqueueItemsAgainWithDifferentDetailLevels()
        {
            GameWorldVoxelChunk chunk1 = new GameWorldVoxelChunk(0, 0, 1);
            GameWorldVoxelChunk chunk2 = new GameWorldVoxelChunk(1, 0, 2);
            GameWorldVoxelChunk chunk3 = new GameWorldVoxelChunk(1, 2, 3);
            GameWorldVoxelChunk chunk4 = new GameWorldVoxelChunk(1, 2, 4);

            queue.Enqueue(chunk2, 0);
            queue.Enqueue(chunk1, 1);
            queue.Enqueue(chunk3, 1);
            queue.Enqueue(chunk4, 2);

            // Enqueue the items again with different detail levels
            queue.Enqueue(chunk1, 1);
            queue.Enqueue(chunk2, 2);
            queue.Enqueue(chunk3, 0);
            queue.Enqueue(chunk4, 1);

            Assert.That(queue.Size, Is.EqualTo(4));
            VerifyDequeue(chunk2, 2);
            VerifyDequeue(chunk1, 1);
            VerifyDequeue(chunk4, 1);
            VerifyDequeue(chunk3, 0);
            VerifyDequeue(null, -1);
            Assert.That(queue.Size, Is.EqualTo(0));
        }

        private void VerifyDequeue(GameWorldVoxelChunk expectedChunk, int expectedDetailLevel)
        {
            int detailLevel;
            Assert.That(queue.Dequeue(out detailLevel), Is.EqualTo(expectedChunk));
            Assert.That(detailLevel, Is.EqualTo(expectedDetailLevel));
        }
    }
}
