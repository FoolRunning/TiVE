using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProdigalSoftware.TiVE.RenderSystem;
using ProdigalSoftware.TiVEPluginFramework;

namespace TiVETests.RenderSystem
{
    /// <summary>
    /// Test fixture for testing the EntityLoadQueue class
    /// </summary>
    [TestFixture]
    public class EntityLoadQueueTests
    {
        private EntityLoadQueue queue;

        [SetUp]
        public void Setup()
        {
            queue = new EntityLoadQueue(5);
        }

        [Test]
        public void Enqueue_PastCapacity()
        {
            DummyEntity chunk1 = new DummyEntity();
            DummyEntity chunk2 = new DummyEntity();
            DummyEntity chunk3 = new DummyEntity();
            DummyEntity chunk4 = new DummyEntity();
            DummyEntity chunk5 = new DummyEntity();
            DummyEntity chunk6 = new DummyEntity();

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
            DummyEntity chunk = new DummyEntity();

            queue.Enqueue(chunk, 1);

            Assert.That(queue.Size, Is.EqualTo(1));
            VerifyDequeue(chunk, 1);
            VerifyDequeue(null, -1);
            Assert.That(queue.Size, Is.EqualTo(0));
        }

        [Test]
        public void EnqueueDequeue_EnqueueItemAfterDequeueAllItems()
        {
            DummyEntity chunk1 = new DummyEntity();
            DummyEntity chunk2 = new DummyEntity();
            DummyEntity chunk3 = new DummyEntity();
            DummyEntity chunk4 = new DummyEntity();
            DummyEntity chunk5 = new DummyEntity();
            DummyEntity chunk6 = new DummyEntity();

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
            DummyEntity chunk1 = new DummyEntity();
            DummyEntity chunk2 = new DummyEntity();
            DummyEntity chunk3 = new DummyEntity();
            DummyEntity chunk4 = new DummyEntity();

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
            DummyEntity chunk1 = new DummyEntity();
            DummyEntity chunk2 = new DummyEntity();
            DummyEntity chunk3 = new DummyEntity();
            DummyEntity chunk4 = new DummyEntity();

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
            DummyEntity chunk1 = new DummyEntity();
            DummyEntity chunk2 = new DummyEntity();
            DummyEntity chunk3 = new DummyEntity();
            DummyEntity chunk4 = new DummyEntity();

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

        private void VerifyDequeue(DummyEntity expectedChunk, int expectedDetailLevel)
        {
            int detailLevel;
            Assert.That(queue.Dequeue(out detailLevel), Is.EqualTo(expectedChunk));
            Assert.That(detailLevel, Is.EqualTo(expectedDetailLevel));
        }

        private sealed class DummyEntity : IEntity
        {
            #region Implementation of IEntity
            public string Name
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<IComponent> Components
            {
                get { throw new NotImplementedException(); }
            }

            public void AddComponent(IComponent component)
            {
                throw new NotImplementedException();
            }

            public T GetComponent<T>() where T : class, IComponent
            {
                throw new NotImplementedException();
            }

            public IComponent GetComponent(string componentName)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
    }
}
