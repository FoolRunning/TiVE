using System;
using System.Collections.Generic;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    /// <summary>
    /// Manages a queue of chunks that are waiting to be loaded. Automatically sorts them by highest-priority.
    /// </summary>
    internal sealed class ChunkLoadQueue
    {
        #region Member variables
        private readonly Dictionary<GameWorldVoxelChunk, QueueItem> chunkLocations;
        private readonly int maxCapacity;
        private volatile int size;
        private QueueItem root;
        private QueueItem tail;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new ChunkLoadQueue with the specified capacity
        /// </summary>
        public ChunkLoadQueue(int maxCapacity)
        {
            this.maxCapacity = maxCapacity;
            chunkLocations = new Dictionary<GameWorldVoxelChunk, QueueItem>(maxCapacity);
            QueueItem prevItem = null;
            for (int i = 0; i < maxCapacity; i++)
            {
                QueueItem item = new QueueItem(prevItem);
                if (prevItem == null)
                    root = item;
                else
                    prevItem.NextItem = item;
                prevItem = item;
            }
            tail = prevItem;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of chunks that are currently in the queue
        /// </summary>
        public int Size
        {
            get { return size; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds the specified chunk to the queue to be loaded at the specified detail level. If the chunk is already in the queue at a 
        /// different detail level, then the chunk is removed from the queue and added at the specifed new detail level.
        /// </summary>
        public void Enqueue(GameWorldVoxelChunk chunk, int detailLevel)
        {
            if (size == maxCapacity)
                throw new InvalidOperationException("Queue is full.");

            Remove(chunk);

            // Favor lowest-detail chunks first. This allows chunks that just got into view to load really quickly, then
            // load in higher-detail versions when the load becomes less.
            QueueItem item = root;
            while (item.Chunk != null && item.DetailLevel >= detailLevel)
                item = item.NextItem;

            if (item.Chunk != null)
            {
                // Item needs to be inserted before the item we found. Pull the item to insert from the tail.
                QueueItem itemToInsert = tail;
                QueueItem prevTail = tail.PrevItem;
                prevTail.NextItem = null;
                tail = prevTail;

                InsertItemAt(itemToInsert, item);
                item = itemToInsert;
            }

            item.Chunk = chunk;
            item.DetailLevel = detailLevel;
            chunkLocations.Add(chunk, item);
            size++;
        }

        /// <summary>
        /// Gets the next chunk in the queue or null if the queue is empty
        /// </summary>
        public GameWorldVoxelChunk Dequeue(out int detailLevel)
        {
            if (size == 0)
            {
                detailLevel = -1;
                return null;
            }

            QueueItem itemToDequeue = root;
            GameWorldVoxelChunk chunk = itemToDequeue.Chunk;
            detailLevel = itemToDequeue.DetailLevel;
            RemoveFromQueue(itemToDequeue);
            return chunk;
        }

        /// <summary>
        /// Clears the queue of all chunks waiting to be loaded
        /// </summary>
        public void Clear()
        {
            QueueItem item = root;
            while (item != null && item.Chunk != null)
            {
                item.RemoveChunk();
                item = item.NextItem;
            }
            chunkLocations.Clear();
            size = 0;
        }

        /// <summary>
        /// Gets whether the specified chunk is already in the queue at the specified detail level
        /// </summary>
        public bool Contains(GameWorldVoxelChunk chunk, int detailLevel)
        {
            QueueItem item;
            return chunkLocations.TryGetValue(chunk, out item) && item.DetailLevel == detailLevel;
        }

        /// <summary>
        /// Removes the specified chunk from the queue
        /// </summary>
        public void Remove(GameWorldVoxelChunk chunk)
        {
            QueueItem item;
            if (!chunkLocations.TryGetValue(chunk, out item))
                return;

            RemoveFromQueue(item);
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Removes the specified item from the queue and move it to the tail
        /// </summary>
        private void RemoveFromQueue(QueueItem item)
        {
            if (item == root)
            {
                QueueItem newRootItem = item.NextItem;
                newRootItem.PrevItem = null;
                root = newRootItem;
            }
            else
            {
                QueueItem prevItem = item.PrevItem;
                QueueItem nextItem = item.NextItem;
                prevItem.NextItem = nextItem;
                nextItem.PrevItem = prevItem;
            }

            // Move the removed item to the end of the chain
            tail.NextItem = item;
            item.PrevItem = tail;
            item.NextItem = null;
            tail = item;

            // Kill the item and update the size
            chunkLocations.Remove(item.Chunk);
            item.RemoveChunk(); // Must happen after the remove
            size--;
        }

        /// <summary>
        /// Inserts the specified item into the chain before the specified item
        /// </summary>
        private void InsertItemAt(QueueItem itemToInsert, QueueItem whereToInsert)
        {
            if (whereToInsert == root)
            {
                root.PrevItem = itemToInsert;
                itemToInsert.PrevItem = null;
                itemToInsert.NextItem = root;
                root = itemToInsert;
            }
            else
            {
                itemToInsert.NextItem = whereToInsert;
                itemToInsert.PrevItem = whereToInsert.PrevItem;
                whereToInsert.PrevItem.NextItem = itemToInsert;
                whereToInsert.PrevItem = itemToInsert;
            }
        }
        #endregion

        #region QueueItem class
        private sealed class QueueItem
        {
            /// <summary>Next item in the chain (will be null for the tail item)</summary>
            public QueueItem NextItem;
            /// <summary>Previous item in the chain (will be null for the root item)</summary>
            public QueueItem PrevItem;
            /// <summary>Chunk in the queue at this spot</summary>
            public GameWorldVoxelChunk Chunk;
            /// <summary>Requested load detail level of the chunk at this spot</summary>
            public int DetailLevel = -1;

            /// <summary>
            /// Creates a new link in the chain with the specified previous item
            /// </summary>
            public QueueItem(QueueItem prevItem)
            {
                PrevItem = prevItem;
            }

            /// <summary>
            /// Removes the chunk out of this spot
            /// </summary>
            public void RemoveChunk()
            {
                Chunk = null;
                DetailLevel = -1;
            }

            public override string ToString()
            {
                return Chunk != null ? Chunk.ToString() : "Empty";
            }
        }
        #endregion
    }
}
