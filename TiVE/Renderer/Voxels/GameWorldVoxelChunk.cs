using System;
using System.Diagnostics;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.World;
using ProdigalSoftware.TiVE.Resources;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.Voxels
{
    internal class GameWorldVoxelChunk : IDisposable
    {
        #region Constants
        public const int TileSize = 5;
        #endregion

        protected MeshBuilder meshBuilder;
        private readonly object syncLock = new object();
        private readonly int chunkX;
        private readonly int chunkY;
        private readonly int chunkZ;
        private bool deleted;
        private IVertexDataCollection mesh;
        private Matrix4 translationMatrix;
        private int chunkPolygonCount;
        private int chunkVoxelCount;
        private int chunkRenderedVoxelCount;

        public GameWorldVoxelChunk(int chunkX, int chunkY, int chunkZ)
        {
            this.chunkX = chunkX;
            this.chunkY = chunkY;
            this.chunkZ = chunkZ;

            translationMatrix = Matrix4.CreateTranslation(chunkX * TileSize * BlockInformation.BlockSize,
                chunkY * TileSize * BlockInformation.BlockSize, chunkZ * TileSize * BlockInformation.BlockSize);
        }

        public void Dispose()
        {
            using (new PerformanceLock(syncLock))
            {
                if (meshBuilder != null)
                    meshBuilder.DropMesh();

                if (mesh != null)
                    mesh.Dispose();

                meshBuilder = null;
                mesh = null;
                deleted = true;
            }
        }

        protected virtual string ShaderName 
        {
            get { return "MainWorld"; }
        }

        public MeshBuilder MeshBuilder 
        {
            get 
            { 
                using (new PerformanceLock(syncLock))
                    return meshBuilder; 
            }
        }

        public bool NeedsInitialization 
        {
            get
            {
                using (new PerformanceLock(syncLock))
                    return meshBuilder != null;
            }
        }

        public bool IsDeleted
        {
            get
            {
                using (new PerformanceLock(syncLock))
                    return deleted;
            }
        }

        public bool IsInside(int checkChunkStartX, int checkChunkStartY, int checkChunkEndX, int checkChunkEndY)
        {
            return checkChunkStartX <= chunkX && checkChunkEndX >= chunkX && checkChunkStartY <= chunkY && checkChunkEndY >= chunkY;
        }

        public void PrepareForLoad()
        {
            using (new PerformanceLock(syncLock))
                deleted = false;
        }

        public void Load(MeshBuilder newMeshBuilder)
        {
            Debug.Assert(newMeshBuilder.IsLocked);

            GameWorld gameWorld = ResourceManager.GameWorldManager.GameWorld;

            //Debug.WriteLine("Started chunk ({0},{1},{2})", chunkStartX, chunkStartY, chunkStartZ);

            int worldVoxelStartX = chunkX * TileSize * BlockInformation.BlockSize;
            int worldVoxelEndX = Math.Min((chunkX + 1) * TileSize * BlockInformation.BlockSize, gameWorld.VoxelSize.X);
            int worldVoxelStartY = chunkY * TileSize * BlockInformation.BlockSize;
            int worldVoxelEndY = Math.Min((chunkY + 1) * TileSize * BlockInformation.BlockSize, gameWorld.VoxelSize.Y);
            int worldVoxelStartZ = chunkZ * TileSize * BlockInformation.BlockSize;
            int worldVoxelEndZ = Math.Min((chunkZ + 1) * TileSize * BlockInformation.BlockSize, gameWorld.VoxelSize.Z);

            int voxelCount;
            int renderedVoxelCount;
            int polygonCount;
            CreateMesh(newMeshBuilder, gameWorld, worldVoxelStartX, worldVoxelEndX, worldVoxelStartY, worldVoxelEndY, worldVoxelStartZ, worldVoxelEndZ, 
                out voxelCount, out renderedVoxelCount, out polygonCount);

            using (new PerformanceLock(syncLock))
            {
                meshBuilder = newMeshBuilder;
                chunkPolygonCount = polygonCount;
                chunkVoxelCount = voxelCount;
                chunkRenderedVoxelCount = renderedVoxelCount;

                if (deleted)
                    Dispose(); // Chunk was deleted while loading
            }
        }

        public bool Initialize(IRendererData voxelInstanceLocationData)
        {
            using (new PerformanceLock(syncLock))
            {
                if (meshBuilder == null)
                    return false;
                
                if (mesh != null)
                    mesh.Dispose();

                if (chunkPolygonCount > 0)
                {
                    mesh = GetMesh(voxelInstanceLocationData);
                    mesh.Initialize();
                }

                meshBuilder.DropMesh(); // Release builder - Must be called after initializing the mesh
                meshBuilder = null;
            }
            return chunkPolygonCount > 0;
        }

        public RenderStatistics Render(ref Matrix4 viewProjectionMatrix)
        {
            if (chunkPolygonCount == 0)
                return new RenderStatistics(); // Nothing to render for this chunk

            IVertexDataCollection meshData;
            using (new PerformanceLock(syncLock))
                meshData = mesh;

            if (meshData == null)
                return new RenderStatistics(); // Nothing to render for this chunk

            Debug.Assert(meshData.IsInitialized);

            IShaderProgram shader = ResourceManager.ShaderManager.GetShaderProgram(ShaderName);
            shader.Bind();

            Matrix4 viewProjectionModelMatrix;
            Matrix4.Mult(ref translationMatrix, ref viewProjectionMatrix, out viewProjectionModelMatrix);
            shader.SetUniform("matrix_ModelViewProjection", ref viewProjectionModelMatrix);

            TiVEController.Backend.Draw(PrimitiveType.Triangles, meshData);
            return new RenderStatistics(1, chunkPolygonCount, chunkVoxelCount, chunkRenderedVoxelCount);
        }

        public override string ToString()
        {
            return string.Format("Chunk ({0}, {1}, {2}) {3}v", chunkX, chunkY, chunkZ, chunkVoxelCount);
        }

        protected virtual void CreateMesh(MeshBuilder newMeshBuilder, GameWorld gameWorld,
            int worldVoxelStartX, int worldVoxelEndX, int worldVoxelStartY, int worldVoxelEndY, int worldVoxelStartZ, int worldVoxelEndZ,
            out int voxelCount, out int renderedVoxelCount, out int polygonCount)
        {
            voxelCount = 0;
            renderedVoxelCount = 0;
            polygonCount = 0;
            int maxVoxelX = gameWorld.VoxelSize.X;
            int maxVoxelY = gameWorld.VoxelSize.Y;
            int maxVoxelZ = gameWorld.VoxelSize.Z;

            for (int x = worldVoxelStartX; x < worldVoxelEndX; x++)
            {
                int cx = x - worldVoxelStartX;

                for (int z = worldVoxelStartZ; z < worldVoxelEndZ; z++)
                {
                    int cz = z - worldVoxelStartZ;

                    for (int y = worldVoxelStartY; y < worldVoxelEndY; y++)
                    {
                        uint color = gameWorld.GetVoxel(x, y, z);
                        if (color == 0)
                            continue;

                        voxelCount++;

                        VoxelSides sides = VoxelSides.None;
                        if (z == maxVoxelZ - 1 || gameWorld.GetVoxel(x, y, z + 1) == 0)
                            sides |= VoxelSides.Front;
                        //if (z == 0 || gameWorld.GetVoxel(x, y, z - 1) == 0) // The back face is never shown to the camera, so there is no need to create it
                        //    sizes |= VoxelSides.Back;
                        if (x == 0 || gameWorld.GetVoxel(x - 1, y, z) == 0)
                            sides |= VoxelSides.Left;
                        if (x == maxVoxelX - 1 || gameWorld.GetVoxel(x + 1, y, z) == 0)
                            sides |= VoxelSides.Right;
                        if (y == 0 || gameWorld.GetVoxel(x, y - 1, z) == 0)
                            sides |= VoxelSides.Bottom;
                        if (y == maxVoxelY - 1 || gameWorld.GetVoxel(x, y + 1, z) == 0)
                            sides |= VoxelSides.Top;

                        if (sides != VoxelSides.None)
                        {
                            float percentR;
                            float percentG;
                            float percentB;
                            gameWorld.GetLightAt(x, y, z, out percentR, out percentG, out percentB);
                            byte a = (byte)((color >> 24) & 0xFF);
                            byte r = (byte)Math.Min(255, (int)(((color >> 16) & 0xFF) * percentR));
                            byte g = (byte)Math.Min(255, (int)(((color >> 8) & 0xFF) * percentG));
                            byte b = (byte)Math.Min(255, (int)(((color >> 0) & 0xFF) * percentB));

                            //byte a = (byte)((color >> 24) & 0xFF);
                            //byte r = (byte)((color >> 16) & 0xFF);
                            //byte g = (byte)((color >> 8) & 0xFF);
                            //byte b = (byte)((color >> 0) & 0xFF);

                            polygonCount += IndexedVoxelGroup.CreateVoxel(newMeshBuilder, sides, cx, y - worldVoxelStartY, cz, r, g, b, a);
                            renderedVoxelCount++;
                        }
                    }
                }
            }
        }

        protected virtual IVertexDataCollection GetMesh(IRendererData voxelInstanceLocationData)
        {
            return meshBuilder.GetMesh();
        }
    }
}
