using System;

namespace ProdigalSoftware.TiVE.Core.Backend
{
    internal enum DataType
    {
        Vertex,
        Index,
        Instance,
    }

    internal enum DataValueType
    {
        Byte,
        Short,
        UShort,
        Int,
        UInt,
        Float,
    }

    /// <summary>
    /// Interface for data that can be used by the renderer backend
    /// </summary>
    internal interface IRendererData : IDisposable
    {
        /// <summary>
        /// Gets the number of data points in this buffer
        /// </summary>
        int DataLength { get; }

        /// <summary>
        /// Gets the number of data elements that are used for each vertex
        /// </summary>
        int ElementsPerVertex { get; }

        /// <summary>
        /// Gets the type of data stored in this buffer
        /// </summary>
        DataType DataType { get; }

        /// <summary>
        /// Gets the type of each value used for rendering in the buffer
        /// </summary>
        DataValueType DataValueType { get; }

        /// <summary>
        /// Initializes this buffer for use in rendering operations.
        /// </summary>
        /// <param name="arrayAttrib">The index of this item in the buffer collection</param>
        /// <returns>True if successful, false otherwise</returns>
        bool Initialize(int arrayAttrib);

        /// <summary>
        /// Changes the data in this buffer to the specified new data. This is an error if the buffer is not dynamic.
        /// </summary>
        void UpdateData<T>(T[] newData, int elementsToUpdate) where T : struct;

        /// <summary>
        /// 
        /// </summary>
        IntPtr MapData(int elementsToMap);

        /// <summary>
        /// 
        /// </summary>
        void UnmapData();
    }
}
