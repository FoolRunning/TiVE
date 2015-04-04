using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework.Components
{
    /// <summary>
    /// Component for entities that are controled with a Lua script.
    /// </summary>
    [PublicAPI]
    public sealed class ScriptComponent : IComponent
    {
        #region Internal data
        internal bool Loaded;
        #endregion

        public readonly string ScriptName;

        public ScriptComponent(string scriptName)
        {
            ScriptName = scriptName;
        }
    }
}
