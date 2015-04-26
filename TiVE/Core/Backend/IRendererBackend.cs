using System.Collections.Generic;
using JetBrains.Annotations;
using ProdigalSoftware.TiVE.Renderer;
using Rectangle = System.Drawing.Rectangle;

namespace ProdigalSoftware.TiVE.Core.Backend
{
    /// <summary>
    /// Types of primitives used to draw sets of vertexes
    /// </summary>
    internal enum PrimitiveType
    {
        /// <summary>Data will be drawn using points (i.e. each vertex will be a point)</summary>
        [PublicAPI] Points,
        /// <summary>Data will be drawn using lines (i.e. every two vertexes will be a line)</summary>
        Lines,
        /// <summary>Data will be drawn using triangles (i.e. every three vertexes will be a triangle)</summary>
        Triangles,
        /// <summary>Data will be drawn using quads (i.e. every four vertexes will be a quad)</summary>
        Quads
    }

    internal enum BlendMode
    {
        None,
        Realistic,
        Additive
    }

    /// <summary>
    /// Native display creation modes
    /// </summary>
    internal enum FullScreenMode
    {
        /// <summary>Display will be created inside a normal window</summary>
        Windowed,
        /// <summary>Display will be created by going into exclusive full-screen mode</summary>
        FullScreen,
        /// <summary>Display will be created using a window that takes up the whole screen (at the current resolution)</summary>
        WindowFullScreen
    }

    internal interface IControllerBackend
    {
        /// <summary>
        /// Gets an implementation of the keyboard interface
        /// </summary>
        IKeyboard Keyboard { get; }

        /// <summary>
        /// Gets an implementation of the mouse interface
        /// </summary>
        IMouse Mouse { get; }

        /// <summary>
        /// Gets a list of available display modes for the main display
        /// </summary>
        IEnumerable<DisplaySetting> AvailableDisplaySettings { get; }
        
        INativeDisplay CreateNatveDisplay(DisplaySetting displaySetting, FullScreenMode fullScreenMode, int antiAliasAmount, bool vsync);

        /// <summary>
        /// Called to perform one-time initialization to setup the render state
        /// </summary>
        void Initialize();

        /// <summary>
        /// Tell the renderer backend that the render window has changed to the specified bounds
        /// </summary>
        void WindowResized(Rectangle newClientBounds);

        /// <summary>
        /// Draws the specified data to the render output using the specified primitive type
        /// </summary>
        void Draw(PrimitiveType primitiveType, IVertexDataCollection data);

        /// <summary>
        /// Initialize the render state for a new frame
        /// </summary>
        void BeforeRenderFrame();

        IVertexDataCollection CreateVertexDataCollection();

        IRendererData CreateData<T>(T[] data, int elementCount, int elementsPerVertex, DataType dataType, 
            DataValueType dataValueType, bool normalize, bool dynamic) where T : struct;

        ITexture CreateTexture(int width, int height);
        
        IShaderProgram CreateShaderProgram();

        void SetBlendMode(BlendMode mode);

        /// <summary>
        /// Disables writing to the depth buffer
        /// </summary>
        void DisableDepthWriting();

        /// <summary>
        /// Enables writing to the depth buffer
        /// </summary>
        void EnableDepthWriting();

        /// <summary>
        /// Gets the path in the assembly for the shader definitions for the backend
        /// </summary>
        string GetShaderDefinitionFileResourcePath();
    }
}
