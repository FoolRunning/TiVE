﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.Meshes;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.World
{
    internal sealed class ChunkRenderTree : IDisposable
    {
        #region Constants
        private const int ChunkVoxelSize = GameWorldVoxelChunk.VoxelSize;
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

        private readonly ChunkRenderTree[] children = new ChunkRenderTree[8];
        private readonly GameWorldVoxelChunk chunk;
        private readonly BoundingBox boundingBox;
        private readonly int depth;

        private IVertexDataCollection debugBoxOutLine;
        #endregion

        #region Constructors
        public ChunkRenderTree(GameWorld gameWorld) :
            this(Vector3f.Zero, new Vector3f(
                (int)Math.Ceiling(gameWorld.BlockSize.X / (float)GameWorldVoxelChunk.BlockSize) * ChunkVoxelSize,
                (int)Math.Ceiling(gameWorld.BlockSize.Y / (float)GameWorldVoxelChunk.BlockSize) * ChunkVoxelSize,
                (int)Math.Ceiling(gameWorld.BlockSize.Z / (float)GameWorldVoxelChunk.BlockSize) * ChunkVoxelSize), 0)
        {
        }

        private ChunkRenderTree(Vector3f minPoint, Vector3f maxPoint, int depth)
        {
            boundingBox = new BoundingBox(minPoint, maxPoint);

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
                chunk = new GameWorldVoxelChunk((int)(minPoint.X / ChunkVoxelSize), (int)(minPoint.Y / ChunkVoxelSize), (int)(minPoint.Z / ChunkVoxelSize));
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
            children[FarBottomLeft] = new ChunkRenderTree(minPoint, boxCenter, childDepth);
            if (hasAvailableX)
                children[FarBottomRight] = new ChunkRenderTree(new Vector3f(boxCenter.X, minPoint.Y, minPoint.Z), new Vector3f(maxPoint.X, boxCenter.Y, boxCenter.Z), childDepth);
            if (hasAvailableY)
                children[FarTopLeft] = new ChunkRenderTree(new Vector3f(minPoint.X, boxCenter.Y, minPoint.Z), new Vector3f(boxCenter.X, maxPoint.Y, boxCenter.Z), childDepth);
            if (hasAvailableX && hasAvailableY)
                children[FarTopRight] = new ChunkRenderTree(new Vector3f(boxCenter.X, boxCenter.Y, minPoint.Z), new Vector3f(maxPoint.X, maxPoint.Y, boxCenter.Z), childDepth);

            if (hasAvailableZ)
            {
                children[NearBottomLeft] = new ChunkRenderTree(new Vector3f(minPoint.X, minPoint.Y, boxCenter.Z), new Vector3f(boxCenter.X, boxCenter.Y, maxPoint.Z), childDepth);
                if (hasAvailableX)
                    children[NearBottomRight] = new ChunkRenderTree(new Vector3f(boxCenter.X, minPoint.Y, boxCenter.Z), new Vector3f(maxPoint.X, boxCenter.Y, maxPoint.Z), childDepth);
                if (hasAvailableY)
                    children[NearTopLeft] = new ChunkRenderTree(new Vector3f(minPoint.X, boxCenter.Y, boxCenter.Z), new Vector3f(boxCenter.X, maxPoint.Y, maxPoint.Z), childDepth);
                if (hasAvailableX && hasAvailableY)
                    children[NearTopRight] = new ChunkRenderTree(boxCenter, maxPoint, depth + 1);
            }
        }
        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (debugBoxOutLine != null)
                debugBoxOutLine.Dispose();

            if (chunk != null)
                chunk.Dispose();

            ChunkRenderTree[] childrenLocal = children;
            for (int i = 0; i < childrenLocal.Length; i++)
            {
                ChunkRenderTree childBox = childrenLocal[i];
                if (childBox != null)
                    childBox.Dispose();
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Fills the specified HashSet with chunks that should be rendered based on the current location and orientation of the specified camera
        /// </summary>
        public void FillChunksToRender(HashSet<GameWorldVoxelChunk> chunksToRender, Camera camera)
        {
            if (chunk != null)
                chunksToRender.Add(chunk);

            ChunkRenderTree[] childrenLocal = children;
            for (int i = 0; i < childrenLocal.Length; i++)
            {
                ChunkRenderTree childBox = childrenLocal[i];
                if (childBox != null)
                {
                    if (camera.BoxInView(childBox.boundingBox, depth <= 10))
                        childBox.FillChunksToRender(chunksToRender, camera);
                }
            }
        }
        #endregion

        #region Debug code
        public RenderStatistics Render(ShaderManager shaderManager, ref Matrix4f viewProjectionMatrix, Camera camera)
        {
            return Render(shaderManager, ref viewProjectionMatrix, camera, -1);
        }

        private RenderStatistics Render(ShaderManager shaderManager, ref Matrix4f viewProjectionMatrix, Camera camera, int locationInParent)
        {
            RenderDebugOutline(shaderManager, ref viewProjectionMatrix, locationInParent);

            RenderStatistics stats = new RenderStatistics(1, 12, 0, 0);
            ChunkRenderTree[] childrenLocal = children;
            for (int i = 0; i < childrenLocal.Length; i++)
            {
                ChunkRenderTree childBox = childrenLocal[i];
                if (childBox != null && camera.BoxInView(childBox.boundingBox, depth <= 10))
                    stats += childBox.Render(shaderManager, ref viewProjectionMatrix, camera, i);
            }

            return stats;
        }

        private void RenderDebugOutline(ShaderManager shaderManager, ref Matrix4f viewProjectionMatrix, int locationInParent)
        {
            if (debugBoxOutLine == null)
            {
                Color4b color = GetColorForLocation(locationInParent);
                debugBoxOutlineBuilder.StartNewMesh();
                int v1 = debugBoxOutlineBuilder.Add((short)(boundingBox.MinPoint.X + depth), (short)(boundingBox.MinPoint.Y + depth), (short)(boundingBox.MinPoint.Z + depth), color);
                int v2 = debugBoxOutlineBuilder.Add((short)(boundingBox.MaxPoint.X - depth), (short)(boundingBox.MinPoint.Y + depth), (short)(boundingBox.MinPoint.Z + depth), color);
                int v3 = debugBoxOutlineBuilder.Add((short)(boundingBox.MaxPoint.X - depth), (short)(boundingBox.MaxPoint.Y - depth), (short)(boundingBox.MinPoint.Z + depth), color);
                int v4 = debugBoxOutlineBuilder.Add((short)(boundingBox.MinPoint.X + depth), (short)(boundingBox.MaxPoint.Y - depth), (short)(boundingBox.MinPoint.Z + depth), color);
                int v5 = debugBoxOutlineBuilder.Add((short)(boundingBox.MinPoint.X + depth), (short)(boundingBox.MinPoint.Y + depth), (short)(boundingBox.MaxPoint.Z - depth), color);
                int v6 = debugBoxOutlineBuilder.Add((short)(boundingBox.MaxPoint.X - depth), (short)(boundingBox.MinPoint.Y + depth), (short)(boundingBox.MaxPoint.Z - depth), color);
                int v7 = debugBoxOutlineBuilder.Add((short)(boundingBox.MaxPoint.X - depth), (short)(boundingBox.MaxPoint.Y - depth), (short)(boundingBox.MaxPoint.Z - depth), color);
                int v8 = debugBoxOutlineBuilder.Add((short)(boundingBox.MinPoint.X + depth), (short)(boundingBox.MaxPoint.Y - depth), (short)(boundingBox.MaxPoint.Z - depth), color);

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

            IShaderProgram shader = shaderManager.GetShaderProgram("MainWorld");
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