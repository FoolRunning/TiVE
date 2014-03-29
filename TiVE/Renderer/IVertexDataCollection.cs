using System;

namespace ProdigalSoftware.TiVE.Renderer
{
    /// <summary>
    /// A collection of data buffers for rendering purposes
    /// </summary>
    internal interface IVertexDataCollection : IDisposable
    {
        /// <summary>
        /// Gets whether or not the buffer collection has been initialized (thread-safe)
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Add the specified data to the collection
        /// </summary>
        void AddBuffer(IRendererData buffer);

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
