using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using JetBrains.Annotations;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class BlockList : IBlockList, IBlockListInternal
    {
        public const string FileExtension = "TiVEb";

        private const string InfoFileHeader = "TiVEb";
        private const string BlockFileInternalInfoFile = "blocks.info";
        private const short FileVersion = 3;

        private readonly List<BlockInformation> blocks = new List<BlockInformation>(500);
        private readonly Dictionary<string, ushort> blockToIndexMap = new Dictionary<string, ushort>();

        private readonly List<AnimationInfo> animationsList = new List<AnimationInfo>();
        private readonly Dictionary<ushort, ushort> blockAnimationMap = new Dictionary<ushort, ushort>();
        private readonly Dictionary<ushort, VoxelGroup> blockMeshes = new Dictionary<ushort, VoxelGroup>();

        public void Dispose()
        {
            foreach (VoxelGroup voxelGroup in blockMeshes.Values)
                voxelGroup.Dispose();

            blocks.Clear();
            blockToIndexMap.Clear();
            animationsList.Clear();
            blockAnimationMap.Clear();
            blockMeshes.Clear();
        }

        public IList<BlockInformation> AllBlocks
        {
            get { return blocks; }
        }

        public int BlockCount
        {
            get { return blocks.Count; }
        }

        public ushort this[string blockName]
        {
            get { return blockToIndexMap[blockName]; }
        }

        public BlockInformation this[int blockIndex]
        {
            get { return blocks[blockIndex]; }
        }

        [CanBeNull]
        public static BlockList FromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            using (ZipFile blockFile = new ZipFile(filePath))
            {
                if (blockFile.Count == 0)
                    return null; // File probably doesn't exist

                ZipEntry blockInfoEntry = blockFile[BlockFileInternalInfoFile];
                if (blockInfoEntry == null)
                    return null; // Zip file does not contain the block information file

                // Verify that this version supports the blocks specified in the block information file
                short fileVersion;
                using (BinaryReader reader = new BinaryReader(blockInfoEntry.OpenReader(), Encoding.ASCII))
                {
                    if (reader.ReadString() != InfoFileHeader)
                        return null;

                    fileVersion = reader.ReadInt16();
                    if (fileVersion > FileVersion)
                        return null;

                    if (reader.ReadByte() != BlockInformation.VoxelSize)
                        return null;
                }

                BlockList newBlockList = new BlockList();
                foreach (ZipEntry entry in blockFile.Where(bf => bf.FileName != BlockFileInternalInfoFile))
                {
                    string blockName = Path.GetFileNameWithoutExtension(entry.FileName);

                    using (BinaryReader reader = new BinaryReader(entry.OpenReader(), Encoding.ASCII))
                    {
                        BlockInformation block = new BlockInformation(blockName);
                        for (int i = 0; i < block.VoxelsArray.Length; i++)
                            block.VoxelsArray[i] = reader.ReadUInt32();
                        
                        if (fileVersion >= 2 && reader.ReadBoolean()) // Light added in version 2
                        {
                            // Block contains a light
                            byte locX = reader.ReadByte();
                            byte locY = reader.ReadByte();
                            byte locZ = reader.ReadByte();
                            float colR = reader.ReadSingle();
                            float colG = reader.ReadSingle();
                            float colB = reader.ReadSingle();
                            int maxBlocks;
                            if (fileVersion >= 3) // Version 3 changed attentuation to the max number of blocks
                                maxBlocks = reader.ReadInt16();
                            else
                                maxBlocks = (int)reader.ReadSingle();
                            block.Light = new PointLight(new Vector3b(locX, locY, locZ), new Color3f(colR, colG, colB), maxBlocks);
                        }
                        newBlockList.AddBlock(block);
                    }
                }

                return newBlockList;
            }
        }

        public void SaveToBlockListFile(string filePath)
        {
            ZipFile blockFile = new ZipFile();

            // Add entry for information about the blocks stored in the block file
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memStream, Encoding.ASCII))
                {
                    writer.Write(InfoFileHeader);
                    writer.Write(FileVersion);
                    writer.Write((byte)BlockInformation.VoxelSize);
                }
                blockFile.AddEntry(BlockFileInternalInfoFile, memStream.ToArray());
            }

            // Add entries for each block
            foreach (BlockInformation block in blocks.Where(b => b != BlockInformation.Empty))
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(memStream, Encoding.ASCII))
                    {
                        for (int i = 0; i < block.VoxelsArray.Length; i++)
                            writer.Write(block.VoxelsArray[i]);

                        // Write out the information about the light (added in version 2)
                        if (block.Light == null)
                            writer.Write(false);
                        else
                        {
                            writer.Write(true);
                            writer.Write(block.Light.Location.X);
                            writer.Write(block.Light.Location.Y);
                            writer.Write(block.Light.Location.Z);

                            writer.Write(block.Light.Color.R);
                            writer.Write(block.Light.Color.G);
                            writer.Write(block.Light.Color.B);

                            writer.Write((short)block.Light.LightBlockDist);
                        }
                    }
                    blockFile.AddEntry(block.BlockName + "." + FileExtension, memStream.ToArray());
                }
            }

            blockFile.Save(filePath);
        }

        public void AddBlocks(IEnumerable<BlockInformation> blocksToAdd)
        {
            foreach (BlockInformation block in blocksToAdd)
                AddBlock(block);
        }

        public void AddBlock(BlockInformation block)
        {
            if (blocks.Count == 0)
                blocks.Add(BlockInformation.Empty);

            ushort index = (ushort)blocks.Count;
            blocks.Add(block);
            blockToIndexMap[block.BlockName] = index;
        }

        public void RemoveBlock(string blockName)
        {
            blockToIndexMap.Remove(blockName);
        }

        public void AddAnimations(IEnumerable<BlockAnimationDefinition> animations)
        {
            if (animations == null)
                return;

            foreach (BlockAnimationDefinition animation in animations)
            {
                AnimationInfo animationInfo = new AnimationInfo(animation.AnimationFrameTime,
                    animation.AnimationSequence.Select(blockName => !string.IsNullOrEmpty(blockName) ? blockToIndexMap[blockName] : (ushort)0));

                if (animationsList.Exists(ani => animationInfo.AnimationSequence.Any(bl => ani.AnimationSequence.Contains(bl))))
                    throw new InvalidOperationException("Block is already used as an animation and can not belong to two animations");

                animationsList.Add(animationInfo);
            }
        }

        public void UpdateAnimations(float timeSinceLastUpdate)
        {
            blockAnimationMap.Clear();

            foreach (AnimationInfo animationInfo in animationsList)
            {
                animationInfo.TimeSinceLastFrame += timeSinceLastUpdate;
                if (animationInfo.TimeSinceLastFrame >= animationInfo.AnimationFrameTime)
                {
                    animationInfo.TimeSinceLastFrame -= animationInfo.AnimationFrameTime;
                    for (int blockIndex = 0; blockIndex < animationInfo.AnimationSequence.Length - 1; blockIndex++)
                        blockAnimationMap.Add(animationInfo.AnimationSequence[blockIndex], animationInfo.AnimationSequence[blockIndex + 1]);
                }
            }
        }

        //public RenderStatistics RenderAnimatedBlocks(GameWorld gameWorld, ShaderManager shaderManager, ref Matrix4 matrixMVP, 
        //    int camMinX, int camMaxX, int camMinY, int camMaxY)
        //{
        //    Matrix4 translationMatrix = Matrix4.Identity;

        //    RenderStatistics stats = new RenderStatistics();
        //    for (int z = gameWorld.BlockSize.Z - 1; z >= 0; z--)
        //    {
        //        translationMatrix.M43 = z * BlockInformation.VoxelSize;
        //        for (int x = camMinX; x < camMaxX; x++)
        //        {
        //            translationMatrix.M41 = x * BlockInformation.VoxelSize;
        //            for (int y = camMinY; y < camMaxY; y++)
        //            {
        //                BlockInformation block = gameWorld[x, y, z];
        //                if (block.NextBlock == null)
        //                    continue;

        //                BlockInformation nextBlock = block.NextBlock;
        //                if (nextBlock != null)
        //                    gameWorld[x, y, z] = block = nextBlock;

        //                VoxelGroup voxelGroup;
        //                if (!blockMeshes.TryGetValue(block, out voxelGroup))
        //                    blockMeshes[block] = voxelGroup = new VoxelGroup(block);

        //                translationMatrix.M42 = y * BlockInformation.VoxelSize;
        //                Matrix4 result;
        //                Matrix4.Mult(ref translationMatrix, ref matrixMVP, out result);
        //                stats += voxelGroup.Render(shaderManager, ref result);
        //            }
        //        }
        //    }

        //    return stats;
        //}

        internal void UpdateNameIndex()
        {
            List<ushort> blocksInIndex = blockToIndexMap.Values.ToList();
            blockToIndexMap.Clear();
            foreach (ushort blockIndex in blocksInIndex)
            {
                BlockInformation block = blocks[blockIndex];
                blockToIndexMap[block.BlockName] = blockIndex;
            }
        }
    }
}
