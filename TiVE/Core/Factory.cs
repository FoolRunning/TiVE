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

        public FactoryImpl()
        {
            //typeCreators.Add(typeof(IBlockList), new Func<IBlockList>(() => new BlockList()));
            typeCreators.Add(typeof(IScene), new Func<IScene>(() => new Scene()));
        }

        #region Implementation of IFactoryImpl
        /// <summary>
        /// Creates a new instance of the specified interface
        /// </summary>
        public T Create<T>()
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

        public Block CreateBlock(string name)
        {
            return new BlockImpl(name);
        }

        public IGameWorld CreateGameWorld(int blockSizeX, int blockSizeY, int blockSizeZ)
        {
            return new GameWorld(blockSizeX, blockSizeY, blockSizeZ);
        }
        #endregion
    }
}
