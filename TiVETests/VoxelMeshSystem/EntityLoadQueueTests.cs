using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using ProdigalSoftware.TiVE.VoxelMeshSystem;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace TiVETests.VoxelMeshSystem
{
    /// <summary>
    /// Test fixture for testing the EntityLoadQueue class
    /// </summary>
    [TestFixture]
    public class EntityLoadQueueTests
    {
        private EntityMeshLoadQueue queue;

        [SetUp]
        public void Setup()
        {
            queue = new EntityMeshLoadQueue();
        }

        [Test]
        public void Enqueue_SpeedTest()
        {
            queue = new EntityMeshLoadQueue();
            Random random = new Random(582754736);
            queue.Enqueue(new EntityQueueItem(new DummyEntity(), (byte)random.Next(3), false));

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 4999; i++)
                queue.Enqueue(new EntityQueueItem(new DummyEntity(), (byte)random.Next(3), false));

            sw.Stop();
            float elapsedTime = sw.ElapsedTicks * 1000.0f / Stopwatch.Frequency;
            Console.WriteLine("took " + elapsedTime + "ms to enqueue " + queue.Size + " items");
            Assert.That(elapsedTime, Is.LessThanOrEqualTo(1.0f));
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

            public bool HasComponent<T>() where T : class, IComponent
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
