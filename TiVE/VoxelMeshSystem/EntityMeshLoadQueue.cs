using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    internal sealed class EntityMeshLoadQueue
    {
        private readonly QueueItemDistanceComparer entityComparer = new QueueItemDistanceComparer();
        private readonly List<EntityLoadQueueItem> entities = new List<EntityLoadQueueItem>(5000);

        public bool IsEmpty => 
            entities.Count == 0;

        public int Size => 
            entities.Count;

        public void Enqueue(EntityLoadQueueItem item)
        {
            int existingItemIndex = FindRealIndex(item);
            //if (existingItemIndex >= 0 && entities[existingItemIndex].Entity != item.Entity)
            //    Console.WriteLine("Probably replaced the wrong item :(");

            if (existingItemIndex >= 0)
            {
                //if (entities[existingItemIndex].DetailLevel == item.DetailLevel && entities[existingItemIndex].ShadowType == item.ShadowType)
                //    Console.WriteLine("Probably enqueued item for no reason :(");
                entities[existingItemIndex] = item;
            }
            else
                entities.Insert(~existingItemIndex, item);
        }

        public void Remove(IEntity item)
        {
            EntityLoadQueueItem queueItem = new EntityLoadQueueItem(item, 0);
            int existingItemIndex = FindRealIndex(queueItem);
            //if (existingItemIndex >= 0 && entities[existingItemIndex].Entity != item)
            //    Console.WriteLine("Probably removed the wrong item :(");

            if (existingItemIndex >= 0)
                entities.RemoveAt(existingItemIndex);
        }

        public EntityLoadQueueItem Dequeue()
        {
            if (entities.Count == 0)
                return null;

            EntityLoadQueueItem item = entities[entities.Count - 1];
            entities.RemoveAt(entities.Count - 1);
            return item;
        }


        public void Sort(CameraComponent cameraData)
        {
            entityComparer.SetComparePoint(cameraData.Location);
            entities.Sort(entityComparer);

            //for (int i = 0; i < entities.Count; i++)
            //{
            //    for (int j = i + 1; j < entities.Count; j++)
            //    {
            //        if (entities[i].Entity == entities[j].Entity)
            //        {
            //            Console.WriteLine("Somehow got the same entity in the queue twice!");
            //        }
            //    }
            //}
        }

        public bool Contains(EntityLoadQueueItem item)
        {
            int existingItemIndex = FindRealIndex(item);
            //if (existingItemIndex >= 0 && entities[existingItemIndex].Entity != item.Entity)
            //    Console.WriteLine("Probably found the wrong item :(");
            return existingItemIndex >= 0;
        }

        public void Clear()
        {
            entities.Clear();
        }

        private int FindRealIndex(EntityLoadQueueItem item)
        {
            int closeExistingItemIndex = entities.BinarySearch(item, entityComparer);
            if (closeExistingItemIndex < 0 || entities[closeExistingItemIndex].Entity == item.Entity)
                return closeExistingItemIndex;

            int itemScore = entityComparer.GetQueueItemScore(item);
            int indexBefore = closeExistingItemIndex;
            while (indexBefore > 0)
            {
                EntityLoadQueueItem closeItem = entities[--indexBefore];
                int closeItemScore = entityComparer.GetQueueItemScore(closeItem);
                if (closeItemScore != itemScore)
                    break; // went too far backwards
                if (closeItem.Entity == item.Entity)
                    return indexBefore;
            }

            int indexAfter = closeExistingItemIndex;
            while (indexAfter < entities.Count - 1)
            {
                EntityLoadQueueItem closeItem = entities[++indexAfter];
                int closeItemScore = entityComparer.GetQueueItemScore(closeItem);
                if (closeItemScore != itemScore)
                    break; // went too far forwards
                if (closeItem.Entity == item.Entity)
                    return indexAfter;
            }

            return ~closeExistingItemIndex; // Couldn't actually find the item in the list so return the index where it would be inserted (how BinarySearch does the same)
        }

        #region QueueItemDistanceComparer class
        private sealed class QueueItemDistanceComparer : IComparer<EntityLoadQueueItem>
        {
            private Vector3i comparePoint;

            public void SetComparePoint(Vector3f newComparePoint)
            {
                comparePoint = new Vector3i((int)newComparePoint.X, (int)newComparePoint.Y, (int)newComparePoint.Z);
            }

            public int Compare(EntityLoadQueueItem x, EntityLoadQueueItem y)
            {
                return GetQueueItemScore(y) - GetQueueItemScore(x);
            }

            public int GetQueueItemScore(EntityLoadQueueItem item)
            {
                int distX = item.EntityLocation.X - comparePoint.X;
                int distY = item.EntityLocation.Y - comparePoint.Y;
                int distZ = item.EntityLocation.Z - comparePoint.Z;
                return distX * distX + distY * distY + distZ * distZ;
            }
        }
        #endregion
    }

    #region EntityLoadQueueItem
    internal sealed class EntityLoadQueueItem
    {
        public readonly IEntity Entity;
        public readonly Vector3i EntityLocation;
        public readonly LODLevel DetailLevel;

        public EntityLoadQueueItem(IEntity entity, LODLevel detailLevel)
        {
            if (detailLevel == LODLevel.NotSet || detailLevel == LODLevel.NumOfLevels)
                throw new ArgumentException("detailLevel not valid: " + detailLevel);

            Entity = entity;
            
            VoxelMeshComponent meshData = entity.GetComponent<VoxelMeshComponent>();
            Debug.Assert(meshData != null);
            EntityLocation = new Vector3i((int)meshData.Location.X, (int)meshData.Location.Y, (int)meshData.Location.Z);
            DetailLevel = detailLevel;
        }
    }
    #endregion
}
