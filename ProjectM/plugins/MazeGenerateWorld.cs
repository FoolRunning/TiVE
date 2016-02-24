using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Plugins
{
    [UsedImplicitly]
    public class MazeGenerateWorld : IWorldGenerator
    {
        private static readonly Random random = new Random();

        private const int RoomLightId = 100;
        private const int DoorAreaId = 2100;

        private const int RoomTreasureChestId = 1;
        private const int RoomColumnId = 2;
        private const int RoomFountainId = 3;
        private const int RoomFireId = 4;

        private enum Direction { Up, Down, Left, Right, None }

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;

        public IGameWorld CreateGameWorld(string gameWorldName)
        {
            if (gameWorldName != "Maze")
                return null;

            IGameWorld gameWorld = Factory.NewGameWorld(513, 513, 12); // x-axis and y-axis must be divisible by 3
            gameWorld.LightingModelType = LightingModelType.Realistic;

            MazeCell[,] dungeonMap = new MazeCell[gameWorld.BlockSize.X / 3, gameWorld.BlockSize.Y / 3];
            List<Vector3i> rooms = CreateRandomRooms(110, 3, 11, 3, 11).ToList();
            int mazeStartAreaId = PlaceRooms(rooms, dungeonMap, 25);
            int lastUsedId = FillBlankWithMaze(dungeonMap, mazeStartAreaId);
            CreateDoors(dungeonMap, lastUsedId);
            CleanUpDeadEnds(dungeonMap, 100);

            //Console.WriteLine("\n\nFinished Maze:");
            //PrintDungeon(dungeonMap);

            FillWorld(gameWorld, dungeonMap, mazeStartAreaId);
            SmoothWorld(gameWorld);

            return gameWorld;
        }

        #region Room placement methods
        private static IEnumerable<Vector3i> CreateRandomRooms(int numOfRooms, int minWidth, int maxWidth, int minHeight, int maxHeight)
        {
            for (int i = 0; i < numOfRooms; i++)
            {
                int roomWidth = minWidth + random.Next((maxWidth - minWidth + 2) / 2) * 2;
                int roomHeight = minHeight + random.Next((maxHeight - minHeight + 2) / 2) * 2;
                //Console.WriteLine("Created room sized: {0}, {1}, {2}", roomWidth, roomHeight, roomDepth);
                yield return new Vector3i(roomWidth, roomHeight, 0);
            }
        }

        private static int PlaceRooms(List<Vector3i> rooms, MazeCell[,] dungeonMap, int maxTries)
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
            return areaId;
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

            bool isBigRoom = room.X >= 9 && room.Y >= 9;
            int testValue = random.Next(10);
            bool createFountains = testValue < 4;
            bool createFire = !createFountains && testValue < 7;
            for (int x = 0; x < room.X; x++)
            {
                for (int y = 0; y < room.Y; y++)
                {
                    dungeonMap[x + roomX, y + roomY].AreaId = areaId;

                    if ((x == 1 || x == room.X - 2) && (y == 1 || y == room.Y - 2))
                        dungeonMap[x + roomX, y + roomY].LightId = RoomLightId;
                    else if (isBigRoom && (x % 9 == 4 || room.X - x - 1 % 9 == 4) && ((y % 9 == 4 || room.Y - y - 1 % 9 == 4)))
                    {
                        if (createFountains)
                            dungeonMap[x + roomX, y + roomY].ItemId = RoomFountainId;
                        else if (createFire)
                            dungeonMap[x + roomX, y + roomY].ItemId = RoomFireId;
                        else
                            dungeonMap[x + roomX, y + roomY].ItemId = RoomColumnId;
                    }
                }
            }

            if (room.X == 3 && room.Y == 3)
                dungeonMap[roomX + 1, roomY + 1].ItemId = RoomTreasureChestId; // Very tiny room are considered treasure rooms
            return true;
        }
        #endregion

        #region Maze generation methods
        private static int FillBlankWithMaze(MazeCell[,] dungeonMap, int mazeAreaId)
        {
            List<MazeCellLocation> cellsInMaze = new List<MazeCellLocation>();

            MazeCellLocation cell = FindBlankSpace(dungeonMap);
            cellsInMaze.Add(cell);
            dungeonMap[cell.X, cell.Y].AreaId = mazeAreaId;

            Direction lastDir = Direction.None;
            bool searchingForNewCell = false;
            //int lightId = 0;
            while (cellsInMaze.Count > 0)
            {
                Direction dir = cell != MazeCellLocation.None ? ChooseRandomDirection(cell, dungeonMap) : Direction.None;
                if (dir == Direction.None)
                {
                    // Couldn't continue with current road so find a good place for another one
                    if (!searchingForNewCell && cell != MazeCellLocation.None)
                        dungeonMap[cell.X, cell.Y].LightId = random.Next(6);

                    //lightId = (lightId + 1) % 6;
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
                        cell = FindBlankSpace(dungeonMap);
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
                        dungeonMap[cell.X, cell.Y].LightId = random.Next(6);

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

            return mazeAreaId;
        }

        private static MazeCellLocation FindBlankSpace(MazeCell[,] dungeonMap)
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

        private static Direction ChooseRandomDirection(MazeCellLocation cell, MazeCell[,] dungeonMap)
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
        private static void CreateDoors(MazeCell[,] dungeonMap, int lastUsedId)
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
                        if (dungeonMap[x, y].PossibleDoorPoint && random.Next(100) < 7 && ConnectedAreaAround(dungeonMap, connectedAreaIds, x, y))
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
                if (tries > 30 && connectedAreaIds.Count >= lastUsedId)
                    break;
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
        private static void FillWorld(IGameWorld gameWorld, MazeCell[,] dungeonMap, int mazeStartAreaId)
        {
            BlockRandomizer grasses = new BlockRandomizer("grass", 50);
            Block dirt = Factory.Get<Block>("dirt");
            Block stoneBack = Factory.Get<Block>("backStone");
            Block stone = Factory.Get<Block>("ston0");
            Block wood = Factory.Get<Block>("wood0");
            Block fountain = Factory.Get<Block>("fountain");
            Block smallLight = Factory.Get<Block>("smallLight");
            //Block smallLightHover = blockList["smallLightHover"];
            Block fire = Factory.Get<Block>("fire");
            //Block lava = blockList["lava0"];

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    int mazeLocX = x / 3;
                    int mazeLocY = y / 3;
                    int areaId = dungeonMap[mazeLocX, mazeLocY].AreaId;
                    int itemId = dungeonMap[mazeLocX, mazeLocY].ItemId;
                    if (areaId == 0)
                    {
                        gameWorld[x, y, 2] = stone;
                        gameWorld[x, y, 3] = stone;
                        gameWorld[x, y, 4] = stone;
                        gameWorld[x, y, 5] = stone;
                        gameWorld[x, y, 6] = stone;
                        gameWorld[x, y, 7] = stone;
                        //gameWorld[x, y, 8] = stone;
                        //gameWorld[x, y, 9] = stone;
                        //gameWorld[x, y, 10] = stone;

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
                        gameWorld[x, y, 2] = stoneBack;
                    else if (areaId >= mazeStartAreaId)
                    {
                        // Maze
                        gameWorld[x, y, 2] = dirt;
                        gameWorld[x, y, 3] = grasses.NextBlock();
                    }
                    else
                    {
                        // Room
                        gameWorld[x, y, 2] = stoneBack;
                    }

                    if (itemId == RoomTreasureChestId)
                        gameWorld[x, y, 3] = wood;
                    else if (itemId == RoomFountainId)
                    {
                        gameWorld[x, y, 3] = wood;
                        if (x % 3 == 1 && y % 3 == 1)
                            gameWorld[x, y, 4] = fountain;
                        if ((x % 3 == 0 || x % 3 == 2) && (y % 3 == 0 || y % 3 == 2))
                            gameWorld[x, y, 4] = smallLight;
                    }
                    else if (itemId == RoomFireId)
                    {
                        gameWorld[x, y, 3] = wood;
                        if (x % 3 == 1 && y % 3 == 1)
                            gameWorld[x, y, 4] = fire;
                    }
                    else if (itemId == RoomColumnId && x % 3 == 1 && y % 3 == 1)
                    {
                        gameWorld[x, y, 3] = wood;
                        gameWorld[x, y, 4] = wood;
                        gameWorld[x, y, 5] = wood;
                        gameWorld[x, y, 6] = wood;
                        gameWorld[x, y, 7] = wood;
                        //gameWorld[x, y, 8] = wood;
                        gameWorld[x - 1, y, 7] = wood;
                        gameWorld[x + 1, y, 7] = wood;
                        gameWorld[x, y - 1, 7] = wood;
                        gameWorld[x, y + 1, 7] = wood;
                    }

                    int lightId = dungeonMap[mazeLocX, mazeLocY].LightId;
                    if (x % 3 == 1 && y % 3 == 1 && lightId != 0)
                    {
                        // Light
                        if (lightId == RoomLightId)
                            gameWorld[x, y, 7] = Factory.Get<Block>("roomLight");
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
                            gameWorld[lightLocX, lightLocY, height] = Factory.Get<Block>("light" + (lightId - 1));
                        }
                    }
                }
            }
        }

        private static void SmoothWorld(IGameWorld gameWorld)
        {
            HashSet<Block> blocksToConsiderEmpty = new HashSet<Block>();
            blocksToConsiderEmpty.Add(Block.Empty);
            blocksToConsiderEmpty.Add(Factory.Get<Block>("smallLightHover"));
            for (int i = 0; i < 50; i++)
                blocksToConsiderEmpty.Add(Factory.Get<Block>("grass" + i));
            for (int i = 0; i < 6; i++)
                blocksToConsiderEmpty.Add(Factory.Get<Block>("light" + i));

            HashSet<Block> blocksToSmooth = new HashSet<Block>();
            for (int i = 0; i < 64; i++)
            {
                blocksToSmooth.Add(Factory.Get<Block>("ston" + i));
                blocksToSmooth.Add(Factory.Get<Block>("wood" + i));
            }

            for (int z = 0; z < gameWorld.BlockSize.Z; z++)
            {
                for (int x = 0; x < gameWorld.BlockSize.X; x++)
                {
                    for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                    {
                        Block block = gameWorld[x, y, z];
                        if (block == Block.Empty || !blocksToSmooth.Contains(block))
                            continue;

                        int sides = 0;

                        if (z == gameWorld.BlockSize.Z - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z + 1]))
                            sides |= Front;

                        if (z == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y, z - 1]))
                            sides |= Back;

                        if (x == 0 || blocksToConsiderEmpty.Contains(gameWorld[x - 1, y, z]))
                            sides |= Left;

                        if (x == gameWorld.BlockSize.X - 1 || blocksToConsiderEmpty.Contains(gameWorld[x + 1, y, z]))
                            sides |= Right;

                        if (y == gameWorld.BlockSize.Y - 1 || blocksToConsiderEmpty.Contains(gameWorld[x, y + 1, z]))
                            sides |= Top;

                        if (y == 0 || blocksToConsiderEmpty.Contains(gameWorld[x, y - 1, z]))
                            sides |= Bottom;

                        string blockNamePart = block.Name.Substring(0, 4);
                        gameWorld[x, y, z] = Factory.Get<Block>(blockNamePart + sides);
                    }
                }
            }
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
            public int ItemId;
            public bool PossibleDoorPoint;
        }
        #endregion
    }
}
