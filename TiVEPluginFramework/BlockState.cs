namespace ProdigalSoftware.TiVEPluginFramework
{
    public enum BlockRotation
    {
        NotRotated = 0,
        NinetyCCW = 1,
        OneEightyCCW = 2,
        TwoSeventyCCW = 3
    }

    public struct BlockState
    {
        private const int RotationMask = 0x3;

        /// <summary>
        /// Compacted state information. Data is as follows (starting at the lowest bit):
        /// <para>2 bits = rotation</para>
        /// </summary>
        private int stateInfo;

        public BlockRotation Rotation 
        {
            get { return (BlockRotation)(stateInfo & RotationMask); }
            set { stateInfo &= (int)value; }
        }
    }
}
