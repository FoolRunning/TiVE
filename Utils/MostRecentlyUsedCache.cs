using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProdigalSoftware.Utils
{
    public sealed class MostRecentlyUsedCache<TKey, TValue>
    {
        #region Member variables
        /// <summary>The most recently used item</summary>
        private ItemContainer cacheRoot;
        /// <summary>The item used the longest time ago</summary>
        private ItemContainer cacheTail;
        private readonly Dictionary<TKey, ItemContainer> itemIndexes;
        private readonly int maxCacheSize;
        #endregion

        #region Constructor
        public MostRecentlyUsedCache(int maxCacheSize)
        {
            if (maxCacheSize <= 1)
                throw new ArgumentOutOfRangeException("maxCacheSize", "Value must be greater than one");

            itemIndexes = new Dictionary<TKey, ItemContainer>(maxCacheSize);
            this.maxCacheSize = maxCacheSize;
        }
        #endregion

        #region Public methods
        public TValue GetFromCache(TKey key, Func<TKey, TValue> createValueItemFunc)
        {
            ItemContainer itemContainer;
            if (itemIndexes.TryGetValue(key, out itemContainer))
            {
                MakeRecent(itemContainer);
                return itemContainer.Item;
            }

            TValue item = createValueItemFunc(key);
            AddItem(key, item);
            return item;
        }
        #endregion

        #region Private helper methods
        private void MakeRecent(ItemContainer itemContainer)
        {
            if (itemContainer == cacheRoot)
                return; // Already the MRU item

            if (itemContainer == cacheTail)
            {
                Debug.Assert(itemContainer.Next == null);
                cacheTail = itemContainer.Prev;
                cacheTail.Next = null;
            }
            else
            {
                itemContainer.Prev.Next = itemContainer.Next;
                itemContainer.Next.Prev = itemContainer.Prev;
            }

            itemContainer.Prev = null;
            itemContainer.Next = cacheRoot;
            cacheRoot.Prev = itemContainer;
            cacheRoot = itemContainer;
        }

        private void AddItem(TKey key, TValue item)
        {
            if (itemIndexes.Count == maxCacheSize)
            {
                // Cache is full, so remove the item used the longest time ago (always the tail)
                ItemContainer itemToRemove = cacheTail;
                itemToRemove.Prev.Next = null;
                cacheTail = itemToRemove.Prev;
                itemIndexes.Remove(itemToRemove.Key);
            }

            ItemContainer newItemContainer = new ItemContainer(key, item);
            itemIndexes.Add(key, newItemContainer);
            if (cacheRoot == null)
            {
                Debug.Assert(cacheTail == null);
                cacheRoot = cacheTail = newItemContainer;
            }
            else
            {
                // New item is the MRU item (always the root)
                newItemContainer.Next = cacheRoot;
                cacheRoot.Prev = newItemContainer;
                cacheRoot = newItemContainer;
            }

            Debug.Assert(itemIndexes.Count <= maxCacheSize);
        }
        #endregion

        #region ItemContainer class
        private sealed class ItemContainer
        {
            public readonly TKey Key;
            public readonly TValue Item;
            public ItemContainer Next;
            public ItemContainer Prev;

            public ItemContainer(TKey key, TValue item)
            {
                Key = key;
                Item = item;
            }
        }
        #endregion
    }
}
