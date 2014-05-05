namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IGameWorld
    {
        int Xsize { get; }

        int Ysize { get; }

        int Zsize { get; }

        void SetBlock(int x, int y, int z, BlockInformation block);

        BlockInformation GetBlock(int x, int y, int z);
    }
}
