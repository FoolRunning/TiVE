using System;
using NUnit.Framework;
using ProdigalSoftware.Utils;

namespace UtilsTests
{
    [TestFixture]
    public class MostRecentlyUsedCacheTests
    {
        private int createdItemCount;

        [SetUp]
        public void TestSetup()
        {
            createdItemCount = 0;
        }

        [Test]
        public void InvalidArgument()
        {
            Assert.That(() => new MostRecentlyUsedCache<string, TestItem>(1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ReusesSameItem()
        {
            MostRecentlyUsedCache<string, TestItem> cache = new MostRecentlyUsedCache<string, TestItem>(2);
            TestItem item1 = cache.GetFromCache("item1", CreateItem);
            TestItem item1_2 = cache.GetFromCache("item1", CreateItem);
            TestItem item1_3 = cache.GetFromCache("item1", CreateItem);

            Assert.That(createdItemCount, Is.EqualTo(1));
            Assert.That(item1_2, Is.SameAs(item1));
            Assert.That(item1_3, Is.SameAs(item1));
        }

        [Test]
        public void CachedItems()
        {
            MostRecentlyUsedCache<string, TestItem> cache = new MostRecentlyUsedCache<string, TestItem>(3);
            TestItem item1 = cache.GetFromCache("item1", CreateItem);
            TestItem item2 = cache.GetFromCache("item2", CreateItem);
            TestItem item3 = cache.GetFromCache("item3", CreateItem);
            TestItem item4 = cache.GetFromCache("item4", CreateItem); // Item1 should be removed from the cache

            Assert.That(createdItemCount, Is.EqualTo(4));
            TestItem item2_2 = cache.GetFromCache("item2", CreateItem); // Should still be in the cache and is now the MRU item
            // Cache should look like: Item3, Item4, Item2
            Assert.That(createdItemCount, Is.EqualTo(4));
            Assert.That(item2_2, Is.SameAs(item2));

            TestItem item1_2 = cache.GetFromCache("item1", CreateItem); // Should re-create Item1. Item3 should be removed from the cache
            // Cache should look like: Item4, Item2, Item1
            Assert.That(createdItemCount, Is.EqualTo(5));
            Assert.That(item1_2, Is.Not.SameAs(item1));

            TestItem item4_2 = cache.GetFromCache("item4", CreateItem); // Should still be in the cache and is now the MRU item
            // Cache should look like: Item2, Item1, Item4
            Assert.That(createdItemCount, Is.EqualTo(5));
            Assert.That(item4_2, Is.SameAs(item4));

            TestItem item2_3 = cache.GetFromCache("item2", CreateItem); // Should still be in the cache and is now the MRU item
            // Cache should look like: Item1, Item4, Item2
            Assert.That(createdItemCount, Is.EqualTo(5));
            Assert.That(item2_3, Is.SameAs(item2));
        }

        private TestItem CreateItem(string name)
        {
            createdItemCount++;
            return new TestItem(name);
        }

        private sealed class TestItem
        {
            private readonly string name;

            public TestItem(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return name;
            }
        }
    }
}
