using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that are renderable
    /// </summary>
    [PublicAPI]
    public sealed class SpriteComponent : VoxelMeshComponent
    {
        public static readonly Guid ID = new Guid("2074F49A-4156-4A15-8FAE-111469704F71");
        private const byte SerializedFileVersion = 1;

        [UsedImplicitly] public BoundingBox BoundingBox;
        [UsedImplicitly] public readonly List<SpriteAnimation> Animations = new List<SpriteAnimation>();

        public SpriteComponent(BinaryReader reader)
        {
            byte fileVersion = reader.ReadByte();
            if (fileVersion > SerializedFileVersion)
                throw new FileTooNewException("SpriteComponent");

            Location = new Vector3f(reader);
            BoundingBox = new BoundingBox(reader);
        }

        public SpriteComponent(Vector3f location, BoundingBox boundingBox) : base(location)
        {
            BoundingBox = boundingBox;
        }

        #region Overrides of VoxelMeshComponent
        public override void SaveTo(BinaryWriter writer)
        {
            writer.Write(SerializedFileVersion);
            Location.SaveTo(writer);
            BoundingBox.SaveTo(writer);
        }
        #endregion
    }
}
