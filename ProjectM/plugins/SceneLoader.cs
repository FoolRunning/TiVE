using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
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
            else if (sceneName == "LiquidTest")
            {
                scene = Factory.Create<IScene>();
                scene.SetGameWorld("LiquidTest");
                IEntity entity = scene.CreateNewEntity("Camera");
                entity.AddComponent(new CameraComponent());
                entity.AddComponent(new ScriptComponent("LiquidTest"));
            }

            return scene;
        }
        #endregion
    }
}
