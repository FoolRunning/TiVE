using System;
using System.Collections.Generic;
using System.Linq;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    /// <summary>
    /// Manages a queue of entities that are waiting to be loaded. Automatically sorts them by highest-priority.
    /// </summary>
    internal sealed class EntityLoadQueue
    {
        #region Member variables
        private readonly EntityQueue[] entityQueues = new EntityQueue[VoxelMeshSystem.VoxelDetailLevelSections];
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new EntityLoadQueue with the specified capacity
        /// </summary>
        public EntityLoadQueue(int maxCapacity)
        {
            for (int i = 0; i < entityQueues.Length; i++)
                entityQueues[i] = new EntityQueue(maxCapacity);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return entityQueues.All(equ => equ.Size == 0); }
        }

        /// <summary>
        /// Gets the number of entities that are currently in the queue
        /// </summary>
        public int Size
        {
            get { return entityQueues.Sum(equ => equ.Size); }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Adds the specified entity to the queue to be loaded at the specified detail level. If the entity is already in the queue at a 
        /// different detail level, then the entity is removed from the queue and added at the specifed new detail level.
        /// </summary>
        public void Enqueue(IEntity entity, byte detailLevel)
        {
            Remove(entity);
            entityQueues[detailLevel].Enqueue(entity);
        }

        /// <summary>
        /// Gets the next entity in the queue or null if the queue is empty
        /// </summary>
        public IEntity Dequeue(out byte detailLevel)
        {
            for (int i = entityQueues.Length - 1; i >= 0; i--)
            {
                IEntity entity = entityQueues[i].Dequeue();
                if (entity != null)
                {
                    detailLevel = (byte)i;
                    return entity;
                }
            }

            detailLevel = VoxelMeshComponent.BlankDetailLevel;
            return null;
        }

        /// <summary>
        /// Clears the queue of all chunks waiting to be loaded
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < entityQueues.Length; i++)
                entityQueues[i].Clear();
        }

        /// <summary>
        /// Gets whether the specified entity is already in the queue at the specified detail level
        /// </summary>
        public bool Contains(IEntity entity, byte detailLevel)
        {
            return entityQueues[detailLevel].Contains(entity);
        }

        /// <summary>
        /// Removes the specified entity from the queue
        /// </summary>
        public void Remove(IEntity entity)
        {
            for (int i = 0; i < entityQueues.Length; i++)
                entityQueues[i].Remove(entity);
        }
        #endregion

        #region EntityQueue class
        private sealed class EntityQueue
        {
            private readonly Dictionary<IEntity, QueueItem> entityLocations;
            private readonly int maxCapacity;
            private volatile int size;
            private QueueItem lastQueuedItem;
            private QueueItem root;
            private QueueItem tail;

            public EntityQueue(int maxCapacity)
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

            /// <summary>
            /// Gets the number of entities that are currently in the queue
            /// </summary>
            public int Size
            {
                get { return size; }
            }

            /// <summary>
            /// Adds the specified entity to the queue to be loaded at the specified detail level. If the entity is already in the queue at a 
            /// different detail level, then the entity is removed from the queue and added at the specifed new detail level.
            /// </summary>
            public void Enqueue(IEntity entity)
            {
                if (size == maxCapacity)
                    throw new InvalidOperationException("Queue is full.");

                QueueItem item = (lastQueuedItem == null) ? root : lastQueuedItem.NextItem;
                item.Entity = entity;
                entityLocations.Add(entity, item);
                lastQueuedItem = item;
                size++;
            }

            /// <summary>
            /// Gets the next entity in the queue or null if the queue is empty
            /// </summary>
            public IEntity Dequeue()
            {
                if (size == 0)
                    return null;

                QueueItem itemToDequeue = root;
                IEntity entity = itemToDequeue.Entity;
                RemoveFromQueue(itemToDequeue);
                return entity;
            }

            /// <summary>
            /// Clears the queue of all entities waiting to be loaded
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
                lastQueuedItem = null;
            }

            /// <summary>
            /// Gets whether the specified entity is already in the queue at the specified detail level
            /// </summary>
            public bool Contains(IEntity entity)
            {
                return entityLocations.ContainsKey(entity);
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

                if (item == lastQueuedItem)
                    lastQueuedItem = item.PrevItem;

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
            }

            public override string ToString()
            {
                return Entity != null ? Entity.ToString() : "Empty";
            }
        }
        #endregion
    }
}
