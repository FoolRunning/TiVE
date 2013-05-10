namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IGameWorld
    {
        int Xsize { get; }

        int Ysize { get; }

        int Zsize { get; }

        void SetBlock(int x, int y, int z, ushort blockIndex);
        
        void SetBiome(int x, int y, int z, byte biomeId);

        ushort GetBlock(int x, int y, int z);
        
        byte GetBiome(int x, int y, int z);
    }
}
