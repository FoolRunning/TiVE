namespace ProdigalSoftware.TiVE.Renderer
{
    internal enum BufferType
    {
        Data,
        Indexes
    }

    /// <summary>
    /// Interface for data buffers
    /// </summary>
    internal interface IVertexData
    {
        /// <summary>
        /// Gets the number of vertexes in this buffer
        /// </summary>
        int VertexCount { get; }

        /// <summary>
        /// Deletes the rendering data that is being used up by this data buffer
        /// </summary>
        void Delete();

        /// <summary>
        /// Initializes this data buffer for use in rendering operations. This method can only be called once on a given data buffer.
        /// </summary>
        /// <param name="arrayAttrib">The index of this item in the buffer collection</param>
        /// <returns>True if successful, false otherwise</returns>
        bool Initialize(int arrayAttrib);
    }
}
