using System;
using System.Collections.Generic;
using ProdigalSoftware.TiVE.RenderSystem.World;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class FactoryImpl : Factory.IFactoryImpl
    {
        private readonly Dictionary<Type, Delegate> typeCreators = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, Delegate> typeLoaders = new Dictionary<Type, Delegate>();

        public FactoryImpl()
        {
            //typeCreators.Add(typeof(IBlockList), new Func<IBlockList>(() => new BlockList()));
            typeCreators.Add(typeof(IScene), new Func<IScene>(() => new Scene()));
            typeLoaders.Add(typeof(Block), new Func<string, Block>(TiVEController.BlockManager.GetBlock));
        }

        #region Implementation of IFactoryImpl
        public T New<T>()
        {
            Delegate creator;
            if (typeCreators.TryGetValue(typeof(T), out creator))
            {
                Func<T> tCreator = creator as Func<T>;
                if (tCreator != null)
                    return tCreator();
            }

            Messages.AddError("No creator found for type " + typeof(T) + "()");
            return default(T);
        }

        public T Get<T>(string name) where T : ITiVESerializable
        {
            Delegate loader;
            if (typeLoaders.TryGetValue(typeof(T), out loader))
            {
                Func<string, T> tLoader = loader as Func<string, T>;
                if (tLoader != null)
                    return tLoader(name);
            }

            Messages.AddError("No loader found for type " + typeof(T) + "()");
            return default(T);
        }

        public IGameWorld NewGameWorld(int blockSizeX, int blockSizeY, int blockSizeZ)
        {
            return new GameWorld(blockSizeX, blockSizeY, blockSizeZ);
        }
        #endregion
    }
}
