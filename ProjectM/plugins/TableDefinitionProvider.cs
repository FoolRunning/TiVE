using System.IO;
using System.Reflection;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Plugins
{
    public class TableDefinitionProvider : ITableDefinitionProvider
    {
        public string GetTableDefinitionContents()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Data", "additionalTables.def");
            return File.Exists(path) ? File.ReadAllText(path) : "";
        }
    }
}
