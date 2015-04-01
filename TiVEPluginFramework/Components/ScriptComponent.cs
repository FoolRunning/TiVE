using MoonSharp.Interpreter;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that are controled with a Lua script.
    /// </summary>
    public sealed class ScriptComponent : IComponent
    {
        internal bool Loaded;
        internal Script Script;

        public string ScriptName;

        public ScriptComponent(string scriptName)
        {
            ScriptName = scriptName;
        }
    }
}
