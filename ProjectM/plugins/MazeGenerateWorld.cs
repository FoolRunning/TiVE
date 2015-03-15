﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.ProjectM.Plugins
{
    [UsedImplicitly]
    public class GenerateMazeWorld : IWorldGenerator
    {
        private enum Direction { Up, Down, Left, Right, None }

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public string BlockListForWorld(string gameWorldName)
        {
            return gameWorldName == "Maze" ? "maze" : null;
        }
        
        public GameWorld CreateGameWorld(string gameWorldName, IBlockList blockList)
        {
            if (gameWorldName != "Maze")
                return null;

            GameWorld gameWorld = new GameWorld(513, 513, 12); // Width and height must be divisible by 3
            gameWorld.LightingModelType = LightingModelType.Fantasy2;

            Random random = new Random();

            List<Vector3i> rooms = CreateRandomRooms(100, random, 3, 11, 3, 11).ToList();

            MazeCell[,] dungeonMap = new MazeCell[gameWorld.BlockSize.X / 3, gameWorld.BlockSize.Y / 3];
            PlaceRooms(rooms, dungeonMap, 25, random);
            int mazeAreaId = rooms.Count;
            FillBlankWithMaze(dungeonMap, random, mazeAreaId);

            //Console.WriteLine("\n\nFinished Maze:");
            //PrintDungeon(dungeonMap);

            FillWorld(gameWorld, blockList, dungeonMap, mazeAreaId);
            SmoothWorld(gameWorld, blockList);

            return gameWorld;
        }

        private static IEnumerable<Vector3i> CreateRandomRooms(int numOfRooms, Random random, 
            int minWidth, int maxWidth, int minHeight, int maxHeight)
        {
            for (int i = 0; i < numOfRooms; i++)
            {
                int roomWidth = minWidth + random.Next((maxWidth - minWidth + 2) / 2) * 2;
                int roomHeight = minHeight + random.Next((maxHeight - minHeight + 2) / 2) * 2;
                //Console.WriteLine("Created room sized: {0}, {1}, {2}", roomWidth, roomHeight, roomDepth);
                yield return new Vector3i(roomWidth, roomHeight, 0);
            }
        }

        private static void PlaceRooms(List<Vector3i> rooms, MazeCell[,] dungeonMap, int maxTries, Random random)
        {
            int maxWidth = dungeonMap.GetLength(0) - 2;
            int maxHeight = dungeonMap.GetLength(1) - 2;

            int areaId = 1;
            foreach (Vector3i room in rooms)
            {
                int tries = 0;
                bool placed;
                do
                {
                    int roomX = 1 + random.Next((maxWidth - room.X + 2) / 2) * 2;
                    int roomY = 1 + random.Next((maxHeight - room.Y + 2) / 2) * 2;
                    placed = TryPlaceRoom(room, roomX, roomY, dungeonMap, areaId);
                }
                while (!placed && tries++ < maxTries);
                
                if (placed)
                    areaId++;
            }
        }

        private static bool TryPlaceRoom(Vector3i room, int roomX, int roomY, MazeCell[,] dungeonMap, int areaId)
        {
            for (int x = 0; x < room.X; x++)
            {
                for (int y = 0; y < room.Y; y++)
                {
                    if (dungeonMap[x + roomX, y + roomY].AreaId != 0)
                        return false;
                }
            }

            for (int x = 0; x < room.X; x++)
            {
                for (int y = 0; y < room.Y; y++)
                    dungeonMap[x + roomX, y + roomY].AreaId = areaId;
            }
            return true;
        }

        private static void FillBlankWithMaze(MazeCell[,] dungeonMap, Random random, int areaId)
        {
            List<MazeCellLocation> cellsInMaze = new List<MazeCellLocation>();

            MazeCellLocation cell = FindBlankSpace(dungeonMap, random);
            cellsInMaze.Add(cell);
            dungeonMap[cell.X, cell.Y].AreaId = areaId;

            Direction lastDir = Direction.None;
            bool searchingForNewCell = false;
            while (cellsInMaze.Count > 0)
            {
                Direction dir = cell != MazeCellLocation.NONE ? ChooseRandomDirection(cell, dungeonMap, random) : Direction.None;
                if (dir == Direction.None)
                {
                    // Couldn't continue with current road so find a good place for another one
                    if (!searchingForNewCell && cell != MazeCellLocation.NONE)
                        dungeonMap[cell.X, cell.Y].LightId = 1;

                    searchingForNewCell = true;
                    cellsInMaze.RemoveAt(cellsInMaze.Count - 1);
                    int rand = random.Next(100);
                    if (cellsInMaze.Count > 0)
                    {
                        if (rand < 75)
                            cell = cellsInMaze[cellsInMaze.Count - 1];
                        else if (rand >= 75)
                            cell = cellsInMaze[random.Next(cellsInMaze.Count)];
                    }
                    else
                    {
                        cell = FindBlankSpace(dungeonMap, random);
                        if (cell != MazeCellLocation.NONE)
                        {
                            cellsInMaze.Add(cell);
                            dungeonMap[cell.X, cell.Y].AreaId = areaId;
                        }
                    }
                }
                else
                {
                    searchingForNewCell = false;
                    if (dir != lastDir)
                        dungeonMap[cell.X, cell.Y].LightId = 1;

                    switch (dir)
                    {
                        case Direction.Up:
                            dungeonMap[cell.X, cell.Y + 1].AreaId = areaId;
                            dungeonMap[cell.X, cell.Y + 2].AreaId = areaId;
                            cell = new MazeCellLocation(cell.X, cell.Y + 2);
                            break;
                        case Direction.Down:
                            dungeonMap[cell.X, cell.Y - 1].AreaId = areaId;
                            dungeonMap[cell.X, cell.Y - 2].AreaId = areaId;
                            cell = new MazeCellLocation(cell.X, cell.Y - 2);
                            break;
                        case Direction.Left:
                            dungeonMap[cell.X - 1, cell.Y].AreaId = areaId;
                            dungeonMap[cell.X - 2, cell.Y].AreaId = areaId;
                            cell = new MazeCellLocation(cell.X - 2, cell.Y);
                            break;
                        case Direction.Right:
                            dungeonMap[cell.X + 1, cell.Y].AreaId = areaId;
                            dungeonMap[cell.X + 2, cell.Y].AreaId = areaId;
                            cell = new MazeCellLocation(cell.X + 2, cell.Y);
                            break;
                    }

                    cellsInMaze.Add(cell);
                    dungeonMap[cell.X, cell.Y].AreaId = areaId;
                }
                lastDir = dir;
            }
        }

        private static MazeCellLocation FindBlankSpace(MazeCell[,] dungeonMap, Random random)
        {
            const int maxTries = 10000;

            int maxWidth = dungeonMap.GetLength(0) - 3;
            int maxHeight = dungeonMap.GetLength(1) - 3;

            for (int tries = 0; tries < maxTries; tries++)
            {
                int testX = 1 + random.Next((maxWidth + 2) / 2) * 2;
                int testY = 1 + random.Next((maxHeight + 2) / 2) * 2;

                if (dungeonMap[testX, testY].AreaId == 0 &&
                    ((testX >= 3 && dungeonMap[testX - 2, testY].AreaId == 0) ||
                    (testY >= 3 && dungeonMap[testX, testY - 2].AreaId == 0) ||
                    (testX < dungeonMap.GetLength(0) - 3 && dungeonMap[testX + 2, testY].AreaId == 0) ||
                    (testY < dungeonMap.GetLength(1) - 3 && dungeonMap[testX, testY + 2].AreaId == 0)))
                {
                    return new MazeCellLocation(testX, testY);
                }
            }

            return MazeCellLocation.NONE;
        }

        private static Direction ChooseRandomDirection(MazeCellLocation cell, MazeCell[,] dungeonMap, Random random)
        {
            const int maxTries = 10000;

            int maxWidth = dungeonMap.GetLength(0);
            int maxHeight = dungeonMap.GetLength(1);

            for (int tries = 0; tries < maxTries; tries++)
            {
                switch (random.Next(4))
                {
                    case 0:
                        if (cell.Y < maxHeight - 3 && dungeonMap[cell.X, cell.Y + 2].AreaId == 0)
                            return Direction.Up;
                        break;
                    case 1:
                        if (cell.Y >= 3 && dungeonMap[cell.X, cell.Y - 2].AreaId == 0)
                            return Direction.Down;
                        break;
                    case 2:
                        if (cell.X >= 3 && dungeonMap[cell.X - 2, cell.Y].AreaId == 0)
                            return Direction.Left;
                        break;
                    case 3:
                        if (cell.X < maxWidth - 3 && dungeonMap[cell.X + 2, cell.Y].AreaId == 0)
                            return Direction.Right;
                        break;
                }
            }

            return Direction.None;
        }
        
        private static void PrintDungeon(int[,] dungeonMap)
        {
            for (int y = dungeonMap.GetLength(1) - 1; y >= 0; y--)
            {
                string d = "";
                for (int x = 0; x < dungeonMap.GetLength(0); x++)
                    d += dungeonMap[x, y] > 0 ? dungeonMap[x, y].ToString("D2") : "  ";
                Console.WriteLine(d);
            }
        }

        private static void FillWorld(GameWorld gameWorld, IBlockList blockList, MazeCell[,] dungeonMap, int mazeAreaId)
        {
            BlockRandomizer backWalls = new BlockRandomizer(blockList, "back", 6);
            BlockRandomizer grasses = new BlockRandomizer(blockList, "grass", 6);
            ushort stone = blockList["ston0"];
            ushort light = blockList["light0"];
            ushort fire = blockList["fire"];
            ushort lava = blockList["lava0"];
            ushort fountain = blockList["fountain"];

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    int mazeLocX = x / 3;
                    int mazeLocY = y / 3;
                    int mazeId = dungeonMap[mazeLocX, mazeLocY].AreaId;
                    if (mazeId == 0)
                    {
                        gameWorld[x, y, 4] = stone;
                        gameWorld[x, y, 5] = stone;
                        gameWorld[x, y, 6] = stone;
                        gameWorld[x, y, 7] = stone;
                        gameWorld[x, y, 8] = stone;
                        gameWorld[x, y, 9] = stone;
                        //gameWorld[x, y, 10] = stone;
                    }
                    else if (mazeId == mazeAreaId)
                    {
                        gameWorld[x, y, 4] = backWalls.NextBlock();
                        gameWorld[x, y, 5] = grasses.NextBlock();
                    }
                    else
                    {
                        gameWorld[x, y, 4] = lava;
                    }

                    if (x % 3 == 1 && y % 3 == 1 && dungeonMap[mazeLocX, mazeLocY].LightId != 0)
                    {
                        int lightLocX = x;
                        int lightLocY = y;

                        if (mazeLocX == 0 || dungeonMap[mazeLocX - 1, mazeLocY].AreaId == 0)
                            lightLocX--;

                        if (mazeLocX == dungeonMap.GetLength(0) - 1 || dungeonMap[mazeLocX + 1, mazeLocY].AreaId == 0)
                            lightLocX++;

                        if (mazeLocY == 0 || dungeonMap[mazeLocX, mazeLocY - 1].AreaId == 0)
                            lightLocY--;

                        if (mazeLocY == dungeonMap.GetLength(1) - 1 || dungeonMap[mazeLocX, mazeLocY + 1].AreaId == 0)
                            lightLocY++;

                        gameWorld[lightLocX, lightLocY, 6] = light;
                    }
                }
            }
        }

        private static void SmoothWorld(GameWorld gameWorld, IBlockList blockList)
        {
            HashSet<int> blocksToConsiderEmpty = new HashSet<int>();
            blocksToConsiderEmpty.Add(0);
            for (int i = 0; i < 6; i++)
                blocksToConsiderEmpty.Add(blockList["grass" + i]);

            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = 0; x < gameWorld.BlockSize.X; x++)
                {
                    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    {
                        ushort blockIndex = gameWorld[x, y, z];
                        if (blockIndex == 0)
                            continue;

                        BlockInformation block = blockList[blockIndex];
                        string blockNameKey = GetBlockSet(block);

                        if (blockNameKey != "ston" && blockNameKey != "sand" && blockNameKey != "lava")
                            continue;

                        int sides = 0;

                        if (z == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z - 1]))
                            sides |= Back;

                        if (z == gameWorld.BlockSize.Z - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z + 1]))
                            sides |= Front;

                        if (x == 0 || blocksToConsiderEmpty.Contains(gameWorld[x - 1, y, z]))
                            sides |= Left;

                        if (x == gameWorld.BlockSize.X - 1 || blocksToConsiderEmpty.Contains(gameWorld[x + 1, y, z]))
                            sides |= Right;

                        if (y == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y - 1, z]))
                            sides |= Bottom;

                        if (y == gameWorld.BlockSize.Y - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y + 1, z]))
                            sides |= Top;

                        gameWorld[x, y, z] = blockList[blockNameKey + sides];
                    }
                }
            }
        }

        private static string GetBlockSet(BlockInformation block)
        {
            return block.BlockName.Substring(0, 4);
        }

        #region MazeCellLocation structure
        private struct MazeCellLocation
        {
            public static readonly MazeCellLocation NONE = new MazeCellLocation(-1, -1);

            public readonly int X;
            public readonly int Y;

            public MazeCellLocation(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static bool operator ==(MazeCellLocation c1, MazeCellLocation c2)
            {
                return c1.X == c2.X && c1.Y == c2.Y;
            }

            public static bool operator !=(MazeCellLocation c1, MazeCellLocation c2)
            {
                return c1.X != c2.X || c1.Y != c2.Y;
            }
        }
        #endregion

        private struct MazeCell
        {
            public int AreaId;
            public int LightId;
        }

        #region BlockRandomizer class
        private sealed class BlockRandomizer
        {
            public readonly ushort[] Blocks;
            private readonly Random random = new Random();

            public BlockRandomizer(IBlockList blockList, string blockname, int blockCount)
            {
                Blocks = new ushort[blockCount];
                for (int i = 0; i < Blocks.Length; i++)
                    Blocks[i] = blockList[blockname + i];
            }

            public ushort NextBlock()
            {
                int blockNum = random.Next(Blocks.Length);
                return Blocks[blockNum];
                //return Blocks[Blocks.Length - 1];
            }
        }
        #endregion
    }
}
