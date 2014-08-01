using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public sealed class AmbientLight : ILight
    {
        private readonly Color4b color;

        public AmbientLight(Color4b color)
        {
            this.color = color;
        }

        public float MaxDist
        {
            get { return float.MaxValue; }
        }

        public Color4b GetColorAtDist(float dist)
        {
            return color;
        }
    }
}
