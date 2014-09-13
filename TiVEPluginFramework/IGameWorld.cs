namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IGameWorld
    {
        int BlockSizeX { get; }

        int BlockSizeY { get; }

        int BlockSizeZ { get; }

        BlockInformation this[int x, int y, int z] { get; set; }
    }
}
