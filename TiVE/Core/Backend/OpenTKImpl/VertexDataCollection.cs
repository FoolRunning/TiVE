using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal sealed class MeshVertexDataCollection : IVertexDataCollection
    {
        private readonly List<IRendererData> buffers = new List<IRendererData>();
        private volatile bool deleted;
        private volatile bool initialized;
        private int vertexArrayId;
        private IRendererData indexBuffer;
        private IRendererData firstVertexBuffer;
        private IRendererData firstInstanceBuffer;

        ~MeshVertexDataCollection()
        {
            Messages.Assert(vertexArrayId == 0, "Data collection was not properly deleted");
        }

        public void Dispose()
        {
            using (new PerformanceLock(buffers))
            {
                buffers.ForEach(b => b.Dispose());
                if (indexBuffer != null)
                    indexBuffer.Dispose();

                if (vertexArrayId != 0)
                {
                    GL.DeleteVertexArray(vertexArrayId);
                    GlUtils.CheckGLErrors();
                }

                vertexArrayId = 0;
                deleted = true;
            }

            GC.SuppressFinalize(this);
        }

        public bool ContainsIndexes
        {
            get { return indexBuffer != null; }
        }

        public bool ContainsInstances
        {
            get { return firstInstanceBuffer != null; }
        }

        public int VertexCount
        {
            get { return firstVertexBuffer != null ? firstVertexBuffer.DataLength : 0; }
        }

        public int IndexCount
        {
            get { return indexBuffer != null ? indexBuffer.DataLength : 0; }
        }

        public DataValueType IndexType
        {
            get { return indexBuffer != null ? indexBuffer.DataValueType : DataValueType.Float; }
        }

        public int InstanceCount
        {
            get { return firstInstanceBuffer != null ? firstInstanceBuffer.DataLength : 0; }
        }

        public bool IsInitialized
        {
            get { return initialized || deleted; }
        }

        public void AddBuffer(IRendererData buffer)
        {
            if (vertexArrayId != 0)
                throw new InvalidOperationException("Can not add a buffer after the collection has been initialized");

            if (buffers.Contains(buffer))
                throw new InvalidOperationException("Can not add the same buffer more then once");

            IRendererData existingBuffer = buffers.Find(b => b.DataType == buffer.DataType);
            if (existingBuffer != null && existingBuffer.DataLength != buffer.DataLength)
                throw new InvalidOperationException("New buffers must contain the same number of data elements as other buffers of the same type");

            if (buffer.DataType == DataType.Index)
            {
                if (indexBuffer != null)
                    throw new InvalidOperationException("Buffer collection can only contain one index buffer");
                indexBuffer = buffer;
            }
            else
            {
                // Cache buffers for speed when getting counts, etc.
                if (firstVertexBuffer == null && buffer.DataType == DataType.Vertex)
                    firstVertexBuffer = buffer;
                else if (firstInstanceBuffer == null && buffer.DataType == DataType.Instance)
                    firstInstanceBuffer = buffer;

                buffers.Add(buffer);
            }
        }

        public bool Initialize()
        {
            if (vertexArrayId != 0)
                return true; // Already initialized

            bool success = true;
            using (new PerformanceLock(buffers))
            {
                vertexArrayId = GL.GenVertexArray();
                GL.BindVertexArray(vertexArrayId);

                for (int i = 0; i < buffers.Count; i++)
                    success &= buffers[i].Initialize(i);
                if (indexBuffer != null)
                    success &= indexBuffer.Initialize(-1);
                initialized = success;
            }
            return success;
        }

        public void Bind()
        {
            // ENHANCE: Create state management and only change the array attributes if needed
            GL.BindVertexArray(vertexArrayId);

            // Enable/disable array attributes
            for (int i = 0; i < buffers.Count; i++)
                GL.EnableVertexAttribArray(i);
        }
    }
}
