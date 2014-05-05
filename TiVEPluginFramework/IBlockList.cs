namespace ProdigalSoftware.TiVEPluginFramework
{
    public interface IBlockList
    {
        BlockInformation this[string blockName] { get; }
    }
}
