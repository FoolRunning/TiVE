using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.ProjectM.Plugins
{
    [UsedImplicitly]
    public class MazeGenerateWorld : IWorldGenerator
    {
        private const int RoomLightId = 100;
        private const int DoorAreaId = 2100;

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
        
        public IGameWorld CreateGameWorld(string gameWorldName, IBlockList blockList)
        {
            if (gameWorldName != "Maze")
                return null;

            IGameWorld gameWorld = Factory.CreateGameWorld(513, 513, 12); // x-axis and y-axis must be divisible by 3
            gameWorld.LightingModelType = LightingModelType.Realistic;
            gameWorld.DoLightCulling = true;

            Random random = new Random();

            MazeCell[,] dungeonMap = new MazeCell[gameWorld.BlockSize.X / 3, gameWorld.BlockSize.Y / 3];
            List<Vector3i> rooms = CreateRandomRooms(150, random, 3, 11, 3, 11).ToList();
            int mazeStartAreaId = rooms.Count + 5;
            PlaceRooms(rooms, dungeonMap, 25, random);
            FillBlankWithMaze(dungeonMap, random, mazeStartAreaId);
            CreateDoors(dungeonMap, random);
            CleanUpDeadEnds(dungeonMap, 10);

            //Console.WriteLine("\n\nFinished Maze:");
            //PrintDungeon(dungeonMap);

            FillWorld(gameWorld, blockList, dungeonMap, mazeStartAreaId);
            SmoothWorld(gameWorld, blockList);

            return gameWorld;
        }

        #region Room placement methods
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
                while (!placed && (areaId == 1 || tries++ < maxTries)); // First room must always be placed

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
                {
                    dungeonMap[x + roomX, y + roomY].AreaId = areaId;

                    if ((x == 1 || x == room.X - 2) && (y == 1 || y == room.Y - 2))
                        dungeonMap[x + roomX, y + roomY].LightId = RoomLightId;
                }
            }
            return true;
        }
        #endregion

        #region Maze generation methods
        private static void FillBlankWithMaze(MazeCell[,] dungeonMap, Random random, int mazeAreaId)
        {
            List<MazeCellLocation> cellsInMaze = new List<MazeCellLocation>();

            MazeCellLocation cell = FindBlankSpace(dungeonMap, random);
            cellsInMaze.Add(cell);
            dungeonMap[cell.X, cell.Y].AreaId = mazeAreaId;

            Direction lastDir = Direction.None;
            bool searchingForNewCell = false;
            int lightId = 0;
            while (cellsInMaze.Count > 0)
            {
                Direction dir = cell != MazeCellLocation.None ? ChooseRandomDirection(cell, dungeonMap, random) : Direction.None;
                if (dir == Direction.None)
                {
                    // Couldn't continue with current road so find a good place for another one
                    if (!searchingForNewCell && cell != MazeCellLocation.None)
                        dungeonMap[cell.X, cell.Y].LightId = lightId + 1;

                    lightId = (lightId + 1) % 6;
                    searchingForNewCell = true;
                    cellsInMaze.RemoveAt(cellsInMaze.Count - 1);
                    if (cellsInMaze.Count > 0)
                    {
                        // There are still cells in our current path, so decide what to do:
                        // * 75% chance to backup to the nearest previous part of the maze path
                        // * 25% chance to start from a random place on the maze path
                        int rand = random.Next(100);
                        if (rand < 75)
                            cell = cellsInMaze[cellsInMaze.Count - 1];
                        else if (rand >= 75)
                            cell = cellsInMaze[random.Next(cellsInMaze.Count)];
                    }
                    else
                    {
                        // There are no more cells in the maze path so we need to find a brand new location to start a new maze path
                        cell = FindBlankSpace(dungeonMap, random);
                        if (cell != MazeCellLocation.None)
                        {
                            cellsInMaze.Add(cell);
                            dungeonMap[cell.X, cell.Y].AreaId = ++mazeAreaId;
                        }
                    }
                }
                else
                {
                    // Found a place for a new cell so continue adding cells to the current maze path
                    searchingForNewCell = false;
                    if (dir != lastDir && lastDir != Direction.None)
                        dungeonMap[cell.X, cell.Y].LightId = lightId + 1;

                    switch (dir)
                    {
                        case Direction.Up:
                            dungeonMap[cell.X, cell.Y + 1].AreaId = mazeAreaId;
                            dungeonMap[cell.X, cell.Y + 2].AreaId = mazeAreaId;
                            cell = new MazeCellLocation(cell.X, cell.Y + 2);
                            break;
                        case Direction.Down:
                            dungeonMap[cell.X, cell.Y - 1].AreaId = mazeAreaId;
                            dungeonMap[cell.X, cell.Y - 2].AreaId = mazeAreaId;
                            cell = new MazeCellLocation(cell.X, cell.Y - 2);
                            break;
                        case Direction.Left:
                            dungeonMap[cell.X - 1, cell.Y].AreaId = mazeAreaId;
                            dungeonMap[cell.X - 2, cell.Y].AreaId = mazeAreaId;
                            cell = new MazeCellLocation(cell.X - 2, cell.Y);
                            break;
                        case Direction.Right:
                            dungeonMap[cell.X + 1, cell.Y].AreaId = mazeAreaId;
                            dungeonMap[cell.X + 2, cell.Y].AreaId = mazeAreaId;
                            cell = new MazeCellLocation(cell.X + 2, cell.Y);
                            break;
                    }

                    cellsInMaze.Add(cell);
                    dungeonMap[cell.X, cell.Y].AreaId = mazeAreaId;
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

            return MazeCellLocation.None;
        }

        private static Direction ChooseRandomDirection(MazeCellLocation cell, MazeCell[,] dungeonMap, Random random)
        {
            int maxWidth = dungeonMap.GetLength(0);
            int maxHeight = dungeonMap.GetLength(1);
            bool availableUp = (cell.Y < maxHeight - 3 && dungeonMap[cell.X, cell.Y + 2].AreaId == 0);
            bool availableDown = (cell.Y >= 3 && dungeonMap[cell.X, cell.Y - 2].AreaId == 0);
            bool availableLeft = (cell.X >= 3 && dungeonMap[cell.X - 2, cell.Y].AreaId == 0);
            bool availableRight = (cell.X < maxWidth - 3 && dungeonMap[cell.X + 2, cell.Y].AreaId == 0);

            if (!availableLeft && !availableRight && !availableUp && !availableDown)
                return Direction.None;

            while (true)
            {
                int randomDir = random.Next(4);
                if (availableUp && randomDir == 0) 
                    return Direction.Up;
                if (availableDown && randomDir == 1)
                    return Direction.Down;
                if (availableLeft && randomDir == 2)
                    return Direction.Left;
                if (availableRight && randomDir == 3)
                    return Direction.Right;
            }
        }
        #endregion

        #region Door placement methods
        private static void CreateDoors(MazeCell[,] dungeonMap, Random random)
        {
            // Find possible door points at places where rooms and maze (or another room) meet
            FindDoorPoints(dungeonMap);

            HashSet<int> connectedAreaIds = new HashSet<int>();
            connectedAreaIds.Add(1); // Start with the first room

            int tries = 0;
            while (tries++ < 1000)
            {
                int foundUnconnectedAreaId = -1;

                for (int x = 1; x < dungeonMap.GetLength(0) - 1; x++)
                {
                    for (int y = 1; y < dungeonMap.GetLength(1) - 1; y++)
                    {
                        if (dungeonMap[x, y].PossibleDoorPoint && random.Next(100) < 5 && ConnectedAreaAround(dungeonMap, connectedAreaIds, x, y))
                        {

                            int unconnectedAreaId = UnconnectedAreaIdAround(dungeonMap, connectedAreaIds, x, y);
                            if (unconnectedAreaId != -1 && (foundUnconnectedAreaId == -1 || unconnectedAreaId == foundUnconnectedAreaId))
                            {
                                dungeonMap[x, y].AreaId = DoorAreaId;
                                foundUnconnectedAreaId = unconnectedAreaId;
                            }
                        }
                    }
                }

                if (foundUnconnectedAreaId != -1)
                {
                    // Found at least one point to add area to the connected path so remove the rest of the connectors for that area
                    for (int x = 1; x < dungeonMap.GetLength(0) - 1; x++)
                    {
                        for (int y = 1; y < dungeonMap.GetLength(1) - 1; y++)
                        {
                            if (dungeonMap[x, y].PossibleDoorPoint && UnconnectedAreaIdAround(dungeonMap, connectedAreaIds, x, y) == foundUnconnectedAreaId)
                                dungeonMap[x, y].PossibleDoorPoint = false;
                        }
                    }
                    connectedAreaIds.Add(foundUnconnectedAreaId);
                    tries = 0;
                }
            }
        }

        private static void FindDoorPoints(MazeCell[,] dungeonMap)
        {
            for (int x = 1; x < dungeonMap.GetLength(0) - 1; x++)
            {
                for (int y = 1; y < dungeonMap.GetLength(1) - 1; y++)
                {
                    int areaToCheck = AreaIdAround(dungeonMap, x, y);
                    if (areaToCheck == -1)
                        continue;

                    dungeonMap[x, y].PossibleDoorPoint =
                        ((dungeonMap[x - 1, y].AreaId != 0 && dungeonMap[x - 1, y].AreaId != areaToCheck) ||
                        (dungeonMap[x + 1, y].AreaId != 0 && dungeonMap[x + 1, y].AreaId != areaToCheck) ||
                        (dungeonMap[x, y - 1].AreaId != 0 && dungeonMap[x, y - 1].AreaId != areaToCheck) ||
                        (dungeonMap[x, y + 1].AreaId != 0 && dungeonMap[x, y + 1].AreaId != areaToCheck));
                }
            }
        }

        private static int AreaIdAround(MazeCell[,] dungeonMap, int x, int y)
        {
            int areaId = dungeonMap[x - 1, y].AreaId;
            if (areaId != 0)
                return areaId;
            
            areaId = dungeonMap[x + 1, y].AreaId;
            if (areaId != 0)
                return areaId;
            
            areaId = dungeonMap[x, y - 1].AreaId;
            if (areaId != 0)
                return areaId;

            areaId = dungeonMap[x, y + 1].AreaId;
            if (areaId != 0)
                return areaId;

            return -1;
        }

        private static bool ConnectedAreaAround(MazeCell[,] dungeonMap, HashSet<int> connectedAreaIds, int x, int y)
        {
            return connectedAreaIds.Contains(dungeonMap[x - 1, y].AreaId) ||
                connectedAreaIds.Contains(dungeonMap[x + 1, y].AreaId) ||
                connectedAreaIds.Contains(dungeonMap[x, y - 1].AreaId) ||
                connectedAreaIds.Contains(dungeonMap[x, y + 1].AreaId);
        }

        private static int UnconnectedAreaIdAround(MazeCell[,] dungeonMap, HashSet<int> connectedAreaIds, int x, int y)
        {
            int areaId = dungeonMap[x - 1, y].AreaId;
            if (!connectedAreaIds.Contains(areaId))
                return areaId;

            areaId = dungeonMap[x + 1, y].AreaId;
            if (!connectedAreaIds.Contains(areaId))
                return areaId;

            areaId = dungeonMap[x, y - 1].AreaId;
            if (!connectedAreaIds.Contains(areaId))
                return areaId;

            areaId = dungeonMap[x, y + 1].AreaId;
            if (!connectedAreaIds.Contains(areaId))
                return areaId;

            return -1;
        }
        #endregion

        private static void CleanUpDeadEnds(MazeCell[,] dungeonMap, int maxAmountOfDeadEndToRemove)
        {
            bool removedDeadEnd = false;
            do
            {
                for (int x = 1; x < dungeonMap.GetLength(0) - 1; x++)
                {
                    for (int y = 1; y < dungeonMap.GetLength(1) - 1; y++)
                    {
                        if (dungeonMap[x, y].AreaId != 0)
                        {
                            int surroundingAreaCount = 0;
                            if (dungeonMap[x - 1, y].AreaId != 0)
                                surroundingAreaCount++;
                            if (dungeonMap[x + 1, y].AreaId != 0)
                                surroundingAreaCount++;
                            if (dungeonMap[x, y - 1].AreaId != 0)
                                surroundingAreaCount++;
                            if (dungeonMap[x, y + 1].AreaId != 0)
                                surroundingAreaCount++;

                            if (surroundingAreaCount == 1)
                            {
                                dungeonMap[x, y].AreaId = 0;
                                dungeonMap[x, y].LightId = 0;
                                removedDeadEnd = true;
                            }
                        }
                    }
                }
                maxAmountOfDeadEndToRemove--;
            }
            while (removedDeadEnd && maxAmountOfDeadEndToRemove > 0);
        }

        #region 3D world creation methods
        private static void FillWorld(IGameWorld gameWorld, IBlockList blockList, MazeCell[,] dungeonMap, int mazeStartAreaId)
        {
            BlockRandomizer dirts = new BlockRandomizer(blockList, "dirt", 6);
            BlockRandomizer grasses = new BlockRandomizer(blockList, "grass", 50);
            BlockRandomizer stoneBacks = new BlockRandomizer(blockList, "backStone", 6);
            ushort stone = blockList["ston0"];
            //ushort fire = blockList["fire"];
            //ushort lava = blockList["lava0"];
            //ushort fountain = blockList["fountain"];

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    int mazeLocX = x / 3;
                    int mazeLocY = y / 3;
                    int areaId = dungeonMap[mazeLocX, mazeLocY].AreaId;
                    if (areaId == 0)
                    {
                        gameWorld[x, y, 2] = stoneBacks.NextBlock();
                        gameWorld[x, y, 3] = stone;
                        gameWorld[x, y, 4] = stone;
                        gameWorld[x, y, 5] = stone;
                        gameWorld[x, y, 6] = stone;
                        gameWorld[x, y, 7] = stone;
                        //gameWorld[x, y, 8] = stone;

                        //if (mazeLocX == 0 || (dungeonMap[mazeLocX - 1, mazeLocY].AreaId > 0 && dungeonMap[mazeLocX - 1, mazeLocY].AreaId < mazeStartAreaId) ||
                        //    mazeLocX == dungeonMap.GetLength(0) - 1 || (dungeonMap[mazeLocX + 1, mazeLocY].AreaId > 0 && dungeonMap[mazeLocX + 1, mazeLocY].AreaId < mazeStartAreaId) ||
                        //    mazeLocY == 0 || (dungeonMap[mazeLocX, mazeLocY - 1].AreaId > 0 && dungeonMap[mazeLocX, mazeLocY - 1].AreaId < mazeStartAreaId) ||
                        //    mazeLocY == dungeonMap.GetLength(1) - 1 || (dungeonMap[mazeLocX, mazeLocY + 1].AreaId > 0 && dungeonMap[mazeLocX, mazeLocY + 1].AreaId < mazeStartAreaId))
                        //{
                        //    // Empty space next to a room
                        //    gameWorld[x, y, 8] = stone;
                        //    gameWorld[x, y, 9] = stone;
                        //    gameWorld[x, y, 10] = stone;
                        //}
                    }
                    else if (areaId == DoorAreaId)
                    {
                        gameWorld[x, y, 2] = stoneBacks.NextBlock();
                    }
                    else if (areaId >= mazeStartAreaId)
                    {
                        // Maze
                        gameWorld[x, y, 2] = dirts.NextBlock();
                        gameWorld[x, y, 3] = grasses.NextBlock();
                    }
                    else
                    {
                        // Room
                        gameWorld[x, y, 2] = stoneBacks.NextBlock();
                    }

                    int lightId = dungeonMap[mazeLocX, mazeLocY].LightId;
                    if (x % 3 == 1 && y % 3 == 1 && lightId != 0)
                    {
                        // Light
                        if (lightId == RoomLightId)
                            gameWorld[x, y, 7] = blockList["roomLight"];
                        else
                        {
                            // For maze lights, move the light near to the maze walls so it's not in the middle of the path
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

                            int height = 4;
                            if (lightLocX == x && lightLocY == y)
                                height = 7;
                            gameWorld[lightLocX, lightLocY, height] = blockList["light" + (lightId - 1)];
                        }
                    }
                }
            }
        }

        private static void SmoothWorld(IGameWorld gameWorld, IBlockList blockList)
        {
            HashSet<int> blocksToConsiderEmpty = new HashSet<int>();
            blocksToConsiderEmpty.Add(0);
            for (int i = 0; i < 50; i++)
                blocksToConsiderEmpty.Add(blockList["grass" + i]);
            for (int i = 0; i < 6; i++)
                blocksToConsiderEmpty.Add(blockList["light" + i]);

            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = 0; x < gameWorld.BlockSize.X; x++)
                {
                    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    {
                        ushort blockIndex = gameWorld[x, y, z];
                        if (blockIndex == 0)
                            continue;

                        Block block = blockList[blockIndex];
                        string blockNameKey = GetBlockSet(block);

                        if (blockNameKey != "ston" && blockNameKey != "sand" && blockNameKey != "shru" && blockNameKey != "lava")
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

        private static string GetBlockSet(Block block)
        {
            return block.Name.Substring(0, 4);
        }
        #endregion

        #region Debugging methods
        private static void PrintDungeon(MazeCell[,] dungeonMap)
        {
            for (int y = dungeonMap.GetLength(1) - 1; y >= 0; y--)
            {
                string d = "";
                for (int x = 0; x < dungeonMap.GetLength(0); x++)
                {
                    if (dungeonMap[x, y].AreaId > 0)
                        d += dungeonMap[x, y].AreaId.ToString("D2");
                    else if (dungeonMap[x, y].PossibleDoorPoint)
                        d += "**";
                    else
                        d += "  ";
                }
                Console.WriteLine(d);
            }
        }
        #endregion

        #region MazeCellLocation structure
        private struct MazeCellLocation
        {
            public static readonly MazeCellLocation None = new MazeCellLocation(-1, -1);

            public readonly int X;
            public readonly int Y;

            public MazeCellLocation(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override bool Equals(object obj)
            {
                MazeCellLocation other = (MazeCellLocation)obj;
                return other.X == X && other.Y == Y;
            }

            public override int GetHashCode()
            {
                return X ^ Y;
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

        #region MazeCell structure
        private struct MazeCell
        {
            public int AreaId;
            public int LightId;
            public bool PossibleDoorPoint;
        }
        #endregion
    }
}
