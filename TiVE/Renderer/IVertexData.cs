namespace ProdigalSoftware.TiVE.Renderer
{
    internal enum DataType
    {
        Vertex,
        Index,
        Instance,
    }

    /// <summary>
    /// Interface for data that can be used by the renderer backend
    /// </summary>
    internal interface IRendererData
    {
        /// <summary>
        /// Gets the number of data points in this buffer
        /// </summary>
        int DataLength { get; }

        /// <summary>
        /// Gets the type of data stored in this buffer
        /// </summary>
        DataType DataType { get; }

        /// <summary>
        /// Deletes the rendering data that is being used up by this buffer
        /// </summary>
        void Delete();

        /// <summary>
        /// Initializes this buffer for use in rendering operations.
        /// </summary>
        /// <param name="arrayAttrib">The index of this item in the buffer collection</param>
        /// <returns>True if successful, false otherwise</returns>
        bool Initialize(int arrayAttrib);
    }
}
