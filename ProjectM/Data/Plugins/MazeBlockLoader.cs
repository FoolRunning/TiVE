using System;
using JetBrains.Annotations;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Components;
using ProdigalSoftware.TiVEPluginFramework.Generators;

namespace ProdigalSoftware.ProjectM.Data.Plugins
{
    [UsedImplicitly]
    public class MazeBlockLoader : IBlockGenerator
    {
        private static readonly RandomGenerator random = new RandomGenerator();
        private static readonly Voxel mortarColor = new Voxel(100, 100, 100);
        private static readonly Vector3b blockCenterVector = new Vector3b(bc, bc, bc);

        private const int Front = 1;
        private const int Back = 2;
        private const int Left = 4;
        private const int Right = 8;
        private const int Top = 16;
        private const int Bottom = 32;
        private const byte bc = BlockLOD32.VoxelSize / 2;
        private const int mv = BlockLOD32.VoxelSize - 1;
        private const int ImperfectionIterations = BlockLOD32.VoxelSize > 16 ? 100 : 25;
        private const bool ForFantasy = true;
        private const int LightDist = ForFantasy ? 17 : 35;
        private const float LightBright = ForFantasy ? 0.5f : 1.0f;
        private const float LightMid = ForFantasy ? 0.38f : 0.5f;
        private const float LightDim = ForFantasy ? 0.17f : 0.3f;
        private const float ObjBright = 1.0f;
        private const float ObjMid = 0.76f;
        private const float ObjDim = 0.34f;

        public Block CreateBlock(string name)
        {
            int num;
            string part, other;
            if (!CommonUtils.ParseBlockName(name, out part, out num, out other))
                return null;

            switch (part)
            {
                case "fire": return CreateFire();
                case "dirt": return CreateBlockInfo("dirt", 0, new Color4f(0.6f, 0.45f, 0.25f, 1.0f), 1.0f, colorVariation: 0.4f);
                case "wood": return CreateRoundedBlockInfo(name, new Voxel(213, 128, 43), 1.0f, num);
                case "leaves": return CreateLeaves(name, num);
                case "lava": return CreateLava();
                case "back": return CreateBackStone(name, num);
                case "stoneBrick": return CreateStone(num, other);
                case "light": return CreateLight(part, num);
                case "grass": return CreateGrass(name);
                case "loadingGrass": return CreateGrass(name);
                case "player": return new Block("player"); // Placeholder for the player starting location
                case "backStone": return CreateBackStone(name, 0);
                case "fountain": return CreateFountain();
                case "roomLight":
                    return CreateBlockInfo(name, BlockLOD32.VoxelSize / 2 - 1, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, null,
                        new LightComponent(blockCenterVector, new Color3f(ForFantasy ? 0.6f : 0.8f, ForFantasy ? 0.6f : 0.8f, ForFantasy ? 0.6f : 0.8f), ForFantasy ? 35 : 50), colorVariation: 0.0f);
                case "redLight":
                    return CreateBlockInfo(name, BlockLOD32.VoxelSize / 5.0f, new Color4f(ObjBright, ObjDim, ObjDim, 1.0f), 1.0f, null,
                        new LightComponent(new Vector3b(bc, bc, bc), new Color3f(LightBright, LightDim, LightDim), ForFantasy ? 5 : 9), colorVariation: 0.0f);
                case "smallLight":
                    return CreateBlockInfo(name, BlockLOD32.VoxelSize / 16.0f, new Color4f(ObjDim, ObjDim, ObjBright, 1.0f), 1.0f, null,
                        new LightComponent(new Vector3b(bc, bc, bc), new Color3f(LightDim, LightDim, LightBright), ForFantasy ? 12 : 20), colorVariation: 0.0f);
                case "hoverLightBlue":
                    return CreateBlockInfo(name, BlockLOD32.VoxelSize / 4.0f, new Color4f(ObjDim, ObjDim, ObjBright, 1.0f), 1.0f, null,
                        new LightComponent(new Vector3b(bc, bc, bc), new Color3f(LightDim, LightDim, LightBright), ForFantasy ? 16 : 25), colorVariation: 0.0f);
                case "loadingLight":
                    return CreateBlockInfo(name, BlockLOD32.VoxelSize / 5.0f, new Color4f(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, null,
                        new LightComponent(blockCenterVector, new Color3f(0.5f, 0.5f, 0.5f), 40), colorVariation: 0.0f);
                case "treeLight":
                    return CreateBlockInfo(name, BlockLOD32.VoxelSize / 8.0f, new Color4f(ObjBright, ObjBright, ObjBright, 1.0f), 1.0f, null,
                        new LightComponent(new Vector3b(bc, bc, bc), new Color3f(LightBright, LightBright, LightBright), 15), colorVariation: 0.0f);

                default: return null;
            }
        }

        private static Block CreateFire()
        {
            Block fireBlock = new Block("fire");
            fireBlock.AddComponent(new ParticleComponent("Fire", new Vector3i(bc, bc, 1)));
            fireBlock.AddComponent(new LightPassthroughComponent());
            fireBlock.AddComponent(new LightComponent(new Vector3b(bc, bc, bc),
                new Color3f(ForFantasy ? 0.7f : 1.0f, ForFantasy ? 0.55f : 0.8f, ForFantasy ? 0.4f : 0.6f), ForFantasy ? 12 : 20));
            return fireBlock;
        }

        private static Block CreateFountain()
        {
            Block fountainBlock = new Block("fountain");
            fountainBlock.AddComponent(new ParticleComponent("Fountain", new Vector3i(bc, bc, 0)));
            fountainBlock.AddComponent(new LightPassthroughComponent());
            return fountainBlock;
        }

        private static Block CreateLava()
        {
            //return CreateRoundedBlockInfo("lava", new Voxel(255, 255, 255, 255, VoxelSettings.AllowLightPassthrough | VoxelSettings.SkipVoxelNormalCalc), 1.0f, 0,
            //    new LightComponent(new Vector3b(bc, bc, bc), new Color3f(0.4f, 0.05f, 0.03f), 4), 0.1f);
            Block lavaBlock = new Block("lava");
            for (int z = 0; z < BlockLOD32.VoxelSize; z++)
            {
                for (int x = 0; x < BlockLOD32.VoxelSize; x++)
                {
                    for (int y = 0; y < BlockLOD32.VoxelSize; y++)
                        lavaBlock.LOD32[x, y, z] = new Voxel(230, 32, 18, 255, VoxelSettings.IgnoreLighting | VoxelSettings.AllowLightPassthrough);
                }
            }
            lavaBlock.AddComponent(new LightComponent(new Vector3b(bc, bc, bc), new Color3f(0.4f, 0.02f, 0.01f), 4));
            lavaBlock.AddComponent(new ParticleComponent("Lava", new Vector3i(0, 0, BlockLOD32.VoxelSize - 1)));
            lavaBlock.AddComponent(new VoxelNoiseComponent(0.3f));
            lavaBlock.AddComponent(new LightPassthroughComponent());
            return lavaBlock;
        }

        private static Block CreateLeaves(string name, int num)
        {
            Block leaves = CreateRoundedBlockInfo(name, new Voxel(13, 200, 23, 255, VoxelSettings.SkipVoxelNormalCalc), 0.05f, num, null, 0.3f);
            leaves.AddComponent(new LightPassthroughComponent());
            return leaves;
        }

        private static Block CreateLight(string name, int num)
        {
            ParticleComponent bugInformation = new ParticleComponent("LightBugs", new Vector3i(bc, bc, bc));

            switch (num)
            {
                case 0: return CreateBlockInfo(name, BlockLOD32.VoxelSize / 8.0f, new Color4f(ObjDim, ObjMid, ObjBright, 1.0f), 1.0f, bugInformation,
                            new LightComponent(blockCenterVector, new Color3f(LightDim, LightMid, LightBright), LightDist), colorVariation: 0.0f);
                case 1: return CreateBlockInfo(name, BlockLOD32.VoxelSize / 8.0f, new Color4f(ObjMid, ObjDim, ObjBright, 1.0f), 1.0f, bugInformation,
                            new LightComponent(blockCenterVector, new Color3f(LightMid, LightDim, LightBright), LightDist), colorVariation: 0.0f);
                case 2: return CreateBlockInfo(name, BlockLOD32.VoxelSize / 8.0f, new Color4f(ObjBright, ObjDim, ObjMid, 1.0f), 1.0f, bugInformation,
                            new LightComponent(blockCenterVector, new Color3f(LightBright, LightDim, LightMid), LightDist), colorVariation: 0.0f);
                case 3: return CreateBlockInfo(name, BlockLOD32.VoxelSize / 8.0f, new Color4f(ObjBright, ObjMid, ObjDim, 1.0f), 1.0f, bugInformation,
                            new LightComponent(blockCenterVector, new Color3f(LightBright, LightMid, LightDim), LightDist), colorVariation: 0.0f);
                case 4: return CreateBlockInfo(name, BlockLOD32.VoxelSize / 8.0f, new Color4f(ObjDim, ObjBright, ObjMid, 1.0f), 1.0f, bugInformation,
                            new LightComponent(blockCenterVector, new Color3f(LightDim, LightBright, LightMid), LightDist), colorVariation: 0.0f);
                case 5: return CreateBlockInfo(name, BlockLOD32.VoxelSize / 8.0f, new Color4f(ObjMid, ObjBright, ObjDim, 1.0f), 1.0f, bugInformation,
                            new LightComponent(blockCenterVector, new Color3f(LightMid, LightBright, LightDim), LightDist), colorVariation: 0.0f);
                case 6: return CreateBlockInfo(name, BlockLOD32.VoxelSize / 8.0f, new Color4f(ObjBright, ObjBright, ObjBright, 1.0f), 1.0f, bugInformation,
                            new LightComponent(blockCenterVector, new Color3f(LightBright, LightBright, LightBright), LightDist), colorVariation: 0.0f);
                default: return null;
            }
        }

        private static Block CreateGrass(string name, int lightDist = 2)
        {
            Block grass = new Block(name);
            grass.AddComponent(new VoxelNoiseComponent(0.4f));
            grass.AddComponent(new LightPassthroughComponent());
            bool putFlowerInBlock = false;
            bool allowLight = !name.StartsWith("loading", StringComparison.Ordinal);
            //const float bsmo = BlockLOD32.VoxelSize - 1;

            //for (int bladeCount = 0; bladeCount < BlockLOD32.VoxelSize * 8; bladeCount++)
            //{
            //    float val = random.NextFloat() * bsmo;
            //    int bladeLength = (int)Math.Round((val * val * val * val) / (bsmo * bsmo * bsmo)) + 1;
            //    //Console.WriteLine("Blade length: " + bladeLength);

            //    float vX, vY, offsetX, offsetY;
            //    do
            //    {
            //        vX = random.Next(BlockLOD32.VoxelSize);
            //        vY = random.Next(BlockLOD32.VoxelSize);
            //        offsetX = random.NextFloat(true) * 6 - 3;
            //        offsetY = random.NextFloat(true) * 6 - 3;
            //    }
            //    while (vX + offsetX < 0.0f || vX + offsetX >= BlockLOD32.VoxelSize || vY + offsetY < 0.0f || vY + offsetY >= BlockLOD32.VoxelSize);

            //    for (int vZ = 0; vZ < bladeLength; vZ++)
            //    {
            //        grass[(int)vX, (int)vY, vZ] = new Voxel(55, 180, 40, 255, VoxelSettings.SkipVoxelNormalCalc);
            //        if (vZ < bladeLength - 1)
            //        {
            //            vX += (offsetX / BlockLOD32.VoxelSize);
            //            vY += (offsetY / BlockLOD32.VoxelSize);
            //        }
            //    }

            //    if (bladeLength == BlockLOD32.VoxelSize && random.NextFloat() < 0.2f && !putFlowerInBlock &&
            //        vX >= 1 && vX <= BlockLOD32.VoxelSize - 2 && vY >= 1 && vY <= BlockLOD32.VoxelSize - 2)
            //    {
            //        int fX = (int)vX;
            //        int fY = (int)vY;
            //        int fZ = bladeLength - 1;
            //        putFlowerInBlock = true;
            //        bool isLit = allowLight && random.NextFloat() < 0.8;
            //        Voxel flowerVoxel = CreateRandomFlowerVoxel(isLit);
            //        if (isLit)
            //        {
            //            grass.AddComponent(new LightComponent(new Vector3b((byte)fX, (byte)fY, (byte)fZ),
            //                new Color3f((byte)(flowerVoxel.R / 2), (byte)(flowerVoxel.G / 2), (byte)(flowerVoxel.B / 2)), lightDist));
            //        }

            //        // Make a flower
            //        grass[fX, fY, fZ] = flowerVoxel;
            //        grass[fX - 1, fY, fZ - 1] = flowerVoxel;
            //        grass[fX + 1, fY, fZ - 1] = flowerVoxel;
            //        grass[fX, fY - 1, fZ - 1] = flowerVoxel;
            //        grass[fX, fY + 1, fZ - 1] = flowerVoxel;
            //        if (BlockLOD32.VoxelSize > 16)
            //        {
            //            grass[fX - 1, fY - 1, fZ - 1] = flowerVoxel;
            //            grass[fX - 1, fY + 1, fZ - 1] = flowerVoxel;
            //            grass[fX + 1, fY - 1, fZ - 1] = flowerVoxel;
            //            grass[fX + 1, fY + 1, fZ - 1] = flowerVoxel;

            //            grass[fX - 1, fY, fZ] = flowerVoxel;
            //            grass[fX + 1, fY, fZ] = flowerVoxel;
            //            grass[fX, fY - 1, fZ] = flowerVoxel;
            //            grass[fX, fY + 1, fZ] = flowerVoxel;

            //            grass[fX - 2, fY - 1, fZ - 1] = flowerVoxel;
            //            grass[fX - 2, fY, fZ - 1] = flowerVoxel;
            //            grass[fX - 2, fY + 1, fZ - 1] = flowerVoxel;

            //            grass[fX + 2, fY - 1, fZ - 1] = flowerVoxel;
            //            grass[fX + 2, fY, fZ - 1] = flowerVoxel;
            //            grass[fX + 2, fY + 1, fZ - 1] = flowerVoxel;

            //            grass[fX - 1, fY - 2, fZ - 1] = flowerVoxel;
            //            grass[fX, fY - 2, fZ - 1] = flowerVoxel;
            //            grass[fX + 1, fY - 2, fZ - 1] = flowerVoxel;

            //            grass[fX - 1, fY + 2, fZ - 1] = flowerVoxel;
            //            grass[fX, fY + 2, fZ - 1] = flowerVoxel;
            //            grass[fX + 1, fY + 2, fZ - 1] = flowerVoxel;
            //        }
            //    }
            //    //int vX = random.Next(BlockLOD32.VoxelSize);
            //    //int vY = random.Next(BlockLOD32.VoxelSize);
            //    //for (int vZ = 0; vZ < bladeLength; vZ++)
            //    //{
            //    //    grass[vX, vY, vZ] = new Voxel(55, 180, 40, 255, VoxelSettings.SkipVoxelNormalCalc);

            //    //    bool tryAgain;
            //    //    do
            //    //    {
            //    //        tryAgain = false;
            //    //        int rnd = random.Next(5);
            //    //        if (rnd == 0 && vX > 0)
            //    //            vX--;
            //    //        else if (rnd == 1 && vY > 0)
            //    //            vY--;
            //    //        else if (rnd == 2 && vX < BlockLOD32.VoxelSize - 1)
            //    //            vX++;
            //    //        else if (rnd == 3 && vY < BlockLOD32.VoxelSize - 1)
            //    //            vY++;
            //    //        else if (rnd != 4)
            //    //            tryAgain = true;
            //    //    }
            //    //    while (tryAgain);

            //    //}
            //}
            const float divisor = BlockLOD32.VoxelSize * 10.4f;
            for (int z = 0; z < BlockLOD32.VoxelSize; z++)
            {
                for (int x = 0; x < BlockLOD32.VoxelSize; x++)
                {
                    for (int y = 0; y < BlockLOD32.VoxelSize; y++)
                    {
                        if (random.NextFloat() < 0.3f - z / divisor && (z == 0 || GrassVoxelUnder(grass, x, y, z)))
                        {
                            grass.LOD32[x, y, z] = new Voxel(55, 180, 40, 255, VoxelSettings.SkipVoxelNormalCalc);
                            if (z == BlockLOD32.VoxelSize - 1 && random.NextFloat() < 0.2f &&
                                x > 1 && x < BlockLOD32.VoxelSize - 2 && y > 1 && y < BlockLOD32.VoxelSize - 2 && !putFlowerInBlock)
                            {
                                putFlowerInBlock = true;
                                bool isLit = allowLight && random.NextFloat() < 0.8;
                                Voxel flowerVoxel = CreateRandomFlowerVoxel(isLit);
                                if (isLit)
                                {
                                    grass.AddComponent(new LightComponent(new Vector3b((byte)x, (byte)y, (byte)z),
                                        new Color3f((byte)(flowerVoxel.R / 2), (byte)(flowerVoxel.G / 2), (byte)(flowerVoxel.B / 2)), lightDist));
                                }

                                // Make a flower
                                grass.LOD32[x, y, z] = flowerVoxel;
                                grass.LOD32[x - 1, y, z - 1] = flowerVoxel;
                                grass.LOD32[x + 1, y, z - 1] = flowerVoxel;
                                grass.LOD32[x, y - 1, z - 1] = flowerVoxel;
                                grass.LOD32[x, y + 1, z - 1] = flowerVoxel;
                                if (BlockLOD32.VoxelSize > 16)
                                {
                                    grass.LOD32[x - 1, y - 1, z - 1] = flowerVoxel;
                                    grass.LOD32[x - 1, y + 1, z - 1] = flowerVoxel;
                                    grass.LOD32[x + 1, y - 1, z - 1] = flowerVoxel;
                                    grass.LOD32[x + 1, y + 1, z - 1] = flowerVoxel;

                                    grass.LOD32[x - 1, y, z] = flowerVoxel;
                                    grass.LOD32[x + 1, y, z] = flowerVoxel;
                                    grass.LOD32[x, y - 1, z] = flowerVoxel;
                                    grass.LOD32[x, y + 1, z] = flowerVoxel;

                                    grass.LOD32[x - 2, y - 1, z - 1] = flowerVoxel;
                                    grass.LOD32[x - 2, y, z - 1] = flowerVoxel;
                                    grass.LOD32[x - 2, y + 1, z - 1] = flowerVoxel;

                                    grass.LOD32[x + 2, y - 1, z - 1] = flowerVoxel;
                                    grass.LOD32[x + 2, y, z - 1] = flowerVoxel;
                                    grass.LOD32[x + 2, y + 1, z - 1] = flowerVoxel;

                                    grass.LOD32[x - 1, y - 2, z - 1] = flowerVoxel;
                                    grass.LOD32[x, y - 2, z - 1] = flowerVoxel;
                                    grass.LOD32[x + 1, y - 2, z - 1] = flowerVoxel;

                                    grass.LOD32[x - 1, y + 2, z - 1] = flowerVoxel;
                                    grass.LOD32[x, y + 2, z - 1] = flowerVoxel;
                                    grass.LOD32[x + 1, y + 2, z - 1] = flowerVoxel;
                                }
                            }
                        }
                    }
                }
            }

            grass.GenerateLODLevels();
            return grass;
        }

        private static Block CreateBackStone(string name, int num)
        {
            Block back = CreateRoundedBlockInfo(name, new Voxel(200, 200, 200), 1.0f, num, null, 0.05f);

            if ((num & Bottom) != 0)
            {
                for (int h = 0; h < ImperfectionIterations; h++)
                {
                    int x = random.Next(BlockLOD32.VoxelSize - 1);
                    int z = random.Next(BlockLOD32.VoxelSize - 1);
                    back.LOD32[x, 0, z] = Voxel.Empty;
                    back.LOD32[x + 1, 0, z] = Voxel.Empty;
                    back.LOD32[x, 0, z + 1] = Voxel.Empty;
                    back.LOD32[x + 1, 0, z + 1] = Voxel.Empty;
                    
                    back.LOD16[x / 2, 0, z / 2] = Voxel.Empty;
                    if (h % 4 == 0)
                        back.LOD8[x / 4, 0, z / 4] = Voxel.Empty;
                }
            }
            if ((num & Top) != 0)
            {
                for (int h = 0; h < ImperfectionIterations; h++)
                {
                    int x = random.Next(BlockLOD32.VoxelSize - 1);
                    int z = random.Next(BlockLOD32.VoxelSize - 1);
                    back.LOD32[x, BlockLOD32.VoxelSize - 1, z] = Voxel.Empty;
                    back.LOD32[x + 1, BlockLOD32.VoxelSize - 1, z] = Voxel.Empty;
                    back.LOD32[x, BlockLOD32.VoxelSize - 1, z + 1] = Voxel.Empty;
                    back.LOD32[x + 1, BlockLOD32.VoxelSize - 1, z + 1] = Voxel.Empty;

                    back.LOD16[x / 2, BlockLOD16.VoxelSize - 1, z / 2] = Voxel.Empty;
                    if (h % 4 == 0)
                        back.LOD8[x / 4, BlockLOD8.VoxelSize - 1, z / 4] = Voxel.Empty;
                }
            }
            if ((num & Left) != 0)
            {
                for (int h = 0; h < ImperfectionIterations; h++)
                {
                    int y = random.Next(BlockLOD32.VoxelSize - 1);
                    int z = random.Next(BlockLOD32.VoxelSize - 1);
                    back.LOD32[0, y, z] = Voxel.Empty;
                    back.LOD32[0, y + 1, z] = Voxel.Empty;
                    back.LOD32[0, y, z + 1] = Voxel.Empty;
                    back.LOD32[0, y + 1, z + 1] = Voxel.Empty;

                    back.LOD16[0, y / 2, z / 2] = Voxel.Empty;
                    if (h % 4 == 0)
                        back.LOD8[0, y / 4, z / 4] = Voxel.Empty;
                }
            }
            if ((num & Right) != 0)
            {
                for (int h = 0; h < ImperfectionIterations; h++)
                {
                    int y = random.Next(BlockLOD32.VoxelSize - 1);
                    int z = random.Next(BlockLOD32.VoxelSize - 1);
                    back.LOD32[BlockLOD32.VoxelSize - 1, y, z] = Voxel.Empty;
                    back.LOD32[BlockLOD32.VoxelSize - 1, y + 1, z] = Voxel.Empty;
                    back.LOD32[BlockLOD32.VoxelSize - 1, y, z + 1] = Voxel.Empty;
                    back.LOD32[BlockLOD32.VoxelSize - 1, y + 1, z + 1] = Voxel.Empty;

                    back.LOD16[BlockLOD16.VoxelSize - 1, y / 2, z / 2] = Voxel.Empty;
                    if (h % 4 == 0)
                        back.LOD8[BlockLOD8.VoxelSize - 1, y / 4, z / 4] = Voxel.Empty;
                }
            }
            if ((num & Front) != 0)
            {
                for (int h = 0; h < ImperfectionIterations; h++)
                {
                    int x = random.Next(BlockLOD32.VoxelSize - 1);
                    int y = random.Next(BlockLOD32.VoxelSize - 1);
                    back.LOD32[x, y, BlockLOD32.VoxelSize - 1] = Voxel.Empty;
                    back.LOD32[x + 1, y, BlockLOD32.VoxelSize - 1] = Voxel.Empty;
                    back.LOD32[x, y + 1, BlockLOD32.VoxelSize - 1] = Voxel.Empty;
                    back.LOD32[x + 1, y + 1, BlockLOD32.VoxelSize - 1] = Voxel.Empty;

                    back.LOD16[x / 2, y / 2, BlockLOD16.VoxelSize - 1] = Voxel.Empty;
                    if (h % 4 == 0)
                        back.LOD8[x / 4, y / 4, BlockLOD8.VoxelSize - 1] = Voxel.Empty;
                }
            }
            if ((num & Back) != 0)
            {
                for (int h = 0; h < ImperfectionIterations; h++)
                {
                    int x = random.Next(BlockLOD32.VoxelSize - 1);
                    int y = random.Next(BlockLOD32.VoxelSize - 1);
                    back.LOD32[x, y, 0] = Voxel.Empty;
                    back.LOD32[x + 1, y, 0] = Voxel.Empty;
                    back.LOD32[x, y + 1, 0] = Voxel.Empty;
                    back.LOD32[x + 1, y + 1, 0] = Voxel.Empty;

                    back.LOD16[x / 2, y / 2, 0] = Voxel.Empty;
                    if (h % 4 == 0)
                        back.LOD8[x / 4, y / 4, 0] = Voxel.Empty;
                }
            }

            back.GenerateLODLevels(LODLevel.V8, LODLevel.V4);
            return back;
        }

        private static Block CreateStone(int num, string other)
        {
            Block stone = CreateRoundedBlockInfo("stoneBrick" + num + "_" + other, new Voxel(229, 229, 229), 1.0f, num, null, 0.1f);
            for (int x = 0; x <= mv; x++)
            {
                if ((num & Bottom) != 0)
                {
                    stone.LOD32[x, 0, 0] = Voxel.Empty;
                    stone.LOD32[x, 0, bc] = Voxel.Empty;

                    if (BlockLOD32.VoxelSize > 16)
                    {
                        stone.LOD32[x, 0, mv] = Voxel.Empty;
                        stone.LOD32[x, 1, mv] = Voxel.Empty;
                        stone.LOD32[x, 1, 0] = Voxel.Empty;
                        stone.LOD32[x, 0, 1] = Voxel.Empty;
                        stone.LOD32[x, 1, 1] = Voxel.Empty;

                        stone.LOD32[x, 0, bc - 1] = Voxel.Empty;
                        stone.LOD32[x, 1, bc - 1] = Voxel.Empty;
                        stone.LOD32[x, 1, bc] = Voxel.Empty;
                        stone.LOD32[x, 0, bc + 1] = Voxel.Empty;
                        stone.LOD32[x, 1, bc + 1] = Voxel.Empty;
                    }
                }

                if ((num & Top) != 0)
                {
                    stone.LOD32[x, mv, 0] = Voxel.Empty;
                    stone.LOD32[x, mv, bc] = Voxel.Empty;

                    if (BlockLOD32.VoxelSize > 16)
                    {
                        stone.LOD32[x, mv, mv] = Voxel.Empty;
                        stone.LOD32[x, mv - 1, mv] = Voxel.Empty;
                        stone.LOD32[x, mv - 1, 0] = Voxel.Empty;
                        stone.LOD32[x, mv, 1] = Voxel.Empty;
                        stone.LOD32[x, mv - 1, 1] = Voxel.Empty;

                        stone.LOD32[x, mv, bc - 1] = Voxel.Empty;
                        stone.LOD32[x, mv - 1, bc - 1] = Voxel.Empty;
                        stone.LOD32[x, mv - 1, bc] = Voxel.Empty;
                        stone.LOD32[x, mv, bc + 1] = Voxel.Empty;
                        stone.LOD32[x, mv - 1, bc + 1] = Voxel.Empty;
                    }
                }
            }
            for (int y = 0; y <= mv; y++)
            {
                if ((num & Left) != 0)
                {
                    stone.LOD32[0, y, 0] = Voxel.Empty;
                    stone.LOD32[0, y, bc] = Voxel.Empty;

                    if (BlockLOD32.VoxelSize > 16)
                    {
                        stone.LOD32[0, y, mv] = Voxel.Empty;
                        stone.LOD32[1, y, mv] = Voxel.Empty;
                        stone.LOD32[1, y, 0] = Voxel.Empty;
                        stone.LOD32[0, y, 1] = Voxel.Empty;
                        stone.LOD32[1, y, 1] = Voxel.Empty;

                        stone.LOD32[0, y, bc - 1] = Voxel.Empty;
                        stone.LOD32[1, y, bc - 1] = Voxel.Empty;
                        stone.LOD32[1, y, bc] = Voxel.Empty;
                        stone.LOD32[0, y, bc + 1] = Voxel.Empty;
                        stone.LOD32[1, y, bc + 1] = Voxel.Empty;
                    }
                }

                if ((num & Right) != 0)
                {
                    stone.LOD32[mv, y, 0] = Voxel.Empty;
                    stone.LOD32[mv, y, bc] = Voxel.Empty;

                    if (BlockLOD32.VoxelSize > 16)
                    {
                        stone.LOD32[mv, y, mv] = Voxel.Empty;
                        stone.LOD32[mv - 1, y, mv] = Voxel.Empty;
                        stone.LOD32[mv - 1, y, 0] = Voxel.Empty;
                        stone.LOD32[mv, y, 1] = Voxel.Empty;
                        stone.LOD32[mv - 1, y, 1] = Voxel.Empty;

                        stone.LOD32[mv, y, bc - 1] = Voxel.Empty;
                        stone.LOD32[mv - 1, y, bc - 1] = Voxel.Empty;
                        stone.LOD32[mv - 1, y, bc] = Voxel.Empty;
                        stone.LOD32[mv, y, bc + 1] = Voxel.Empty;
                        stone.LOD32[mv - 1, y, bc + 1] = Voxel.Empty;
                    }
                }
            }

            for (int x = 0; x <= mv; x++)
            {
                for (int y = 0; y <= mv; y++)
                {
                    ReplaceVoxel(stone, x, y, 0, mortarColor);
                    ReplaceVoxel(stone, x, y, bc, mortarColor);
                }
            }
            for (int z = 0; z < bc; z++)
            {
                if ((num & Bottom) != 0)
                {
                    stone.LOD32[bc - 4, 0, z] = Voxel.Empty;
                    stone.LOD32[bc + 4, 0, z + bc] = Voxel.Empty;

                    if (BlockLOD32.VoxelSize > 16)
                    {
                        stone.LOD32[bc - 5, 0, z] = Voxel.Empty;
                        stone.LOD32[bc - 5, 1, z] = Voxel.Empty;
                        stone.LOD32[bc - 4, 1, z] = Voxel.Empty;
                        stone.LOD32[bc - 3, 0, z] = Voxel.Empty;
                        stone.LOD32[bc - 3, 1, z] = Voxel.Empty;

                        stone.LOD32[bc + 3, 0, z + bc] = Voxel.Empty;
                        stone.LOD32[bc + 3, 1, z + bc] = Voxel.Empty;
                        stone.LOD32[bc + 4, 1, z + bc] = Voxel.Empty;
                        stone.LOD32[bc + 5, 0, z + bc] = Voxel.Empty;
                        stone.LOD32[bc + 5, 1, z + bc] = Voxel.Empty;
                    }
                }

                if ((num & Top) != 0)
                {
                    stone.LOD32[bc - 4, mv, z] = Voxel.Empty;
                    stone.LOD32[bc + 4, mv, z + bc] = Voxel.Empty;

                    if (BlockLOD32.VoxelSize > 16)
                    {
                        stone.LOD32[bc - 5, mv, z] = Voxel.Empty;
                        stone.LOD32[bc - 5, mv - 1, z] = Voxel.Empty;
                        stone.LOD32[bc - 4, mv - 1, z] = Voxel.Empty;
                        stone.LOD32[bc - 3, mv, z] = Voxel.Empty;
                        stone.LOD32[bc - 3, mv - 1, z] = Voxel.Empty;

                        stone.LOD32[bc + 3, mv, z + bc] = Voxel.Empty;
                        stone.LOD32[bc + 3, mv - 1, z + bc] = Voxel.Empty;
                        stone.LOD32[bc + 4, mv - 1, z + bc] = Voxel.Empty;
                        stone.LOD32[bc + 5, mv, z + bc] = Voxel.Empty;
                        stone.LOD32[bc + 5, mv - 1, z + bc] = Voxel.Empty;
                    }
                }

                if ((num & Left) != 0)
                {
                    stone.LOD32[0, bc - 4, z] = Voxel.Empty;
                    stone.LOD32[0, bc + 4, z + bc] = Voxel.Empty;

                    if (BlockLOD32.VoxelSize > 16)
                    {
                        stone.LOD32[0, bc - 5, z] = Voxel.Empty;
                        stone.LOD32[1, bc - 5, z] = Voxel.Empty;
                        stone.LOD32[1, bc - 4, z] = Voxel.Empty;
                        stone.LOD32[0, bc - 3, z] = Voxel.Empty;
                        stone.LOD32[1, bc - 3, z] = Voxel.Empty;

                        stone.LOD32[0, bc + 3, z + bc] = Voxel.Empty;
                        stone.LOD32[1, bc + 3, z + bc] = Voxel.Empty;
                        stone.LOD32[1, bc + 4, z + bc] = Voxel.Empty;
                        stone.LOD32[0, bc + 5, z + bc] = Voxel.Empty;
                        stone.LOD32[1, bc + 5, z + bc] = Voxel.Empty;
                    }
                }

                if ((num & Right) != 0)
                {
                    stone.LOD32[mv, bc - 4, z] = Voxel.Empty;
                    stone.LOD32[mv, bc + 4, z + bc] = Voxel.Empty;

                    if (BlockLOD32.VoxelSize > 16)
                    {
                        stone.LOD32[mv, bc - 5, z] = Voxel.Empty;
                        stone.LOD32[mv - 1, bc - 5, z] = Voxel.Empty;
                        stone.LOD32[mv - 1, bc - 4, z] = Voxel.Empty;
                        stone.LOD32[mv, bc - 3, z] = Voxel.Empty;
                        stone.LOD32[mv - 1, bc - 3, z] = Voxel.Empty;

                        stone.LOD32[mv, bc + 3, z + bc] = Voxel.Empty;
                        stone.LOD32[mv - 1, bc + 3, z + bc] = Voxel.Empty;
                        stone.LOD32[mv - 1, bc + 4, z + bc] = Voxel.Empty;
                        stone.LOD32[mv, bc + 5, z + bc] = Voxel.Empty;
                        stone.LOD32[mv - 1, bc + 5, z + bc] = Voxel.Empty;
                    }
                }

                for (int n = 0; n <= mv; n++)
                {
                    ReplaceVoxel(stone, bc - 4, n, z, mortarColor);
                    if (z < bc - 1 || (num & Front) == 0)
                        ReplaceVoxel(stone, bc + 4, n, z + bc, mortarColor);

                    ReplaceVoxel(stone, n, bc - 4, z, mortarColor);
                    if (z < bc - 1 || (num & Front) == 0)
                        ReplaceVoxel(stone, n, bc + 4, z + bc, mortarColor);
                }
            }

            if ((num & Bottom) != 0)
            {
                for (int h = 0; h < ImperfectionIterations * 2; h++)
                    stone.LOD32[random.Next(BlockLOD32.VoxelSize), 0, random.Next(BlockLOD32.VoxelSize)] = Voxel.Empty;
            }
            if ((num & Top) != 0)
            {
                for (int h = 0; h < ImperfectionIterations * 2; h++)
                    stone.LOD32[random.Next(BlockLOD32.VoxelSize), BlockLOD32.VoxelSize - 1, random.Next(BlockLOD32.VoxelSize)] = Voxel.Empty;
            }
            if ((num & Left) != 0)
            {
                for (int h = 0; h < ImperfectionIterations * 2; h++)
                    stone.LOD32[0, random.Next(BlockLOD32.VoxelSize), random.Next(BlockLOD32.VoxelSize)] = Voxel.Empty;
            }
            if ((num & Right) != 0)
            {
                for (int h = 0; h < ImperfectionIterations * 2; h++)
                    stone.LOD32[BlockLOD32.VoxelSize - 1, random.Next(BlockLOD32.VoxelSize), random.Next(BlockLOD32.VoxelSize)] = Voxel.Empty;
            }
            if ((num & Front) != 0)
            {
                for (int h = 0; h < ImperfectionIterations * 2; h++)
                    stone.LOD32[random.Next(BlockLOD32.VoxelSize), random.Next(BlockLOD32.VoxelSize), BlockLOD32.VoxelSize - 1] = Voxel.Empty;
            }
            if ((num & Back) != 0)
            {
                for (int h = 0; h < ImperfectionIterations * 2; h++)
                    stone.LOD32[random.Next(BlockLOD32.VoxelSize), random.Next(BlockLOD32.VoxelSize), 0] = Voxel.Empty;
            }

            stone.GenerateLODLevels();
            return stone;
        }

        private static void ReplaceVoxel(Block block, int x, int y, int z, Voxel newVoxel)
        {
            if (block.LOD32[x, y, z] != Voxel.Empty)
                block.LOD32[x, y, z] = newVoxel;
        }

        private static Voxel CreateRandomFlowerVoxel(bool isLit)
        {
            VoxelSettings settings = VoxelSettings.SkipVoxelNormalCalc | VoxelSettings.AllowLightPassthrough;
            if (isLit)
                settings |= VoxelSettings.IgnoreLighting;
            switch (random.Next(6))
            {
                case 1: return new Voxel(255, 0, 0, 255, settings);
                case 2: return new Voxel(0, 0, 255, 255, settings);
                case 3: return new Voxel(255, 0, 255, 255, settings);
                case 4: return new Voxel(255, 255, 0, 255, settings);
                case 5: return new Voxel(0, 255, 255, 255, settings);
                default: return new Voxel(0, 255, 0, 255, settings);
            }
        }

        private static bool GrassVoxelUnder(Block block, int x, int y, int z)
        {
            int countUnder = 0;
            if (block.LOD32[x, y, z - 1] != Voxel.Empty)
                countUnder++;
            if (x > 0 && block.LOD32[x - 1, y, z - 1] != Voxel.Empty)
                countUnder++;
            if (x < BlockLOD32.VoxelSize - 1 && block.LOD32[x + 1, y, z - 1] != Voxel.Empty)
                countUnder++;
            if (y > 0 && block.LOD32[x, y - 1, z - 1] != Voxel.Empty)
                countUnder++;
            if (y < BlockLOD32.VoxelSize - 1 && block.LOD32[x, y + 1, z - 1] != Voxel.Empty)
                countUnder++;
            return countUnder == 1;
        }

        private static Block CreateRoundedBlockInfo(string name, Voxel voxel, float voxelDensity, int sides, LightComponent light = null, float colorVariation = 0.2f)
        {
            const float mid = BlockLOD32.VoxelSize / 2.0f - 0.5f;
            const float sphereSize = BlockLOD32.VoxelSize / 2.0f;

            Block block = new Block(name);
            if (colorVariation > 0.0f)
                block.AddComponent(new VoxelNoiseComponent(colorVariation, mortarColor));

            if (light != null)
            {
                block.AddComponent(light);
                block.AddComponent(new LightPassthroughComponent());
            }

            for (int x = 0; x < BlockLOD32.VoxelSize; x++)
            {
                for (int y = 0; y < BlockLOD32.VoxelSize; y++)
                {
                    for (int z = 0; z < BlockLOD32.VoxelSize; z++)
                    {
                        if (((sides & Top) != 0 && (sides & Front) != 0 && y - (int)mid > BlockLOD32.VoxelSize - z) ||   // rounded Top-Front
                            ((sides & Front) != 0 && (sides & Bottom) != 0 && y + (int)mid < z) ||                  // rounded Front-Bottom
                            ((sides & Bottom) != 0 && (sides & Back) != 0 && y + (int)mid < BlockLOD32.VoxelSize - z) || // rounded Bottom-Back
                            ((sides & Back) != 0 && (sides & Top) != 0 && y - (int)mid > z))                        // rounded Back-Top
                        {
                            // Cylinder around the x-axis
                            float dist = (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Front) != 0 && x - (int)mid > BlockLOD32.VoxelSize - z) || // rounded Right-Front
                            ((sides & Front) != 0 && (sides & Left) != 0 && x + (int)mid < z) ||                    // rounded Front-Left
                            ((sides & Left) != 0 && (sides & Back) != 0 && x + (int)mid < BlockLOD32.VoxelSize - z) ||   // rounded Left-Back
                            ((sides & Back) != 0 && (sides & Right) != 0 && x - (int)mid > z))                      // rounded Back-Right
                        {
                            // Cylinder around the y-axis
                            float dist = (x - mid) * (x - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (((sides & Right) != 0 && (sides & Top) != 0 && x - (int)mid > BlockLOD32.VoxelSize - y) ||   // rounded Right-Top
                            ((sides & Top) != 0 && (sides & Left) != 0 && x + (int)mid < y) ||                      // rounded Top-Left
                            ((sides & Left) != 0 && (sides & Bottom) != 0 && x + (int)mid < BlockLOD32.VoxelSize - y) || // rounded Left-Bottom
                            ((sides & Bottom) != 0 && (sides & Right) != 0 && x - (int)mid > y))                    // rounded Bottom-Right
                        {
                            // Cylinder around the z-axis
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if ((((sides & Top) != 0 && (sides & Bottom) != 0 && (sides & Left) != 0 && x < mid) || // rounded Left
                            ((sides & Top) != 0 && (sides & Bottom) != 0 && (sides & Right) != 0 && x > mid) || // rounded Right
                            ((sides & Top) != 0 && (sides & Right) != 0 && (sides & Left) != 0 && y > mid) ||   // rounded Top
                            ((sides & Bottom) != 0 && (sides & Right) != 0 && (sides & Left) != 0 && y < mid))  // rounded Bottom
                            && (((sides & Front) != 0 && z > mid) || ((sides & Back) != 0 && z < mid)))         // on the front or back
                        {
                            // rounded front or back
                            float dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereSize * sphereSize)
                                continue;
                        }

                        if (random.NextFloat() < voxelDensity)
                            block.LOD32[x, y, z] = voxel;
                    }
                }
            }

            block.GenerateLODLevels();
            return block;
        }

        private static Block CreateBlockInfo(string name, float sphereRadius, Color4f color, float voxelDensity,
            ParticleComponent particleSystem = null, LightComponent light = null, int zStart = 0, int zLimit = BlockLOD32.VoxelSize, 
            float colorVariation = 0.2f)
        {
            const int mid = BlockLOD32.VoxelSize / 2;

            Block block = new Block(name);
            VoxelSettings settings = VoxelSettings.None;
            if (particleSystem != null)
                block.AddComponent(particleSystem);
            if (light == null)
            {
                if (colorVariation != 0.0f)
                    block.AddComponent(new VoxelNoiseComponent(colorVariation));
            }
            else
            {
                block.AddComponent(light);
                block.AddComponent(new LightPassthroughComponent());
                settings = VoxelSettings.AllowLightPassthrough | VoxelSettings.IgnoreLighting;
            }

            for (int x = 0; x < BlockLOD32.VoxelSize; x++)
            {
                for (int y = 0; y < BlockLOD32.VoxelSize; y++)
                {
                    for (int z = zStart; z < zLimit; z++)
                    {
                        if (sphereRadius > 0)
                        {
                            int dist = (x - mid) * (x - mid) + (y - mid) * (y - mid) + (z - mid) * (z - mid);
                            if (dist > sphereRadius * sphereRadius)
                                continue;
                        }

                        if (random.NextFloat() < voxelDensity)
                            block.LOD32[x, y, z] = new Voxel(color.R, color.G, color.B, color.A, settings);
                    }
                }
            }

            block.GenerateLODLevels();
            return block;
        }
    }
}
