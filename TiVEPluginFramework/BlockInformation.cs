using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.TiVEPluginFramework.Particles;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public sealed class BlockInformation
    {
        /// <summary>Number of voxels that make up a block on each axis</summary>
        public const int BlockSize = 9;

        public static readonly BlockInformation Empty = new BlockInformation("Empty");

        public readonly string BlockName;
        public readonly ParticleSystemInformation ParticleSystem;
        public readonly ILight Light;

        private readonly uint[] voxels = new uint[BlockSize * BlockSize * BlockSize];

        public BlockInformation(BlockInformation toCopy, string blockName, ParticleSystemInformation particleSystem = null, ILight light = null)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            Array.Copy(toCopy.voxels, voxels, voxels.Length);
            BlockName = blockName;
            ParticleSystem = particleSystem;
            Light = light;
        }

        public BlockInformation(string blockName, ParticleSystemInformation particleSystem = null, ILight light = null)
        {
            if (blockName == null)
                throw new ArgumentNullException("blockName");

            BlockName = blockName;
            ParticleSystem = particleSystem;
            Light = light;
        }

        internal uint[] VoxelsArray
        {
            get { return voxels; }
        }

        /// <summary>
        /// Gets/sets the voxel at the specified location
        /// </summary>
        public uint this[int x, int y, int z]
        {
            get { return voxels[GetOffset(x, y, z)]; }
            set { voxels[GetOffset(x, y, z)] = value; }
        }

        public static BlockInformation FromFile(string path)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open), Encoding.ASCII))
            {
                string id = reader.ReadString();
                if (id != "TiVEb")
                    return null; // Not really a TiVE block file

                int blockSize = reader.ReadByte();
                if (blockSize != BlockSize)
                    return null; // Wrong block size

                BlockInformation block = new BlockInformation(Path.GetFileNameWithoutExtension(path));
                for (int i = 0; i < block.voxels.Length; i++)
                    block.voxels[i] = reader.ReadUInt32();
                return block;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetOffset(int x, int y, int z)
        {
            Debug.Assert(x >= 0 && x < BlockSize);
            Debug.Assert(y >= 0 && y < BlockSize);
            Debug.Assert(z >= 0 && z < BlockSize);

            return (z * BlockSize + x) * BlockSize + y; // y-axis major for speed
        }
    }
}
