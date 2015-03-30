using System;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Particles
{
    internal struct RunningParticleSystem : IEquatable<RunningParticleSystem>
    {
        public readonly ParticleSystemComponent SystemInfo;

        private readonly int blockX;
        private readonly int blockY;
        private readonly int blockZ;

        public RunningParticleSystem(int blockX, int blockY, int blockZ)
        {
            this.blockX = blockX;
            this.blockY = blockY;
            this.blockZ = blockZ;
            SystemInfo = null;
        }

        public RunningParticleSystem(int blockX, int blockY, int blockZ, ParticleSystemComponent systemInfo)
        {
            this.blockX = blockX;
            this.blockY = blockY;
            this.blockZ = blockZ;
            SystemInfo = systemInfo;
        }

        public Vector3i WorldLocation
        {
            get
            {
                return new Vector3i(
                    blockX * Block.VoxelSize + SystemInfo.Location.X,
                    blockY * Block.VoxelSize + SystemInfo.Location.Y, 
                    blockZ * Block.VoxelSize + SystemInfo.Location.Z);
            }
        }

        public bool Equals(RunningParticleSystem other)
        {
            return other.blockX == blockX && other.blockY == blockY && other.blockZ == blockZ;
        }

        public override bool Equals(object obj)
        {
            RunningParticleSystem other = (RunningParticleSystem)obj;
            return other.blockX == blockX && other.blockY == blockY && other.blockZ == blockZ;
        }

        public override int GetHashCode()
        {
            // 12 bits for x and y and 8 bits for z (Enough for unique hashes for each block of a 4096x4096x256 world)
            return ((blockX & 0xFFF) << 20) ^ ((blockY & 0xFFF) << 8) ^ (blockZ & 0xFF);
        }

        public override string ToString()
        {
            return string.Format("SystemInfo ({0}, {1}, {2}) - {3}", blockX, blockY, blockZ, SystemInfo != null ? SystemInfo.Controller.ToString() : "{none}");
        }
    }
}
