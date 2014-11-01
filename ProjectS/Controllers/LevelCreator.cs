using System;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.ProjectS.Controllers
{
    /// <summary>
    /// World generation stage to fill the game world with blocks.
    /// </summary>
    public class LevelCreator : IWorldGeneratorStage
    {
        /// <summary>
        /// Updates the specified gameworld with blocks
        /// </summary>
        public void UpdateWorld(IGameWorld gameWorld, long seed, IBlockList blockList)
        {
            BlockIncrementer lights = new BlockIncrementer(blockList, "Light", 3);
            BlockInformation wallBottom = blockList["WallBottom"];
            BlockInformation wallTop = blockList["WallTop"];
            BlockInformation wallCornerBottom = blockList["WallCornerBottom"];
            BlockInformation wallCornerTop = blockList["WallCornerTop"];
            BlockInformation wallEndBottom = blockList["WallEndBottom"];
            BlockInformation wallEndTop = blockList["WallEndTop"];
            BlockInformation floorBright = blockList["FloorWhite"];
            BlockInformation floorDark = blockList["FloorDark"];

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
                gameWorld[x, 0, 1] = wallBottom;
                gameWorld[x, 0, 2] = wallTop;

                gameWorld[x, gameWorld.BlockSize.Y - 1, 1] = wallBottom;
                gameWorld[x, gameWorld.BlockSize.Y - 1, 2] = wallTop;

                if (x % 5 == 0)
                {
                    gameWorld[x, 0, 3] = lights.NextBlock();
                    gameWorld[x, gameWorld.BlockSize.Y - 1, 3] = lights.NextBlock();
                }
            }

            for (int y = 0; y < gameWorld.BlockSize.Y; y++)
            {
                gameWorld[0, y, 1] = wallBottom.Rotate(BlockRotation.NinetyCCW);
                gameWorld[0, y, 2] = wallTop.Rotate(BlockRotation.NinetyCCW);

                gameWorld[gameWorld.BlockSize.X - 1, y, 1] = wallBottom.Rotate(BlockRotation.NinetyCCW);
                gameWorld[gameWorld.BlockSize.X - 1, y, 2] = wallTop.Rotate(BlockRotation.NinetyCCW);

                if (y % 5 == 0)
                {
                    gameWorld[0, y, 3] = lights.NextBlock();
                    gameWorld[gameWorld.BlockSize.X - 1, y, 3] = lights.NextBlock();
                }
            }

            gameWorld[0, 0, 1] = wallCornerBottom.Rotate(BlockRotation.OneEightyCCW);
            gameWorld[0, 0, 2] = wallCornerTop.Rotate(BlockRotation.OneEightyCCW);

            gameWorld[0, gameWorld.BlockSize.Y - 1, 1] = wallCornerBottom.Rotate(BlockRotation.NinetyCCW);
            gameWorld[0, gameWorld.BlockSize.Y - 1, 2] = wallCornerTop.Rotate(BlockRotation.NinetyCCW);

            gameWorld[gameWorld.BlockSize.X - 1, 0, 1] = wallCornerBottom.Rotate(BlockRotation.TwoSeventyCCW);
            gameWorld[gameWorld.BlockSize.X - 1, 0, 2] = wallCornerTop.Rotate(BlockRotation.TwoSeventyCCW);

            gameWorld[gameWorld.BlockSize.X - 1, gameWorld.BlockSize.Y - 1, 1] = wallCornerBottom;
            gameWorld[gameWorld.BlockSize.X - 1, gameWorld.BlockSize.Y - 1, 2] = wallCornerTop;

            for (int x = 20; x < 51; x++)
            {
                gameWorld[x, 20, 1] = wallBottom;
                //gameWorld[x, 20, 2] = wallTop;
                if (x % 5 == 0)
                    gameWorld[x, 20, 2] = lights.NextBlock();
            }
            gameWorld[19, 20, 1] = wallEndBottom.Rotate(BlockRotation.OneEightyCCW);
            //gameWorld[19, 20, 2] = wallEndTop.Rotate(BlockRotation.OneEightyCCW);
            gameWorld[51, 20, 1] = wallEndBottom;
            //gameWorld[51, 20, 2] = wallEndTop;
        }

        public ushort Priority
        {
            get { return 100; }
        }

        public string StageDescription
        {
            get { return "Loading Level"; }
        }

        private sealed class BlockIncrementer
        {
            private readonly BlockInformation[] blocks;
            private readonly int blockCount;
            private int currentBlock;

            public BlockIncrementer(IBlockList blockList, string blockname, int blockCount)
            {
                this.blockCount = blockCount;
                blocks = new BlockInformation[blockCount];
                for (int i = 0; i < blockCount; i++)
                    blocks[i] = blockList[blockname + i];
            }

            public BlockInformation NextBlock()
            {
                currentBlock = (currentBlock + 1) % blockCount;
                return blocks[currentBlock];
            }
        }
    }
}
