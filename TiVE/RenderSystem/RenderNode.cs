using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.Meshes;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem
{
    internal sealed class RenderNode : IDisposable
    {
        #region Constants
        private const int ChunkVoxelSize = ChunkComponent.VoxelSize;
        private const int NearTopLeft = 0;
        private const int NearTopRight = 1;
        private const int NearBottomLeft = 2;
        private const int NearBottomRight = 3;
        private const int FarTopLeft = 4;
        private const int FarTopRight = 5;
        private const int FarBottomLeft = 6;
        private const int FarBottomRight = 7;
        #endregion

        #region Member variables
        private static readonly LargeMeshBuilder debugBoxOutlineBuilder = new LargeMeshBuilder(8, 24);

        public readonly RenderNode[] ChildNodes = new RenderNode[8];
        public readonly BoundingBox BoundingBox;

        private readonly int depth;
        private readonly List<IEntity> entities;
        private IVertexDataCollection debugBoxOutLine;
        #endregion

        #region Constructors
        public RenderNode(GameWorld gameWorld, IScene scene) :
            this(Vector3f.Zero, new Vector3f(
                (int)Math.Ceiling(gameWorld.BlockSize.X / (float)ChunkComponent.BlockSize) * ChunkVoxelSize,
                (int)Math.Ceiling(gameWorld.BlockSize.Y / (float)ChunkComponent.BlockSize) * ChunkVoxelSize,
                (int)Math.Ceiling(gameWorld.BlockSize.Z / (float)ChunkComponent.BlockSize) * ChunkVoxelSize), 0, scene)
        {
        }

        private RenderNode(Vector3f minPoint, Vector3f maxPoint, int depth, IScene scene)
        {
            BoundingBox = new BoundingBox(minPoint, maxPoint);

            this.depth = depth;
            Debug.Assert((int)(maxPoint.X - minPoint.X) % ChunkVoxelSize == 0, "Game world box should fit an even number of chunks inside on the x-axis");
            Debug.Assert((int)(maxPoint.Y - minPoint.Y) % ChunkVoxelSize == 0, "Game world box should fit an even number of chunks inside on the y-axis");
            Debug.Assert((int)(maxPoint.Z - minPoint.Z) % ChunkVoxelSize == 0, "Game world box should fit an even number of chunks inside on the z-axis");

            Debug.Assert(maxPoint.X - minPoint.X > 0, "X-axis: min is greater than or equal to max");
            Debug.Assert(maxPoint.Y - minPoint.Y > 0, "Y-axis: min is greater than or equal to max");
            Debug.Assert(maxPoint.Z - minPoint.Z > 0, "Z-axis: min is greater than or equal to max");

            int boxSizeX = (int)(maxPoint.X - minPoint.X);
            int boxSizeY = (int)(maxPoint.Y - minPoint.Y);
            int boxSizeZ = (int)(maxPoint.Z - minPoint.Z);
            if (boxSizeX <= ChunkVoxelSize && boxSizeY <= ChunkVoxelSize && boxSizeZ <= ChunkVoxelSize)
            {
                // Box size is the size of a chunk, so create a chunk for this leaf node
                Vector3i chunkLoc = new Vector3i((int)(minPoint.X / ChunkVoxelSize), (int)(minPoint.Y / ChunkVoxelSize), (int)(minPoint.Z / ChunkVoxelSize));
                IEntity entity = scene.CreateNewEntity(string.Format("Chunk ({0}, {1}, {2})", chunkLoc.X, chunkLoc.Y, chunkLoc.Z));
                ChunkComponent chunkData = new ChunkComponent(chunkLoc);
                entity.AddComponent(chunkData);
                entity.AddComponent(new RenderComponent(new Vector3f(chunkData.ChunkVoxelLoc.X, chunkData.ChunkVoxelLoc.Y, chunkData.ChunkVoxelLoc.Z)));
                entities = new List<IEntity>(2);
                entities.Add(entity);
                return;
            }

            // Find the center of the box while making sure to evenly divide the box by the chunk size
            Vector3f boxCenter = new Vector3f((int)(minPoint.X + maxPoint.X) / 2 / ChunkVoxelSize * ChunkVoxelSize,
                (int)(minPoint.Y + maxPoint.Y) / 2 / ChunkVoxelSize * ChunkVoxelSize,
                (int)(minPoint.Z + maxPoint.Z) / 2 / ChunkVoxelSize * ChunkVoxelSize);

            bool hasAvailableX = true;
            bool hasAvailableY = true;
            bool hasAvailableZ = true;
            if (boxCenter.X - minPoint.X < ChunkVoxelSize)
            {
                boxCenter.X = maxPoint.X;
                hasAvailableX = false;
            }

            if (boxCenter.Y - minPoint.Y < ChunkVoxelSize)
            {
                boxCenter.Y = maxPoint.Y;
                hasAvailableY = false;
            }

            if (boxCenter.Z - minPoint.Z < ChunkVoxelSize)
            {
                boxCenter.Z = maxPoint.Z;
                hasAvailableZ = false;
            }

            int childDepth = depth + 1;
            ChildNodes[FarBottomLeft] = new RenderNode(minPoint, boxCenter, childDepth, scene);
            if (hasAvailableX)
                ChildNodes[FarBottomRight] = new RenderNode(new Vector3f(boxCenter.X, minPoint.Y, minPoint.Z), new Vector3f(maxPoint.X, boxCenter.Y, boxCenter.Z), childDepth, scene);
            if (hasAvailableY)
                ChildNodes[FarTopLeft] = new RenderNode(new Vector3f(minPoint.X, boxCenter.Y, minPoint.Z), new Vector3f(boxCenter.X, maxPoint.Y, boxCenter.Z), childDepth, scene);
            if (hasAvailableX && hasAvailableY)
                ChildNodes[FarTopRight] = new RenderNode(new Vector3f(boxCenter.X, boxCenter.Y, minPoint.Z), new Vector3f(maxPoint.X, maxPoint.Y, boxCenter.Z), childDepth, scene);

            if (hasAvailableZ)
            {
                ChildNodes[NearBottomLeft] = new RenderNode(new Vector3f(minPoint.X, minPoint.Y, boxCenter.Z), new Vector3f(boxCenter.X, boxCenter.Y, maxPoint.Z), childDepth, scene);
                if (hasAvailableX)
                    ChildNodes[NearBottomRight] = new RenderNode(new Vector3f(boxCenter.X, minPoint.Y, boxCenter.Z), new Vector3f(maxPoint.X, boxCenter.Y, maxPoint.Z), childDepth, scene);
                if (hasAvailableY)
                    ChildNodes[NearTopLeft] = new RenderNode(new Vector3f(minPoint.X, boxCenter.Y, boxCenter.Z), new Vector3f(boxCenter.X, maxPoint.Y, maxPoint.Z), childDepth, scene);
                if (hasAvailableX && hasAvailableY)
                    ChildNodes[NearTopRight] = new RenderNode(boxCenter, maxPoint, depth + 1, scene);
            }
        }
        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (debugBoxOutLine != null)
                debugBoxOutLine.Dispose();

            RenderNode[] childrenLocal = ChildNodes;
            for (int i = 0; i < childrenLocal.Length; i++)
            {
                RenderNode childBox = childrenLocal[i];
                if (childBox != null)
                    childBox.Dispose();
            }
        }
        #endregion

        #region Properties
        public List<IEntity> Entities
        {
            get { return entities; }
        }
        #endregion

        #region Debug code
        public void RenderDebugOutline(ShaderManager shaderManager, ref Matrix4f viewProjectionMatrix, int locationInParent)
        {
            if (debugBoxOutLine == null)
            {
                Color4b color = GetColorForLocation(locationInParent);
                debugBoxOutlineBuilder.StartNewMesh();
                int v1 = debugBoxOutlineBuilder.Add((short)(BoundingBox.MinPoint.X + depth), (short)(BoundingBox.MinPoint.Y + depth), (short)(BoundingBox.MinPoint.Z + depth), color);
                int v2 = debugBoxOutlineBuilder.Add((short)(BoundingBox.MaxPoint.X - depth), (short)(BoundingBox.MinPoint.Y + depth), (short)(BoundingBox.MinPoint.Z + depth), color);
                int v3 = debugBoxOutlineBuilder.Add((short)(BoundingBox.MaxPoint.X - depth), (short)(BoundingBox.MaxPoint.Y - depth), (short)(BoundingBox.MinPoint.Z + depth), color);
                int v4 = debugBoxOutlineBuilder.Add((short)(BoundingBox.MinPoint.X + depth), (short)(BoundingBox.MaxPoint.Y - depth), (short)(BoundingBox.MinPoint.Z + depth), color);
                int v5 = debugBoxOutlineBuilder.Add((short)(BoundingBox.MinPoint.X + depth), (short)(BoundingBox.MinPoint.Y + depth), (short)(BoundingBox.MaxPoint.Z - depth), color);
                int v6 = debugBoxOutlineBuilder.Add((short)(BoundingBox.MaxPoint.X - depth), (short)(BoundingBox.MinPoint.Y + depth), (short)(BoundingBox.MaxPoint.Z - depth), color);
                int v7 = debugBoxOutlineBuilder.Add((short)(BoundingBox.MaxPoint.X - depth), (short)(BoundingBox.MaxPoint.Y - depth), (short)(BoundingBox.MaxPoint.Z - depth), color);
                int v8 = debugBoxOutlineBuilder.Add((short)(BoundingBox.MinPoint.X + depth), (short)(BoundingBox.MaxPoint.Y - depth), (short)(BoundingBox.MaxPoint.Z - depth), color);

                // far plane outline
                debugBoxOutlineBuilder.AddIndex(v1);
                debugBoxOutlineBuilder.AddIndex(v2);
                debugBoxOutlineBuilder.AddIndex(v2);
                debugBoxOutlineBuilder.AddIndex(v3);
                debugBoxOutlineBuilder.AddIndex(v3);
                debugBoxOutlineBuilder.AddIndex(v4);
                debugBoxOutlineBuilder.AddIndex(v4);
                debugBoxOutlineBuilder.AddIndex(v1);

                // near plane outline
                debugBoxOutlineBuilder.AddIndex(v5);
                debugBoxOutlineBuilder.AddIndex(v6);
                debugBoxOutlineBuilder.AddIndex(v6);
                debugBoxOutlineBuilder.AddIndex(v7);
                debugBoxOutlineBuilder.AddIndex(v7);
                debugBoxOutlineBuilder.AddIndex(v8);
                debugBoxOutlineBuilder.AddIndex(v8);
                debugBoxOutlineBuilder.AddIndex(v5);

                // Other outline lines
                debugBoxOutlineBuilder.AddIndex(v1);
                debugBoxOutlineBuilder.AddIndex(v5);
                debugBoxOutlineBuilder.AddIndex(v2);
                debugBoxOutlineBuilder.AddIndex(v6);
                debugBoxOutlineBuilder.AddIndex(v3);
                debugBoxOutlineBuilder.AddIndex(v7);
                debugBoxOutlineBuilder.AddIndex(v4);
                debugBoxOutlineBuilder.AddIndex(v8);

                debugBoxOutLine = debugBoxOutlineBuilder.GetMesh();
                debugBoxOutlineBuilder.DropMesh();
                debugBoxOutLine.Initialize();
            }

            IShaderProgram shader = shaderManager.GetShaderProgram("NonShadedNonInstanced");
            shader.Bind();

            shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionMatrix);

            TiVEController.Backend.Draw(PrimitiveType.Lines, debugBoxOutLine);
        }

        private static Color4b GetColorForLocation(int location)
        {
            switch (location)
            {
                case FarTopLeft: return new Color4b(255, 0, 0, 255);
                case FarTopRight: return new Color4b(0, 255, 0, 255);
                case FarBottomLeft: return new Color4b(0, 0, 255, 255);
                case FarBottomRight: return new Color4b(255, 255, 0, 255);
                case NearTopLeft: return new Color4b(0, 255, 255, 255);
                case NearTopRight: return new Color4b(255, 0, 255, 255);
                case NearBottomLeft: return new Color4b(100, 255, 50, 255);
                case NearBottomRight: return new Color4b(50, 100, 255, 255);
                default: return new Color4b(255, 255, 255, 255);
            }
        }
        #endregion
    }
}
