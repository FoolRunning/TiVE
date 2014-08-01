using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVEPluginFramework.Lighting
{
    public sealed class DirectionalLight : ILight
    {
        private readonly Color4b brightest;

        public DirectionalLight(Vector3b angle, Color4b brightest)
        {
            Angle = angle;
            this.brightest = brightest;
        }

        public Vector3b Angle { get; private set; }

        public float MaxDist 
        {
            get { return float.MaxValue; }
        }

        public Color4b GetColorAtDist(float dist)
        {
            return brightest;
        }
    }
}
