using JetBrains.Annotations;
using MoonSharp.Interpreter;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that are controled with a Lua script.
    /// </summary>
    [PublicAPI]
    [MoonSharpUserData]
    public sealed class ScriptComponent : IComponent
    {
        internal bool Loaded;
        internal Script Script;
        internal DynValue UpdateFunctionCached;

        public string ScriptName;

        public ScriptComponent(string scriptName)
        {
            ScriptName = scriptName;
        }
    }
}
