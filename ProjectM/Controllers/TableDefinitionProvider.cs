using System.IO;
using System.Reflection;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectM.Controllers
{
    public class TableDefinitionProvider : ITableDefinitionProvider
    {
        public string GetTableDefinitionContents()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "additionalTables.def");
            return File.Exists(path) ? File.ReadAllText(path) : "";
        }
    }
}
