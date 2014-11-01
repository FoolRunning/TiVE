using OpenTK;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface ICamera
    {
        Matrix4 ViewMatrix { get; }

        Matrix4 ProjectionMatrix { get; }

        Vector3 Location { get; }

        Vector3 LookAtLocation { get; }

        float AspectRatio { get;  }

        float FoV { get; set; }

        void SetViewport(int width, int height);

        void SetLocation(float x, float y, float z);

        void SetLookAtLocation(float x, float y, float z);

        void Update();
    }
}
