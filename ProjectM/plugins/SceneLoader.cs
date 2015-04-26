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
                IEntity entity = scene.CreateNewEntity("Camera");
                entity.AddComponent(new CameraComponent());
                entity.AddComponent(new ScriptComponent("Maze"));
            }
            else if (sceneName == "LiquidTest")
            {
                scene = Factory.Create<IScene>();
                scene.SetGameWorld("LiquidTest");
                IEntity entity = scene.CreateNewEntity("Camera");
                entity.AddComponent(new CameraComponent());
                entity.AddComponent(new ScriptComponent("LiquidTest"));
            }
            else if (sceneName == "StressTest")
            {
                scene = Factory.Create<IScene>();
                scene.SetGameWorld("StressTest");
                IEntity entity = scene.CreateNewEntity("Camera");
                entity.AddComponent(new CameraComponent());
                entity.AddComponent(new ScriptComponent("StressTest"));
            }

            return scene;
        }
        #endregion
    }
}
