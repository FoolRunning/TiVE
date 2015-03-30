namespace ProdigalSoftware.TiVEPluginFramework.Generators
{
    /// <summary>
    /// Provides additional table definitions for TiVE
    /// </summary>
    public interface ITableDefinitionProvider
    {
        /// <summary>
        /// Gets a table definition file contents containing the additional table definitions
        /// </summary>
        string GetTableDefinitionContents();
    }
}
