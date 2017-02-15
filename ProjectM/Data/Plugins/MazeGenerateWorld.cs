using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Data.Plugins
{
    [UsedImplicitly]
    public class MazeGenerateWorld : IWorldGenerator
    {
        private static readonly RandomGenerator random = new RandomGenerator();

        private const int AreaIdDoor = 2100;

        private const int LightIdRoomLight = 100;
        private const int LightIdSmallLight = 101;

        private const int ItemIdPlayer = 1;
        private const int ItemIdTreasureChest = 2;
        private const int ItemIdColumn = 3;
        private const int ItemIdFountain = 4;
        private const int ItemIdFire = 5;
        private const int ItemIdLava = 6;
        private const int ItemIdGrass = 7;
        private const int ItemIdNoCeiling = 8;

        private enum Direction { Up, Down, Left, Right, None }

        public IGameWorld CreateGameWorld(string gameWorldName)
        {
            if (gameWorldName != "Maze")
                return null;

            //IGameWorld gameWorld = Factory.NewGameWorld(1110, 1110, 120); // x-axis and y-axis must be divisible by 3
            IGameWorld gameWorld = Factory.NewGameWorld(300, 300, 16); // x-axis and y-axis must be divisible by 3
            //IGameWorld gameWorld = Factory.NewGameWorld(111, 111, 12); // x-axis and y-axis must be divisible by 3
            gameWorld.LightingModelType = LightingModelType.Fantasy3;

            MazeCell[,] dungeonMap = new MazeCell[gameWorld.BlockSize.X / 3, gameWorld.BlockSize.Y / 3];
            //List<Vector3i> rooms = CreateRandomRooms(random.Next(50) + 160, 3, 15, 3, 15).ToList();
            List<Vector3i> rooms = CreateRandomRooms(random.Next(50) + 20, 3, 15, 3, 15).ToList();
            //List<Vector3i> rooms = CreateRandomRooms(10, 3, 11, 3, 11).ToList();
            int mazeStartAreaId = PlaceRooms(rooms, dungeonMap, random.Next(60) + 10);
            int lastUsedId = FillBlankWithMaze(dungeonMap, mazeStartAreaId);

            // Find possible door points at places where rooms and maze (or another room) meet
            FindDoorPoints(dungeonMap);

            //Console.WriteLine("\n\nMaze before placing doors:");
            //PrintDungeon(dungeonMap);

            CreateDoors(dungeonMap, mazeStartAreaId, lastUsedId);

            //Console.WriteLine("\n\nMaze before cleaning dead ends:");
            //PrintDungeon(dungeonMap);

            CleanUpDeadEnds(dungeonMap, random.Next(60) + 20);
            
            //Console.WriteLine("\n\nMaze before removing annoying turns:");
            //PrintDungeon(dungeonMap);

            RemoveAnnoyingMazeTurns(dungeonMap, mazeStartAreaId, lastUsedId);

            //Console.WriteLine("\n\nFinished Maze:");
            //PrintDungeon(dungeonMap);

            FillWorld(gameWorld, dungeonMap, mazeStartAreaId);
            CommonUtils.SmoothGameWorldForMazeBlocks(gameWorld, false);

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

        private enum BigRoomType
        {
            None,
            Fountain,
            Fire,
            Columns,
            Lava,
            Grass
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

            BigRoomType roomType = BigRoomType.None;
            if (room.X >= 9 && room.Y >= 9)
            {
                int testValue = random.Next(100);
                if (testValue < 20)
                    roomType = BigRoomType.Fountain;
                else if (testValue < 40)
                    roomType = BigRoomType.Fire;
                else if (testValue < 60)
                    roomType = BigRoomType.Columns;
                else if (testValue < 80)
                    roomType = BigRoomType.Lava;
                else
                    roomType = BigRoomType.Grass;
            }

            for (int x = 0; x < room.X; x++)
            {
                for (int y = 0; y < room.Y; y++)
                {
                    dungeonMap[x + roomX, y + roomY].AreaId = areaId;

                    if (roomType == BigRoomType.None && (x == 1 || x == room.X - 2) && (y == 1 || y == room.Y - 2))
                        dungeonMap[x + roomX, y + roomY].LightId = LightIdRoomLight;
                    else if (roomType == BigRoomType.Lava)
                    {
                        if ((x % 3 == 1 && (y == 1 || y == room.Y - 2)) ||
                            (y % 3 == 1 && (x == 1 || x == room.X - 2)))
                        {
                            dungeonMap[x + roomX, y + roomY].LightId = LightIdSmallLight;
                        }

                        if (x > 2 && x < room.X - 3 && y > 2 && y < room.Y - 3)
                            dungeonMap[x + roomX, y + roomY].ItemId = ItemIdLava;
                    }
                    else if (roomType == BigRoomType.Grass)
                    {
                        if (x > 0 && x < room.X - 1 && y > 0 && y < room.Y - 1)
                            dungeonMap[x + roomX, y + roomY].ItemId = ItemIdGrass;
                        else
                            dungeonMap[x + roomX, y + roomY].ItemId = ItemIdNoCeiling;
                    }
                    else if (roomType != BigRoomType.None && (x % 9 == 4 || room.X - x - 1 % 9 == 4) && ((y % 9 == 4 || room.Y - y - 1 % 9 == 4)))
                    {
                        if (roomType == BigRoomType.Fountain)
                            dungeonMap[x + roomX, y + roomY].ItemId = ItemIdFountain;
                        else if (roomType == BigRoomType.Fire)
                            dungeonMap[x + roomX, y + roomY].ItemId = ItemIdFire;
                        else if (roomType == BigRoomType.Columns)
                            dungeonMap[x + roomX, y + roomY].ItemId = ItemIdColumn;
                    }
                }
            }

            if (areaId == 1)
                dungeonMap[roomX + 1, roomY + 1].ItemId = ItemIdPlayer; // First room is always the starting area for the player
            else if (room.X == 3 && room.Y == 3)
                dungeonMap[roomX + 1, roomY + 1].ItemId = ItemIdTreasureChest; // Very tiny room are considered treasure rooms
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
                        dungeonMap[cell.X, cell.Y].LightId = random.Next(7) + 1;

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
                        dungeonMap[cell.X, cell.Y].LightId = random.Next(7) + 1;

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

        #region Find doors methods
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
        #endregion

        #region Door placement methods
        private static void CreateDoors(MazeCell[,] dungeonMap, int mazeStartAreaId, int lastUsedAreaId)
        {
            HashSet<int> connectedAreaIds = new HashSet<int>();
            connectedAreaIds.Add(random.Next(mazeStartAreaId - 1) + 1); // Start with a random room

            for ( ; ; )
            {
                // Choose a random room to connect
                int areaIdToConnect;
                do
                {
                    areaIdToConnect = random.Next(lastUsedAreaId) + 1;
                }
                while (connectedAreaIds.Contains(areaIdToConnect));

                // Find all possible door locations that connect to the wanted room and to the connected area. This may result in no possibilities if
                // there are no door possibilities that connect with the connected area.
                List<MazeCellLocation> possibleDoors = FindDoorsThatConnectWithConnectedArea(dungeonMap, connectedAreaIds, areaIdToConnect);
                if (possibleDoors.Count == 0)
                    continue;

                connectedAreaIds.Add(areaIdToConnect);

                // Choose a random door location to be the "main door"
                int mainDoorIndex = random.Next(possibleDoors.Count);
                MazeCellLocation chosenDoor = possibleDoors[mainDoorIndex];
                dungeonMap[chosenDoor.X, chosenDoor.Y].AreaId = AreaIdDoor;
                dungeonMap[chosenDoor.X, chosenDoor.Y].PossibleDoorPoint = false;
                possibleDoors.RemoveAt(mainDoorIndex);
                connectedAreaIds.Add(areaIdToConnect);

                // Remove all other possible door connection points that connect with the wanted room to the connected area. 
                // Also allow for a small chance that a new door will actually be created instead of it being removed.
                while (possibleDoors.Count > 0)
                {
                    MazeCellLocation otherPossibleDoor = possibleDoors[possibleDoors.Count - 1];
                    if (random.Next(100) < 7) // 7% chance to create another door
                        dungeonMap[otherPossibleDoor.X, otherPossibleDoor.Y].AreaId = AreaIdDoor;

                    dungeonMap[otherPossibleDoor.X, otherPossibleDoor.Y].PossibleDoorPoint = false;
                    possibleDoors.RemoveAt(possibleDoors.Count - 1);
                }

                if (connectedAreaIds.Count >= lastUsedAreaId)
                    break;
            }
        }

        private static List<MazeCellLocation> FindDoorsThatConnectWithConnectedArea(MazeCell[,] dungeonMap, HashSet<int> connectedAreaIds, int areaId)
        {
            List<MazeCellLocation> possibleDoors = new List<MazeCellLocation>(50);
            for (int x = 1; x < dungeonMap.GetLength(0) - 1; x++)
            {
                for (int y = 1; y < dungeonMap.GetLength(1) - 1; y++)
                {
                    if (dungeonMap[x, y].PossibleDoorPoint && HasAreaAround(dungeonMap, x, y, areaId) && HasConnectedAreaAround(dungeonMap, connectedAreaIds, x, y))
                        possibleDoors.Add(new MazeCellLocation(x, y));
                }
            }
            return possibleDoors;
        }

        private static bool HasAreaAround(MazeCell[,] dungeonMap, int x, int y, int areaId)
        {
            return dungeonMap[x - 1, y].AreaId == areaId || dungeonMap[x + 1, y].AreaId == areaId || 
                dungeonMap[x, y - 1].AreaId == areaId || dungeonMap[x, y + 1].AreaId == areaId;
        }

        private static bool HasConnectedAreaAround(MazeCell[,] dungeonMap, HashSet<int> connectedAreaIds, int x, int y)
        {
            return connectedAreaIds.Contains(dungeonMap[x - 1, y].AreaId) ||
                connectedAreaIds.Contains(dungeonMap[x + 1, y].AreaId) ||
                connectedAreaIds.Contains(dungeonMap[x, y - 1].AreaId) ||
                connectedAreaIds.Contains(dungeonMap[x, y + 1].AreaId);
        }
        #endregion

        #region Maze cleanup methods
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

                            if (surroundingAreaCount <= 1)
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

            // Add a light to any remaining dead-ends
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

                        if (surroundingAreaCount <= 1)
                            dungeonMap[x, y].LightId = random.Next(7) + 1;
                    }
                }
            }
        }

        private static void RemoveAnnoyingMazeTurns(MazeCell[,] dungeonMap, int mazeStartAreaId, int lastUsedAreaId)
        {
            HashSet<int> mazeAreaIdsAlreadyHandled = new HashSet<int>();
            for (int x = 1; x < dungeonMap.GetLength(0) - 1; x++)
            {
                for (int y = 1; y < dungeonMap.GetLength(1) - 1; y++)
                {
                    int areaId = dungeonMap[x, y].AreaId;
                    if (areaId >= mazeStartAreaId && areaId <= lastUsedAreaId && !mazeAreaIdsAlreadyHandled.Contains(areaId))
                    {
                        mazeAreaIdsAlreadyHandled.Add(areaId);
                        MazeCellLocation mazeAreaStart = FindMazeAreaStart(dungeonMap, areaId);
                        WalkMazeAreaRemovingRedundantTurns(dungeonMap, mazeAreaStart, areaId, Direction.None);
                    }
                }
            }
        }

        private static MazeCellLocation FindMazeAreaStart(MazeCell[,] dungeonMap, int mazeAreaId)
        {
            for (int x = 1; x < dungeonMap.GetLength(0) - 1; x++)
            {
                for (int y = 1; y < dungeonMap.GetLength(1) - 1; y++)
                {
                    int areaId = dungeonMap[x, y].AreaId;
                    if (areaId == mazeAreaId)
                    {
                        if (SurroundingAreaCount(dungeonMap, x, y, mazeAreaId) <= 1)
                            return new MazeCellLocation(x, y);
                    }
                }
            }

            throw new InvalidOperationException("We somehow failed to find the start of the maze area");
        }

        private static void WalkMazeAreaRemovingRedundantTurns(MazeCell[,] dungeonMap, MazeCellLocation mazeAreaStart, int mazeAreaId, Direction walkDir)
        {
            //Console.WriteLine("Maze area start for {0:X2} is ({1}, {2}), direction {3}", mazeAreaId, mazeAreaStart.X, mazeAreaStart.Y, walkDir);
            List<MazeCellLocation> previousMazeCells = new List<MazeCellLocation>(50);
            int x = mazeAreaStart.X;
            int y = mazeAreaStart.Y;
            for (; ; )
            {
                int surroundingMazeAreaCount = SurroundingAreaCount(dungeonMap, x, y, mazeAreaId);
                if (surroundingMazeAreaCount > 2)
                {
                    //Console.WriteLine("Found 'T' or '+' at ({0}, {1}), direction {2}", x, y, walkDir);

                    // Found a 'T' or '+' so restart walking in each direction and stop this walking
                    if (walkDir != Direction.Right && dungeonMap[x - 1, y].AreaId == mazeAreaId)
                        WalkMazeAreaRemovingRedundantTurns(dungeonMap, new MazeCellLocation(x - 1, y), mazeAreaId, Direction.Left);
                    if (walkDir != Direction.Left && dungeonMap[x + 1, y].AreaId == mazeAreaId)
                        WalkMazeAreaRemovingRedundantTurns(dungeonMap, new MazeCellLocation(x + 1, y), mazeAreaId, Direction.Right);
                    if (walkDir != Direction.Down && dungeonMap[x, y - 1].AreaId == mazeAreaId)
                        WalkMazeAreaRemovingRedundantTurns(dungeonMap, new MazeCellLocation(x, y - 1), mazeAreaId, Direction.Up);
                    if (walkDir != Direction.Up && dungeonMap[x, y + 1].AreaId == mazeAreaId)
                        WalkMazeAreaRemovingRedundantTurns(dungeonMap, new MazeCellLocation(x, y + 1), mazeAreaId, Direction.Down);
                    break;
                }

                previousMazeCells.Add(new MazeCellLocation(x, y));

                int prevX = x;
                int prevY = y;
                if (walkDir != Direction.Right && dungeonMap[x - 1, y].AreaId == mazeAreaId)
                {
                    x--;
                    walkDir = Direction.Left;
                }
                else if (walkDir != Direction.Left && dungeonMap[x + 1, y].AreaId == mazeAreaId)
                {
                    x++;
                    walkDir = Direction.Right;
                }
                else if (walkDir != Direction.Down && dungeonMap[x, y - 1].AreaId == mazeAreaId)
                {
                    y--;
                    walkDir = Direction.Up;
                }
                else if (walkDir != Direction.Up && dungeonMap[x, y + 1].AreaId == mazeAreaId)
                {
                    y++;
                    walkDir = Direction.Down;
                }

                //Console.WriteLine("Moved {0} to ({1}, {2})", walkDir, x, y);

                if (x == prevX && y == prevY)
                {
                    //Console.WriteLine("Maze area end for {0:X2} is ({1}, {2}), direction {3}", mazeAreaId, x, y, walkDir);
                    break; // Found a dead-end so we're finished
                }

                int shortCutCellIndex = -1;
                MazeCellLocation shortcutCell = new MazeCellLocation(-1, -1);
                for (int i = 0; i < previousMazeCells.Count - 2; i++)
                {
                    MazeCellLocation cell = previousMazeCells[i];
                    if (cell.X == x - 2 && cell.Y == y)
                    {
                        shortCutCellIndex = i;
                        shortcutCell = new MazeCellLocation(x - 1, y);
                        walkDir = Direction.Right;
                        break;
                    }
                    if (cell.X == x + 2 && cell.Y == y)
                    {
                        shortCutCellIndex = i;
                        shortcutCell = new MazeCellLocation(x + 1, y);
                        walkDir = Direction.Left;
                        break;
                    }
                    if (cell.X == x && cell.Y == y - 2)
                    {
                        shortCutCellIndex = i;
                        shortcutCell = new MazeCellLocation(x, y - 1);
                        walkDir = Direction.Down;
                        break;
                    }
                    if (cell.X == x && cell.Y == y + 2)
                    {
                        shortCutCellIndex = i;
                        shortcutCell = new MazeCellLocation(x, y + 1);
                        walkDir = Direction.Up;
                        break;
                    }
                }

                if (shortCutCellIndex >= 0)
                {
                    for (int i = previousMazeCells.Count - 1; i > shortCutCellIndex; i--)
                    {
                        MazeCellLocation deletedCell = previousMazeCells[i];
                        dungeonMap[deletedCell.X, deletedCell.Y].AreaId = 0;
                        dungeonMap[deletedCell.X, deletedCell.Y].LightId = 0;
                        previousMazeCells.RemoveAt(i);
                    }

                    dungeonMap[shortcutCell.X, shortcutCell.Y].AreaId = mazeAreaId;
                    previousMazeCells.Add(shortcutCell);
                }

                if ((dungeonMap[x - 1, y].AreaId != 0 && dungeonMap[x - 1, y].AreaId != mazeAreaId) ||
                    (dungeonMap[x + 1, y].AreaId != 0 && dungeonMap[x + 1, y].AreaId != mazeAreaId) ||
                    (dungeonMap[x, y - 1].AreaId != 0 && dungeonMap[x, y - 1].AreaId != mazeAreaId) ||
                    (dungeonMap[x, y + 1].AreaId != 0 && dungeonMap[x, y + 1].AreaId != mazeAreaId))
                {
                    // Found a connector. Make sure we don't remove any part of the maze before that connector.
                    previousMazeCells.Clear();
                }
            }
        }

        private static int SurroundingAreaCount(MazeCell[,] dungeonMap, int x, int y, int areaId)
        {
            int surroundingAreaCount = 0;
            if (dungeonMap[x - 1, y].AreaId == areaId)
                surroundingAreaCount++;
            if (dungeonMap[x + 1, y].AreaId == areaId)
                surroundingAreaCount++;
            if (dungeonMap[x, y - 1].AreaId == areaId)
                surroundingAreaCount++;
            if (dungeonMap[x, y + 1].AreaId == areaId)
                surroundingAreaCount++;
            return surroundingAreaCount;
        }
        #endregion

        #region 3D world creation methods
        private static void FillWorld(IGameWorld gameWorld, MazeCell[,] dungeonMap, int mazeStartAreaId)
        {
            BlockRandomizer grasses = new BlockRandomizer("grass", CommonUtils.grassBlockDuplicates);
            BlockRandomizer unlitGrasses = new BlockRandomizer("loadingGrass", CommonUtils.grassBlockDuplicates);
            Block player = Factory.Get<Block>("player");
            Block dirt = Factory.Get<Block>("dirt");
            //Block stone = Factory.Get<Block>("stoneBrick0_0");
            Block stoneBack = Factory.Get<Block>("back0_0");
            Block wood = Factory.Get<Block>("wood0");
            Block leaves = Factory.Get<Block>("leaves0_0");
            Block fountain = Factory.Get<Block>("fountain");
            Block smallLight = Factory.Get<Block>("smallLight");
            Block roomLight = Factory.Get<Block>("roomLight");
            Block redLight = Factory.Get<Block>("redLight");
            Block treeLight = Factory.Get<Block>("treeLight");
            Block hoverLightBlue = Factory.Get<Block>("hoverLightBlue");
            Block fountainLight = Factory.Get<Block>("fountainLight");
            Block dummy = Factory.Get<Block>("dummy");
            //Block smallLightHover = blockList["smallLightHover"];
            Block fire = Factory.Get<Block>("fire");
            Block lava = Factory.Get<Block>("lava");
            Block lavaUnlit = Factory.Get<Block>("lavaUnlit");
            Block[] mazeLights = new Block[7];
            for (int i = 0; i < mazeLights.Length; i++)
                mazeLights[i] = Factory.Get<Block>("light" + i);

            HashSet<int> roomIds = new HashSet<int>();
            for (int i = 1; i < mazeStartAreaId; i++)
                roomIds.Add(i);

            for (int x = 0; x < gameWorld.BlockSize.X; x++)
            {
                for (int y = 0; y < gameWorld.BlockSize.Y; y++)
                {
                    int mazeLocX = x / 3;
                    int mazeLocY = y / 3;
                    int areaId = dungeonMap[mazeLocX, mazeLocY].AreaId;
                    int itemId = dungeonMap[mazeLocX, mazeLocY].ItemId;
                    gameWorld[x, y, 0] = dirt;
                    gameWorld[x, y, 1] = stoneBack;
                    if (areaId == 0)
                    {
                        // Stone wall (i.e. not maze or room)
                        gameWorld[x, y, 2] = stoneBack;
                        gameWorld[x, y, 3] = stoneBack;
                        gameWorld[x, y, 4] = stoneBack;
                        gameWorld[x, y, 5] = stoneBack;
                        gameWorld[x, y, 6] = stoneBack;
                        gameWorld[x, y, 7] = stoneBack;

                        if (mazeLocX == 0 || mazeLocX == dungeonMap.GetLength(0) - 1 || mazeLocY == 0 || mazeLocY == dungeonMap.GetLength(1) - 1)
                        {
                            // Map bounds
                            gameWorld[x, y, 3] = stoneBack;
                            gameWorld[x, y, 4] = stoneBack;
                            gameWorld[x, y, 5] = stoneBack;
                            gameWorld[x, y, 6] = stoneBack;
                            gameWorld[x, y, 7] = stoneBack;
                            gameWorld[x, y, 8] = stoneBack;
                            gameWorld[x, y, 9] = stoneBack;
                            gameWorld[x, y, 10] = stoneBack;
                            gameWorld[x, y, 11] = stoneBack;
                            if (x % 3 == 1 && y % 3 == 1 && (mazeLocX % 3 == 1 || mazeLocY % 3 == 1))
                            {
                                if (mazeLocX == 0)
                                    gameWorld[x + 2, y, 10] = redLight;
                                else if (mazeLocX == dungeonMap.GetLength(0) - 1)
                                    gameWorld[x - 2, y, 10] = redLight;
                                else if (mazeLocY == 0)
                                    gameWorld[x, y + 2, 10] = redLight;
                                else if (mazeLocY == dungeonMap.GetLength(1) - 1)
                                    gameWorld[x, y - 2, 10] = redLight;
                            }
                        }
                        else if (roomIds.Contains(dungeonMap[mazeLocX - 1, mazeLocY].AreaId) || roomIds.Contains(dungeonMap[mazeLocX + 1, mazeLocY].AreaId) ||
                            roomIds.Contains(dungeonMap[mazeLocX, mazeLocY - 1].AreaId) || roomIds.Contains(dungeonMap[mazeLocX, mazeLocY + 1].AreaId) ||
                            roomIds.Contains(dungeonMap[mazeLocX - 1, mazeLocY - 1].AreaId) || roomIds.Contains(dungeonMap[mazeLocX + 1, mazeLocY - 1].AreaId) ||
                            roomIds.Contains(dungeonMap[mazeLocX - 1, mazeLocY + 1].AreaId) || roomIds.Contains(dungeonMap[mazeLocX + 1, mazeLocY + 1].AreaId))
                        {
                            // Stone wall next to a room
                            gameWorld[x, y, 8] = stoneBack;
                            gameWorld[x, y, 9] = stoneBack;
                            gameWorld[x, y, 10] = stoneBack;
                            if (BlockNextToRoom(dungeonMap, roomIds, gameWorld, x, y))
                                gameWorld[x, y, 11] = wood;
                        }
                    }
                    else if (areaId == AreaIdDoor)
                    {
                        gameWorld[x, y, 2] = stoneBack;
                        gameWorld[x, y, 8] = stoneBack;
                        gameWorld[x, y, 9] = stoneBack;
                        gameWorld[x, y, 10] = stoneBack;
                        if (BlockNextToRoom(dungeonMap, roomIds, gameWorld, x, y))
                            gameWorld[x, y, 11] = wood;
                    }
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
                        if (itemId != ItemIdGrass && itemId != ItemIdNoCeiling)
                            gameWorld[x, y, 11] = wood;
                    }

                    if (itemId == ItemIdPlayer && x % 3 == 1 && y % 3 == 1)
                        gameWorld[x, y, 5] = player;
                    else if (itemId == ItemIdTreasureChest)
                        gameWorld[x, y, 3] = wood;
                    else if (itemId == ItemIdFountain)
                    {
                        gameWorld[x, y, 3] = wood;
                        if (x % 3 == 1 && y % 3 == 1)
                        {
                            gameWorld[x, y, 4] = fountain;
                            gameWorld[x, y, 7] = fountainLight;
                        }
                        if ((x % 3 == 0 || x % 3 == 2) && (y % 3 == 0 || y % 3 == 2))
                            gameWorld[x, y, 4] = dummy;
                    }
                    else if (itemId == ItemIdFire)
                    {
                        gameWorld[x, y, 3] = wood;
                        if (x % 3 == 1 && y % 3 == 1)
                            gameWorld[x, y, 4] = fire;
                    }
                    else if (itemId == ItemIdLava)
                    {
                        if (random.Next(100) < 25)
                            gameWorld[x, y, 0] = lava;
                        else
                            gameWorld[x, y, 0] = lavaUnlit;

                        gameWorld[x, y, 1] = Block.Empty;
                        gameWorld[x, y, 2] = Block.Empty;
                        if (x % 3 == 1 || y % 3 == 1)
                            gameWorld[x, y, 2] = stoneBack;
                    }
                    else if (itemId == ItemIdGrass)
                    {
                        gameWorld[x, y, 1] = dirt;
                        if (random.Next(100) < 10 && x % 3 == 1 && y % 3 == 1)
                            gameWorld[x, y, 8] = treeLight;

                        if (random.Next(100) < 5 && x % 3 == 1 && y % 3 == 1)
                        {
                            // Add a tree
                            gameWorld[x, y, 2] = wood;
                            gameWorld[x, y, 3] = wood;
                            gameWorld[x, y, 4] = wood;
                            gameWorld[x, y, 5] = wood;
                            gameWorld[x, y, 6] = wood;
                            gameWorld[x, y, 7] = wood;
                            gameWorld[x, y, 8] = wood;
                            gameWorld[x, y, 9] = wood;
                            gameWorld[x, y, 10] = wood;
                            gameWorld[x - 1, y, 10] = wood;
                            gameWorld[x + 1, y, 10] = wood;
                            gameWorld[x, y - 1, 10] = wood;
                            gameWorld[x, y + 1, 10] = wood;
                            for (int z = 11; z < 16; z++)
                            {
                                for (int lx = x - 6; lx <= x + 6; lx++)
                                {
                                    for (int ly = y - 6; ly <= y + 6; ly++)
                                    {
                                        int dist = (lx - x) * (lx - x) + (ly - y) * (ly - y) + (15 - z) * (15 - z);
                                        if (dist <= 9)
                                            gameWorld[lx, ly, z] = wood;
                                        else if (dist <= 25)//(z - 9) * (z - 9))
                                            gameWorld[lx, ly, z] = leaves;
                                    }
                                }
                            }
                            gameWorld[x, y, 11] = wood;
                            gameWorld[x - 1, y, 11] = wood;
                            gameWorld[x + 1, y, 11] = wood;
                            gameWorld[x, y - 1, 11] = wood;
                            gameWorld[x, y + 1, 11] = wood;
                        }
                        else
                            gameWorld[x, y, 2] = unlitGrasses.NextBlock();
                    }
                    else if (itemId == ItemIdColumn && x % 3 == 1 && y % 3 == 1)
                    {
                        gameWorld[x, y, 3] = wood;
                        gameWorld[x, y, 4] = wood;
                        gameWorld[x, y, 5] = wood;
                        gameWorld[x, y, 6] = wood;
                        gameWorld[x, y, 7] = wood;
                        gameWorld[x, y, 8] = wood;
                        gameWorld[x, y, 9] = wood;
                        gameWorld[x, y, 10] = wood;
                        gameWorld[x - 1, y, 10] = wood;
                        gameWorld[x + 1, y, 10] = wood;
                        gameWorld[x, y - 1, 10] = wood;
                        gameWorld[x, y + 1, 10] = wood;

                        if (random.Next(100) >= 25) // 25% chance for it not to be lit
                            gameWorld[x - 1, y, 7] = fire;
                        if (random.Next(100) >= 25)
                            gameWorld[x + 1, y, 7] = fire;
                        if (random.Next(100) >= 25)
                            gameWorld[x, y - 1, 7] = fire;
                        if (random.Next(100) >= 25)
                            gameWorld[x, y + 1, 7] = fire;
                    }

                    int lightId = dungeonMap[mazeLocX, mazeLocY].LightId;
                    if (x % 3 == 1 && y % 3 == 1 && lightId != 0)
                    {
                        // Light
                        if (lightId == LightIdRoomLight)
                            gameWorld[x, y, 10] = roomLight;
                        else if (lightId == LightIdSmallLight)
                            gameWorld[x, y, 8] = hoverLightBlue;
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

                            gameWorld[lightLocX, lightLocY, height] = mazeLights[lightId - 1];
                        }
                    }
                }
            }
        }

        private static bool BlockNextToRoom(MazeCell[,] dungeonMap, HashSet<int> roomIds, IGameWorld gameWorld, int blockX, int blockY)
        {
            return (blockX > 0 && roomIds.Contains(GetAreaIdAt(dungeonMap, blockX - 1, blockY))) ||
                (blockX < gameWorld.BlockSize.X - 1 && roomIds.Contains(GetAreaIdAt(dungeonMap, blockX + 1, blockY))) ||
                (blockY > 0 && roomIds.Contains(GetAreaIdAt(dungeonMap, blockX, blockY - 1))) ||
                (blockY < gameWorld.BlockSize.Y - 1 && roomIds.Contains(GetAreaIdAt(dungeonMap, blockX, blockY + 1)));
        }

        private static int GetAreaIdAt(MazeCell[,] dungeonMap, int blockX, int blockY)
        {
            return dungeonMap[blockX / 3, blockY / 3].AreaId;
        }
        #endregion

        #region Debugging methods
        private static void PrintDungeon(MazeCell[,] dungeonMap)
        {
            for (int y = dungeonMap.GetLength(1) - 1; y >= 0; y--)
            //for (int y = 0; y < dungeonMap.GetLength(1); y++)
            {
                string d = "";
                for (int x = 0; x < dungeonMap.GetLength(0); x++)
                {
                    if (dungeonMap[x, y].AreaId == AreaIdDoor)
                        d += "[]";
                    else if (dungeonMap[x, y].AreaId > 0)
                        d += dungeonMap[x, y].AreaId.ToString("X2");
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
