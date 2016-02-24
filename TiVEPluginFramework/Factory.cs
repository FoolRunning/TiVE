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
        public static T New<T>()
        {
            return Implementation.New<T>();
        }

        public static T Get<T>(string name) where T : ITiVESerializable
        {
            return Implementation.Get<T>(name);
        }

        /// <summary>
        /// Creates a new instance of a game world
        /// </summary>
        public static IGameWorld NewGameWorld(int sizeX, int sizeY, int sizeZ)
        {
            return Implementation.NewGameWorld(sizeX, sizeY, sizeZ);
        }

        internal interface IFactoryImpl
        {
            T New<T>();

            T Get<T>(string name) where T : ITiVESerializable;

            IGameWorld NewGameWorld(int blockSizeX, int blockSizeY, int blockSizeZ);
        }
    }
}
