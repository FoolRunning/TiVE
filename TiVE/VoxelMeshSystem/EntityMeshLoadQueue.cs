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
        private readonly List<EntityQueueItem> entities = new List<EntityQueueItem>(5000);

        public bool IsEmpty
        {
            get { return entities.Count == 0; }
        }

        public int Size
        {
            get { return entities.Count; }
        }

        private int FindRealIndex(EntityQueueItem item)
        {
            int closeExistingItemIndex = entities.BinarySearch(item, entityComparer);
            if (closeExistingItemIndex < 0 || entities[closeExistingItemIndex].Entity == item.Entity)
                return closeExistingItemIndex;

            int itemScore = entityComparer.GetQueueItemScore(item);
            int indexBefore = closeExistingItemIndex;
            while (indexBefore > 0)
            {
                EntityQueueItem closeItem = entities[--indexBefore];
                int closeItemScore = entityComparer.GetQueueItemScore(closeItem);
                if (closeItemScore != itemScore)
                    break; // went too far backwards
                if (closeItem.Entity == item.Entity)
                    return indexBefore;
            }

            int indexAfter = closeExistingItemIndex;
            while (indexAfter < entities.Count - 1)
            {
                EntityQueueItem closeItem = entities[++indexAfter];
                int closeItemScore = entityComparer.GetQueueItemScore(closeItem);
                if (closeItemScore != itemScore)
                    break; // went too far forwards
                if (closeItem.Entity == item.Entity)
                    return indexAfter;
            }
            
            return ~closeExistingItemIndex; // Couldn't actually find the item in the list so return the index where it would be inserted (how BinarySearch does the same)
        }

        public void Enqueue(EntityQueueItem item)
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
            EntityQueueItem queueItem = new EntityQueueItem(item, 0, false);
            int existingItemIndex = FindRealIndex(queueItem);
            if (existingItemIndex >= 0 && entities[existingItemIndex].Entity != item)
                Console.WriteLine("Probably removed the wrong item :(");

            if (existingItemIndex >= 0)
                entities.RemoveAt(existingItemIndex);
        }

        public EntityQueueItem Dequeue()
        {
            if (entities.Count == 0)
                return null;

            EntityQueueItem item = entities[entities.Count - 1];
            entities.RemoveAt(entities.Count - 1);
            return item;
        }


        public void Sort(CameraComponent cameraData)
        {
            entityComparer.SetComparePoint(cameraData.Location);
            entities.Sort(entityComparer);
        }

        public bool Contains(EntityQueueItem item)
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

        #region QueueItemDistanceComparer class
        private sealed class QueueItemDistanceComparer : IComparer<EntityQueueItem>
        {
            private Vector3i comparePoint;

            public void SetComparePoint(Vector3f newComparePoint)
            {
                comparePoint = new Vector3i((int)newComparePoint.X, (int)newComparePoint.Y, (int)newComparePoint.Z);
            }

            public int Compare(EntityQueueItem x, EntityQueueItem y)
            {
                return GetQueueItemScore(y) - GetQueueItemScore(x);
            }

            public int GetQueueItemScore(EntityQueueItem item)
            {
                int distX = item.EntityLocation.X - comparePoint.X;
                int distY = item.EntityLocation.Y - comparePoint.Y;
                int distZ = item.EntityLocation.Z - comparePoint.Z;
                return distX * distX + distY * distY + distZ * distZ;

                //if (distFromPointSquared == 0) // rare, but could happen
                //    return int.MaxValue;

                //// Score so that near items are most important, but as you get further out, detail level is more important (with higher-value levels being more important)
                //return 100000 / distFromPointSquared + item.DetailLevel * 11713;
            }
        }
        #endregion
    }

    internal sealed class EntityQueueItem
    {
        public readonly IEntity Entity;
        public readonly Vector3i EntityLocation;
        private readonly byte meshCreationInfo;

        public EntityQueueItem(IEntity entity, byte detailLevel, bool computeShadows)
        {
            Entity = entity;
            
            VoxelMeshComponent meshData = entity.GetComponent<VoxelMeshComponent>();
            Debug.Assert(meshData != null);
            EntityLocation = new Vector3i((int)meshData.Location.X, (int)meshData.Location.Y, (int)meshData.Location.Z);

            meshCreationInfo = (byte)((detailLevel & 0x7) | (computeShadows ? 0x8 : 0x0));
        }

        public byte DetailLevel
        {
            get { return (byte)(meshCreationInfo & 0x7); }
        }

        public bool ComputeShadows
        {
            get { return (meshCreationInfo & 0x8) != 0; }
        }
    }
}
