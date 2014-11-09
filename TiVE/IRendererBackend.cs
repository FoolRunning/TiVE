using System.Drawing;
using ProdigalSoftware.TiVE.Renderer;

namespace ProdigalSoftware.TiVE
{
    internal enum PrimitiveType
    {
        Points,
        Lines,
        Triangles,
        Quads,
    }

    internal enum BlendMode
    {
        None,
        Realistic,
        Additive,
    }

    internal interface IRendererBackend
    {
        INativeWindow CreateNatveWindow(int width, int height, bool fullScreen, bool vsync);

        void Initialize();

        void WindowResized(Rectangle newBounds);

        void Draw(PrimitiveType primitiveType, IVertexDataCollection data);

        void BeforeRenderFrame();

        IVertexDataCollection CreateVertexDataCollection();

        IRendererData CreateData<T>(T[] data, int elementCount, int elementsPerVertex, DataType dataType, 
            ValueType valueType, bool normalize, bool dynamic) where T : struct;
        
        IShaderProgram CreateShaderProgram();

        string GetShaderDefinitionFileResourcePath();

        void SetBlendMode(BlendMode mode);

        void DisableDepthWriting();

        void EnableDepthWriting();
    }
}
