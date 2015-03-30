using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal class RendererData<T> : IRendererData where T : struct
    {
        private T[] data;
        private int elementCount;
        private int allocatedDataElementCount;
        private readonly int elementsPerVertex;
        private readonly DataType dataType;
        private readonly DataValueType dataValueType;
        private readonly bool normalize;
        private readonly bool dynamic;
        private int dataVboId;

        public RendererData(T[] data, int elementCount, int elementsPerVertex, DataType dataType, DataValueType dataValueType, bool normalize, bool dynamic)
        {
            this.data = data;
            allocatedDataElementCount = data.Length;
            this.elementCount = elementCount;
            this.elementsPerVertex = elementsPerVertex;
            this.dataType = dataType;
            this.dataValueType = dataValueType;
            this.normalize = normalize;
            this.dynamic = dynamic;
        }

        ~RendererData()
        {
            Messages.Assert(dataVboId == 0, "Data buffer was not properly deleted");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (dataVboId != 0)
            {
                GL.DeleteBuffer(dataVboId);
                GlUtils.CheckGLErrors();
            }
            dataVboId = 0;
            data = null;
            GC.SuppressFinalize(this);
        }

        public int DataLength
        {
            get { return elementCount; }
        }

        public int ElementsPerVertex
        {
            get { return elementsPerVertex; }
        }

        public DataType DataType
        {
            get { return dataType; }
        }

        public DataValueType DataValueType
        {
            get { return dataValueType; }
        }

        public bool Initialize(int arrayAttrib)
        {
            BufferTarget target = Target;
            if (dataVboId != 0)
            {
                Bind(target, arrayAttrib);
                return dataVboId != 0;
            }

            if (data == null)
                throw new InvalidOperationException("Buffers can not be re-initialized");

            int sizeInBytes = (elementCount > 0 ? elementCount : allocatedDataElementCount) * Marshal.SizeOf(typeof(T));

            // Load data into OpenGL
            dataVboId = GL.GenBuffer();
            Bind(target, arrayAttrib);
            T[] dataVal = elementCount > 0 ? data : null;
            GL.BufferData(target, new IntPtr(sizeInBytes), dataVal, dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
            GlUtils.CheckGLErrors();
            data = null;
            return true;
        }

        /// <summary>
        /// Changes the data in this buffer to the specified new data. This is an error if the buffer is not dynamic.
        /// </summary>
        public void UpdateData<T2>(T2[] newData, int elementsToUpdate) where T2 : struct
        {
            if (!dynamic)
                throw new InvalidOperationException("Can not update the contents of a static buffer");

            BufferTarget target = Target;
            GL.BindBuffer(target, dataVboId);

            elementCount = elementsToUpdate;
            int sizeInBytes = elementsToUpdate * Marshal.SizeOf(typeof(T2));
            if (elementCount > allocatedDataElementCount)
                allocatedDataElementCount = elementCount;

            //int totalSizeInBytes = allocatedDataElementCount * Marshal.SizeOf(typeof(T2));
            GL.BufferData(target, new IntPtr(sizeInBytes), (T2[])null, BufferUsageHint.StreamDraw);
            GL.BufferSubData(target, new IntPtr(0), new IntPtr(sizeInBytes), newData);
            GlUtils.CheckGLErrors();
        }

        /// <summary>
        /// 
        /// </summary>
        public IntPtr MapData(int elementsToMap)
        {
            BufferTarget target = Target;
            GL.BindBuffer(target, dataVboId);

            elementCount = elementsToMap;
            int sizeInBytes = elementsToMap * Marshal.SizeOf(typeof(T));
            IntPtr result = GL.MapBufferRange(Target, new IntPtr(0), new IntPtr(sizeInBytes),
                BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
            GlUtils.CheckGLErrors();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnmapData()
        {
            BufferTarget target = Target;
            GL.BindBuffer(target, dataVboId);
            GL.UnmapBuffer(target);
            GlUtils.CheckGLErrors();
        }

        private void Bind(BufferTarget target, int arrayAttrib)
        {
            GL.BindBuffer(target, dataVboId);
            if (dataType != DataType.Index)
            {
                GL.EnableVertexAttribArray(arrayAttrib);
                GL.VertexAttribPointer(arrayAttrib, elementsPerVertex, PointerType, normalize, 0, 0);
            }

            if (dataType == DataType.Instance)
                GL.VertexAttribDivisor(arrayAttrib, 1);

            GlUtils.CheckGLErrors();
        }

        private VertexAttribPointerType PointerType
        {
            get
            {
                switch (dataValueType)
                {
                    case DataValueType.Byte: return VertexAttribPointerType.UnsignedByte;
                    case DataValueType.Short: return VertexAttribPointerType.Short;
                    case DataValueType.UShort: return VertexAttribPointerType.UnsignedShort;
                    case DataValueType.Int: return VertexAttribPointerType.Int;
                    case DataValueType.UInt: return VertexAttribPointerType.UnsignedInt;
                    case DataValueType.Float: return VertexAttribPointerType.Float;
                    default: throw new InvalidOperationException("Unknown value type: " + dataValueType);
                }
            }
        }

        private BufferTarget Target
        {
            get
            {
                switch (dataType)
                {
                    case DataType.Index: return BufferTarget.ElementArrayBuffer;
                    case DataType.Vertex:
                    case DataType.Instance:
                        return BufferTarget.ArrayBuffer;
                    default: throw new InvalidOperationException("Unknown buffer type: " + dataType);
                }
            }
        }
    }
}
