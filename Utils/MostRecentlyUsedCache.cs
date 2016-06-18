using System;
using System.Collections.Generic;

namespace ProdigalSoftware.Utils
{
    public sealed class MostRecentlyUsedCache<TKey, TValue>
    {
        private readonly KeyValuePair<TKey, TValue>[] itemCache;
        private readonly int maxCacheSize;
        private int itemCount;

        public MostRecentlyUsedCache(int maxCacheSize)
        {
            itemCache = new KeyValuePair<TKey, TValue>[maxCacheSize];
            this.maxCacheSize = maxCacheSize;
        }

        public TValue GetFromCache(TKey key, Func<TKey, TValue> createValueItemFunc)
        {
            for (int i = 0; i < itemCount; i++)
            {
                if (Equals(itemCache[i].Key,key))
                {
                    KeyValuePair<TKey, TValue> item = itemCache[i];
                    RemoveItemAt(i);
                    AddItem(item);
                    return item.Value;
                }
            }

            TValue value = createValueItemFunc(key);
            AddItem(new KeyValuePair<TKey, TValue>(key, value));
            return value;
        }

        private void AddItem(KeyValuePair<TKey, TValue> item)
        {
            if (itemCount == maxCacheSize)
                RemoveItemAt(0);

            itemCache[itemCount++] = item;
        }

        private void RemoveItemAt(int index)
        {
            for (int j = index; j < itemCount - 1; j++)
                itemCache[j] = itemCache[j + 1];
            itemCount--;
        }
    }
}
