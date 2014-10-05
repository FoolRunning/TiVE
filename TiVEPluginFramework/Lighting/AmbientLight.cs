using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public sealed class AmbientLight : ILight
    {
        private readonly Color4f color;

        public AmbientLight(Color4f color)
        {
            this.color = color;
        }

        public float MaxVoxelDist
        {
            get { return float.MaxValue; }
        }

        public Vector3b Location
        {
            get { return new Vector3b(); }
        }

        public Color4f GetColorAtDistSquared(float dist)
        {
            return color;
        }
    }
}
