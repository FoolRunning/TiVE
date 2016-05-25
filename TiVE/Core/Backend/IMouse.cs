using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core.Backend
{
    internal interface IMouse
    {
        Vector2i Location { get; }
        int WheelLocation { get; }
    }
}
