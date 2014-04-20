using System;
using System.Collections.Generic;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Particles;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Resources
{
    internal sealed class ParticleSystemManager : IDisposable
    {
        private readonly Dictionary<ParticleSystemInformation, ParticleSystemCollection> particleSystemCollections =
            new Dictionary<ParticleSystemInformation, ParticleSystemCollection>();

        public void Dispose()
        {
            foreach (ParticleSystemCollection systemCollection in particleSystemCollections.Values)
                systemCollection.Dispose();
            particleSystemCollections.Clear();
        }

        public void AddParticleSystem(ParticleSystem system)
        {
            ParticleSystemCollection collection;
            lock (particleSystemCollections)
            {
                if (!particleSystemCollections.TryGetValue(system.SystemInformation, out collection))
                    particleSystemCollections[system.SystemInformation] = collection = new ParticleSystemCollection(system.SystemInformation);
            }
            collection.Add(system);
        }

        public void RemoveParticleSystem(ParticleSystem system)
        {
            ParticleSystemCollection collection;
            lock (particleSystemCollections)
                particleSystemCollections.TryGetValue(system.SystemInformation, out collection);
            
            if (collection != null)
                collection.Remove(system);
        }

        private readonly List<ParticleSystemCollection> updateList = new List<ParticleSystemCollection>();
        private readonly List<ParticleSystemCollection> renderList = new List<ParticleSystemCollection>();

        public void UpdateAllSystems(float timeSinceLastFrame)
        {
            updateList.Clear();
            lock (particleSystemCollections)
                updateList.AddRange(particleSystemCollections.Values);

            foreach (ParticleSystemCollection systemCollection in updateList)
                systemCollection.UpdateAll(timeSinceLastFrame);
        }

        public RenderStatistics Render(ref Matrix4 matrixMVP)
        {
            renderList.Clear();
            lock (particleSystemCollections)
                renderList.AddRange(particleSystemCollections.Values);

            int drawCount = 0;
            int polygonCount = 0;
            int voxelCount = 0;
            int renderedVoxelCount = 0;
            foreach (ParticleSystemCollection systemCollection in renderList)
            {
                RenderStatistics stats = systemCollection.Render(ref matrixMVP);
                drawCount += stats.DrawCount;
                polygonCount += stats.PolygonCount;
                voxelCount += stats.VoxelCount;
                renderedVoxelCount += stats.RenderedVoxelCount;
            }

            return new RenderStatistics(drawCount, polygonCount, voxelCount, renderedVoxelCount);
        }
    }
}
