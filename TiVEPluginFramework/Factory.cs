namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Interface for objects that can create other objects
    /// </summary>
    public static class Factory
    {
        internal static IFactoryImpl Implementation;

        /// <summary>
        /// Creates a new instance of the specified interface
        /// </summary>
        public static T Create<T>()
        {
            return Implementation.Create<T>();
        }

        /// <summary>
        /// Creates a new instance of a block
        /// </summary>
        public static Block CreateBlock(string name)
        {
            return Implementation.CreateBlock(name);
        }

        /// <summary>
        /// Creates a new instance of a game world
        /// </summary>
        public static IGameWorld CreateGameWorld(int sizeX, int sizeY, int sizeZ)
        {
            return Implementation.CreateGameWorld(sizeX, sizeY, sizeZ);
        }

        internal interface IFactoryImpl
        {
            T Create<T>();

            Block CreateBlock(string name);

            IGameWorld CreateGameWorld(int blockSizeX, int blockSizeY, int blockSizeZ);
        }
    }
}
