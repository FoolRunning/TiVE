using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    /// <summary>
    /// Manages a queue of entities that are waiting to be loaded. Automatically sorts them by highest-priority.
    /// </summary>
    internal sealed class EntityLoadQueue
    {
        #region Member variables
        private readonly Dictionary<IEntity, QueueItem> entityLocations;
        private readonly int maxCapacity;
        private volatile int size;
        private QueueItem root;
        private QueueItem tail;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new EntityLoadQueue with the specified capacity
        /// </summary>
        public EntityLoadQueue(int maxCapacity)
        {
            this.maxCapacity = maxCapacity;
            entityLocations = new Dictionary<IEntity, QueueItem>(maxCapacity);
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
        /// Gets the number of entities that are currently in the queue
        /// </summary>
        public int Size
        {
            get { return size; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds the specified entity to the queue to be loaded at the specified detail level. If the entity is already in the queue at a 
        /// different detail level, then the entity is removed from the queue and added at the specifed new detail level.
        /// </summary>
        public void Enqueue(IEntity entity, byte detailLevel)
        {
            if (size == maxCapacity)
                throw new InvalidOperationException("Queue is full.");

            Remove(entity);

            // Favor lowest-detail chunks first. This allows chunks that just got into view to load really quickly, then
            // load in higher-detail versions when the load becomes less.
            QueueItem item = root;
            while (item.Entity != null && item.DetailLevel >= detailLevel)
                item = item.NextItem;

            if (item.Entity != null)
            {
                // Item needs to be inserted before the item we found. Pull the item to insert from the tail.
                QueueItem itemToInsert = tail;
                QueueItem prevTail = tail.PrevItem;
                prevTail.NextItem = null;
                tail = prevTail;

                InsertItemAt(itemToInsert, item);
                item = itemToInsert;
            }

            item.Entity = entity;
            item.DetailLevel = detailLevel;
            entityLocations.Add(entity, item);
            size++;
        }

        /// <summary>
        /// Gets the next entity in the queue or null if the queue is empty
        /// </summary>
        public IEntity Dequeue(out byte detailLevel)
        {
            if (size == 0)
            {
                detailLevel = VoxelMeshComponent.BlankDetailLevel;
                return null;
            }

            QueueItem itemToDequeue = root;
            IEntity chunk = itemToDequeue.Entity;
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
            while (item != null && item.Entity != null)
            {
                item.RemoveEntity();
                item = item.NextItem;
            }
            entityLocations.Clear();
            size = 0;
        }

        /// <summary>
        /// Gets whether the specified entity is already in the queue at the specified detail level
        /// </summary>
        public bool Contains(IEntity entity, byte detailLevel)
        {
            QueueItem item;
            return entityLocations.TryGetValue(entity, out item) && item.DetailLevel == detailLevel;
        }

        /// <summary>
        /// Removes the specified entity from the queue
        /// </summary>
        public void Remove(IEntity entity)
        {
            QueueItem item;
            if (!entityLocations.TryGetValue(entity, out item))
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
            entityLocations.Remove(item.Entity);
            item.RemoveEntity(); // Must happen after the remove
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
            /// <summary>Entity in the queue at this spot</summary>
            public IEntity Entity;
            /// <summary>Requested load detail level of the entity at this spot</summary>
            public byte DetailLevel = VoxelMeshComponent.BlankDetailLevel;

            /// <summary>
            /// Creates a new link in the chain with the specified previous item
            /// </summary>
            public QueueItem(QueueItem prevItem)
            {
                PrevItem = prevItem;
            }

            /// <summary>
            /// Removes the entity out of this spot
            /// </summary>
            public void RemoveEntity()
            {
                Entity = null;
                DetailLevel = VoxelMeshComponent.BlankDetailLevel;
            }

            public override string ToString()
            {
                return Entity != null ? Entity.ToString() : "Empty";
            }
        }
        #endregion
    }
}
