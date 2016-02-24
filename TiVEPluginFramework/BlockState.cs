using System;
using System.IO;

namespace ProdigalSoftware.TiVEPluginFramework
{
    public enum BlockRotation
    {
        NotRotated = 0,
        NinetyCCW = 1,
        OneEightyCCW = 2,
        TwoSeventyCCW = 3
    }

    public struct BlockState : ITiVESerializable
    {
        public static readonly Guid ID = new Guid("FFB18914-311A-4EDA-8A58-65156696FED2");

        private const int RotationMask = 0x3;

        /// <summary>
        /// Compacted state information. Data is as follows (starting at the lowest bit):
        /// <para>2 bits = rotation</para>
        /// </summary>
        private int stateInfo;

        public BlockState(BinaryReader reader)
        {
            stateInfo = reader.ReadInt32();
        }

        public BlockRotation Rotation 
        {
            get { return (BlockRotation)(stateInfo & RotationMask); }
            set { stateInfo = (stateInfo & ~RotationMask) | (int)value;  }
        }

        public void SaveTo(BinaryWriter writer)
        {
            writer.Write(stateInfo);
        }
    }
}
