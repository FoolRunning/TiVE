namespace ProdigalSoftware.TiVE.Renderer
{
    /// <summary>
    /// A collection of data buffers for rendering purposes
    /// </summary>
    internal interface IVertexDataCollection
    {
        /// <summary>
        /// Add the specified data to the collection
        /// </summary>
        void AddBuffer(IRendererData buffer);

        /// <summary>
        /// Deletes the resources used by all of the data in this collection
        /// </summary>
        void Delete();

        /// <summary>
        /// Initializes the data in this collection for use in rendering operations.
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        bool Initialize();

        /// <summary>
        /// Makes this data the current rendering data
        /// </summary>
        void Bind();
    }
}
