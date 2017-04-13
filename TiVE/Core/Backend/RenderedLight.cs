using System.Runtime.InteropServices;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core.Backend
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RenderedLight
    {
        public Vector3f Location;
        public Color3f Color;
        public readonly float CachedValue;

        public RenderedLight(Vector3f location, Color3f color, float cachedValue)
        {
            Location = location;
            Color = color;
            CachedValue = cachedValue;
        }
    }
}
