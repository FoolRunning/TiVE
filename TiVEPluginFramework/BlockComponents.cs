using System;
using System.IO;
using System.Threading;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IBlockComponent : ITiVESerializable
    {
    }

    public sealed class LightComponent : IBlockComponent
    {
        public static readonly Guid ID = new Guid("67767135-A2B9-4FF6-A39E-CFE6F052CD6D");

        internal readonly Vector3b Location;
        internal readonly ushort LightBlockDist;
        internal readonly Color3f Color;

        public LightComponent(BinaryReader reader)
        {
            LightBlockDist = reader.ReadUInt16();
            Location = new Vector3b(reader);
            Color = new Color3f(reader);
        }

        public LightComponent(Vector3b location, Color3f color, int lightBlockDist)
        {
            if (lightBlockDist < 0 || lightBlockDist > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("lightBlockDist", "block distance must be between 0 and 65535");

            Location = location;
            Color = color;
            LightBlockDist = (ushort)lightBlockDist;
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(LightBlockDist);
            Location.SaveTo(writer);
            Color.SaveTo(writer);
        }
    }

    public sealed class AnimationComponent : IBlockComponent
    {
        public static readonly Guid ID = new Guid("B0D0B2C8-3732-49C6-B800-694DE5771E1C");

        public readonly float AnimationFrameTime;
        public readonly string NextBlockName;

        public AnimationComponent(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public AnimationComponent(int animationFrameTimeMs, string nextBlockName)
        {
            AnimationFrameTime = animationFrameTimeMs / 1000.0f;
            NextBlockName = nextBlockName;
        }

        public void SaveTo(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    #region VoxelNoiseComponent class
    public sealed class VoxelNoiseComponent : IBlockComponent
    {
        public static readonly Guid ID = new Guid("7100C34D-7025-4341-9272-23CB407E201A");

        private readonly RandomGenerator random = new RandomGenerator();
        private readonly float variationPercentage;
        private readonly Voxel[] voxelsToIgnore;

        public VoxelNoiseComponent(BinaryReader reader)
        {
            variationPercentage = reader.ReadSingle();
            int ignoredCount = reader.ReadByte();
            if (ignoredCount > 0)
            {
                voxelsToIgnore = new Voxel[ignoredCount];
                for (int i = 0; i < ignoredCount; i++)
                    voxelsToIgnore[i] = new Voxel(reader);
            }
        }

        public VoxelNoiseComponent(float variationPercentage, params Voxel[] voxelsToIgnore)
        {
            if (voxelsToIgnore.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException("voxelsToIgnore", "Maximum number of voxels to ignore is 255");

            this.variationPercentage = variationPercentage;
            this.voxelsToIgnore = voxelsToIgnore.Length > 0 ? voxelsToIgnore : null;
        }

        internal Voxel Adjust(Voxel voxel)
        {
            if (voxelsToIgnore != null)
            {
                for (int i = 0; i < voxelsToIgnore.Length; i++) // For loop for speed as this is a fairly performance-critical method
                {
                    if (voxelsToIgnore[i] == voxel)
                        return voxel;
                }
            }

            Monitor.Enter(random); // The Random class is not thread-safe and produces very strange results when multiple threads access it at the same time
            float rnd = random.NextFloat();
            Monitor.Exit(random);

            float scale = rnd * variationPercentage + (1.0f - variationPercentage / 2.0f);
            return new Voxel((byte)Math.Min((int)(voxel.R * scale), 255), (byte)Math.Min((int)(voxel.G * scale), 255), 
                (byte)Math.Min((int)(voxel.B * scale), 255), voxel.A, voxel.Settings);
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(variationPercentage);
            if (voxelsToIgnore == null)
                writer.Write((byte)0);
            else
            {
                writer.Write((byte)voxelsToIgnore.Length);
                foreach (Voxel voxel in voxelsToIgnore)
                    voxel.SaveTo(writer);
            }
        }
    }
    #endregion

    #region LightPassthroughComponent class
    public sealed class LightPassthroughComponent : IBlockComponent
    {
        public static readonly Guid ID = new Guid("2DCE92B9-B1C7-481E-9C4B-5691DE368A7F");

        public LightPassthroughComponent()
        {
        }

        public LightPassthroughComponent(BinaryReader reader)
        {
            // Nothing to do
        }

        public void SaveTo(BinaryWriter writer)
        {
            // Nothing to do
        }
    }
    #endregion
}
