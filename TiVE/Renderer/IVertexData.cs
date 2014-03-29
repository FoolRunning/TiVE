using System;

namespace ProdigalSoftware.TiVE.Renderer
{
    internal enum DataType
    {
        Vertex,
        Index,
        Instance,
    }

    internal enum ValueType
    {
        Byte,
        Short,
        Int,
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
        ValueType ValueType { get; }

        /// <summary>
        /// Initializes this buffer for use in rendering operations.
        /// </summary>
        /// <param name="arrayAttrib">The index of this item in the buffer collection</param>
        /// <returns>True if successful, false otherwise</returns>
        bool Initialize(int arrayAttrib);

        /// <summary>
        /// Locks the data so that it won't not be deleted (even if Delete() is called)
        /// </summary>
        void Lock();

        /// <summary>
        /// Unlocks the data so it can be deleted
        /// </summary>
        void Unlock();
    }
}
