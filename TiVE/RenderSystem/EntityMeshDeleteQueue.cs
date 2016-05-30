using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    internal sealed class EntityMeshDeleteQueue
    {
        private readonly QueueItemDistanceComparer entityComparer = new QueueItemDistanceComparer();
        private readonly List<EntityDeleteQueueItem> entities;

        public EntityMeshDeleteQueue(int queueSize)
        {
            entities = new List<EntityDeleteQueueItem>((int)(queueSize * 1.5f));
        }

        public int Size
        {
            get { return entities.Count; }
        }

        public void Enqueue(EntityDeleteQueueItem item)
        {
            int existingItemIndex = FindRealIndex(item);
            if (existingItemIndex >= 0 && entities[existingItemIndex].Entity != item.Entity)
                Console.WriteLine("Probably replaced the wrong item :(");

            if (existingItemIndex >= 0)
                entities[existingItemIndex] = item;
            else
                entities.Insert(~existingItemIndex, item);
        }

        public void Remove(IEntity item)
        {
            EntityDeleteQueueItem queueItem = new EntityDeleteQueueItem(item);
            int existingItemIndex = FindRealIndex(queueItem);
            if (existingItemIndex >= 0 && entities[existingItemIndex].Entity != item)
                Console.WriteLine("Probably removed the wrong item :(");

            if (existingItemIndex >= 0)
                entities.RemoveAt(existingItemIndex);
        }

        public EntityDeleteQueueItem Dequeue()
        {
            if (entities.Count == 0)
                return null;

            EntityDeleteQueueItem item = entities[entities.Count - 1];
            entities.RemoveAt(entities.Count - 1);
            return item;
        }


        public void Sort(CameraComponent cameraData)
        {
            entityComparer.SetComparePoint(cameraData.Location);
            entities.Sort(entityComparer);
        }

        public bool Contains(EntityDeleteQueueItem item)
        {
            int existingItemIndex = FindRealIndex(item);
            if (existingItemIndex >= 0 && entities[existingItemIndex].Entity != item.Entity)
                Console.WriteLine("Probably found the wrong item :(");
            return existingItemIndex >= 0;
        }

        public void Clear()
        {
            entities.Clear();
        }

        private int FindRealIndex(EntityDeleteQueueItem item)
        {
            int closeExistingItemIndex = entities.BinarySearch(item, entityComparer);
            if (closeExistingItemIndex < 0 || entities[closeExistingItemIndex].Entity == item.Entity)
                return closeExistingItemIndex;

            int itemScore = entityComparer.GetQueueItemScore(item);
            int indexBefore = closeExistingItemIndex;
            while (indexBefore > 0)
            {
                EntityDeleteQueueItem closeItem = entities[--indexBefore];
                int closeItemScore = entityComparer.GetQueueItemScore(closeItem);
                if (closeItemScore != itemScore)
                    break; // went too far backwards
                if (closeItem.Entity == item.Entity)
                    return indexBefore;
            }

            int indexAfter = closeExistingItemIndex;
            while (indexAfter < entities.Count - 1)
            {
                EntityDeleteQueueItem closeItem = entities[++indexAfter];
                int closeItemScore = entityComparer.GetQueueItemScore(closeItem);
                if (closeItemScore != itemScore)
                    break; // went too far forwards
                if (closeItem.Entity == item.Entity)
                    return indexAfter;
            }

            return ~closeExistingItemIndex; // Couldn't actually find the item in the list so return the index where it would be inserted (how BinarySearch does the same)
        }

        #region QueueItemDistanceComparer class
        private sealed class QueueItemDistanceComparer : IComparer<EntityDeleteQueueItem>
        {
            private Vector3i comparePoint;

            public void SetComparePoint(Vector3f newComparePoint)
            {
                comparePoint = new Vector3i((int)newComparePoint.X, (int)newComparePoint.Y, (int)newComparePoint.Z);
            }

            public int Compare(EntityDeleteQueueItem x, EntityDeleteQueueItem y)
            {
                return GetQueueItemScore(x) - GetQueueItemScore(y);
            }

            public int GetQueueItemScore(EntityDeleteQueueItem item)
            {
                int distX = item.EntityLocation.X - comparePoint.X;
                int distY = item.EntityLocation.Y - comparePoint.Y;
                int distZ = item.EntityLocation.Z - comparePoint.Z;
                return distX * distX + distY * distY + distZ * distZ;
            }
        }
        #endregion
    }

    internal sealed class EntityDeleteQueueItem
    {
        public readonly IEntity Entity;
        public readonly Vector3i EntityLocation;

        public EntityDeleteQueueItem(IEntity entity)
        {
            Entity = entity;
            
            VoxelMeshComponent renderData = entity.GetComponent<VoxelMeshComponent>();
            Debug.Assert(renderData != null);
            EntityLocation = new Vector3i((int)renderData.Location.X, (int)renderData.Location.Y, (int)renderData.Location.Z);
        }
    }
}
