using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Plugins
{
    [UsedImplicitly]
    public class SceneLoader : ISceneGenerator
    {
        #region Implementation of ISceneGenerator
        public IScene CreateScene(string sceneName)
        {
            IScene scene = null;
            if (sceneName == "Loading")
            {
                scene = Factory.Create<IScene>();
            }
            else if (sceneName == "Maze")
            {
                scene = Factory.Create<IScene>();
                scene.SetGameWorld("Maze");
            }

            return scene;
        }
        #endregion
    }
}
