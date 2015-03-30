using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Plugins
{
    /// <summary>
    /// World generation stage to fill the game world with blocks.
    /// </summary>
    public class LevelCreator : IWorldGenerator
    {
        #region Implementation of IWorldGenerator
        public string BlockListForWorld(string gameWorldName)
        {
            return gameWorldName == "Snake" ? "Snake16x16" : null;
        }

        public IGameWorld CreateGameWorld(string gameWorldName, IBlockList blockList)
        {
            if (gameWorldName != "Snake")
                return null;

            IGameWorld gameWorld = Factory.CreateGameWorld(41, 31, 4);

            ushort lightRed = blockList["Light_Red"];
            ushort lightGreen = blockList["Light_Green"];
            ushort lightBlue = blockList["Light_Blue"];
            ushort lightWhite = blockList["Light_White"];
            ushort wall = blockList["Wall_Bottom"];
            ushort wallCorner = blockList["Wall_Bottom_Corner"];
            ushort wallLight = blockList["Wall_Bottom_Light"];
            ushort floorBright = blockList["Floor_White"];
            ushort floorDark = blockList["Floor_Dark"];
            //ushort wallEnd = blockList["Wall_Bottom_End"];

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                bool bright = (x / 2) % 2 == 0;
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    if (y % 2 == 0)
                        bright = !bright;

                    gameWorld[x, y, 0] = bright ? floorBright : floorDark;
                }
            }

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                gameWorld[x, 0, 1] = wall;
                gameWorld[x, gameWorld.BlockSize.Y - 1, 1] = wall;

                if (x % 5 == 0)
                {
                    gameWorld[x, 0, 2] = lightGreen;
                    gameWorld[x, 0, 1] = wallLight;

                    gameWorld[x, gameWorld.BlockSize.Y - 1, 2] = lightGreen;
                    gameWorld[x, gameWorld.BlockSize.Y - 1, 1] = wallLight;
                }
            }

            for (int y = 0; y < gameWorld.BlockSize.Y; y++)
            {
                gameWorld[0, y, 1] = wall;//.Rotate(BlockRotation.NinetyCCW);
                gameWorld[gameWorld.BlockSize.X - 1, y, 1] = wall;//.Rotate(BlockRotation.NinetyCCW);

                if (y % 5 == 0)
                {
                    gameWorld[0, y, 2] = lightRed;
                    gameWorld[0, y, 1] = wallLight;//.Rotate(BlockRotation.NinetyCCW);

                    gameWorld[gameWorld.BlockSize.X - 1, y, 2] = lightRed;
                    gameWorld[gameWorld.BlockSize.X - 1, y, 1] = wallLight;//.Rotate(BlockRotation.NinetyCCW);
                }
            }

            gameWorld[0, 0, 1] = wallCorner;//.Rotate(BlockRotation.OneEightyCCW);
            gameWorld[0, gameWorld.BlockSize.Y - 1, 1] = wallCorner;//.Rotate(BlockRotation.NinetyCCW);
            gameWorld[gameWorld.BlockSize.X - 1, 0, 1] = wallCorner;//.Rotate(BlockRotation.TwoSeventyCCW);
            gameWorld[gameWorld.BlockSize.X - 1, gameWorld.BlockSize.Y - 1, 1] = wallCorner;

            for (int x = 10; x < 31; x++)
            {
                gameWorld[x, 15, 1] = wall;
                if (x % 5 == 0)
                {
                    gameWorld[x, 15, 2] = lightBlue;
                    gameWorld[x, 15, 1] = wallLight;
                }
            }
            //gameWorld[19, 20, 1] = wallEnd;//.Rotate(BlockRotation.OneEightyCCW);
            //gameWorld[51, 20, 1] = wallEnd;

            return gameWorld;
        }
        #endregion

        private sealed class BlockIncrementer
        {
            private readonly ushort[] blocks;
            private readonly int blockCount;
            private int currentBlock;

            public BlockIncrementer(IBlockList blockList, string blockname, int blockCount)
            {
                this.blockCount = blockCount;
                blocks = new ushort[blockCount];
                for (int i = 0; i < blockCount; i++)
                    blocks[i] = blockList[blockname + i];
            }

            public ushort NextBlock()
            {
                currentBlock = (currentBlock + 1) % blockCount;
                return blocks[currentBlock];
            }
        }
    }
}
