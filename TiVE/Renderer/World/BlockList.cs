using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using JetBrains.Annotations;
using OpenTK;
using ProdigalSoftware.TiVE.Renderer.Voxels;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.TiVEPluginFramework.Lighting;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Renderer.World
{
    internal sealed class BlockList : IBlockList
    {
        public const string FileExtension = "TiVEb";

        private const string InfoFileHeader = "TiVEb";
        private const string BlockFileInternalInfoFile = "blocks.info";
        private const short FileVersion = 2;

        private readonly Dictionary<string, BlockInformation> blockToIndexMap = new Dictionary<string, BlockInformation>();
        private readonly List<AnimationInfo> animationsList = new List<AnimationInfo>();
        private readonly HashSet<BlockInformation> blocksBelongingToAnimations = new HashSet<BlockInformation>();
        private readonly Dictionary<BlockInformation, BlockInformation> blockAnimationMap = new Dictionary<BlockInformation, BlockInformation>();
        private readonly Dictionary<BlockInformation, VoxelGroup> blockMeshes = new Dictionary<BlockInformation, VoxelGroup>();

        public void Dispose()
        {
            foreach (VoxelGroup voxelGroup in blockMeshes.Values)
                voxelGroup.Dispose();
        }

        public IEnumerable<BlockInformation> AllBlocks
        {
            get { return blockToIndexMap.Values; }
        }

        public int BlockCount
        {
            get { return blockToIndexMap.Count; }
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
                            float atten = reader.ReadSingle();
                            block.Light = new PointLight(new Vector3b(locX, locY, locZ), new Color3f(colR, colG, colB), atten);
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
            foreach (BlockInformation block in blockToIndexMap.Values)
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

                            writer.Write(block.Light.Attenuation);
                        }
                    }
                    blockFile.AddEntry(block.BlockName + "." + FileExtension, memStream.ToArray());
                }
            }

            blockFile.Save(filePath);
        }

        public void AddBlocks(IEnumerable<BlockInformation> blocks)
        {
            foreach (BlockInformation block in blocks)
                AddBlock(block);
        }

        public void AddBlock(BlockInformation block)
        {
            blockToIndexMap[block.BlockName] = block;
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
                    animation.AnimationSequence.Select(blockName => !string.IsNullOrEmpty(blockName) ? blockToIndexMap[blockName] : BlockInformation.Empty));

                if (animationsList.Exists(ani => animationInfo.AnimationSequence.Any(bl => ani.AnimationSequence.Contains(bl))))
                    throw new InvalidOperationException("Block is already used as an animation and can not belong to two animations");

                blocksBelongingToAnimations.UnionWith(animationInfo.AnimationSequence);
                animationsList.Add(animationInfo);
            }
        }

        public bool BelongsToAnimation(BlockInformation block)
        {
            return blocksBelongingToAnimations.Contains(block);
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

        public RenderStatistics RenderAnimatedBlocks(GameWorld gameWorld, ShaderManager shaderManager, ref Matrix4 matrixMVP, 
            int camMinX, int camMaxX, int camMinY, int camMaxY)
        {
            Matrix4 translationMatrix = Matrix4.Identity;

            RenderStatistics stats = new RenderStatistics();
            for (int z = gameWorld.BlockSize.Z - 1; z >= 0; z--)
            {
                translationMatrix.M43 = z * BlockInformation.VoxelSize;
                for (int x = camMinX; x < camMaxX; x++)
                {
                    translationMatrix.M41 = x * BlockInformation.VoxelSize;
                    for (int y = camMinY; y < camMaxY; y++)
                    {
                        BlockInformation block = gameWorld[x, y, z];
                        if (!BelongsToAnimation(block))
                            continue;

                        BlockInformation newBlock = NextFrameFor(block);
                        if (newBlock != null)
                            gameWorld[x, y, z] = block = newBlock;

                        VoxelGroup voxelGroup;
                        if (!blockMeshes.TryGetValue(block, out voxelGroup))
                            blockMeshes[block] = voxelGroup = new VoxelGroup(block);

                        translationMatrix.M42 = y * BlockInformation.VoxelSize;
                        Matrix4 result;
                        Matrix4.Mult(ref translationMatrix, ref matrixMVP, out result);
                        stats += voxelGroup.Render(shaderManager, ref result);
                    }
                }
            }

            return stats;
        }

        private BlockInformation NextFrameFor(BlockInformation block)
        {
            BlockInformation nextBlock;
            blockAnimationMap.TryGetValue(block, out nextBlock);
            return nextBlock;
        }

        public BlockInformation this[string blockName]
        {
            get { return blockToIndexMap[blockName]; }
        }

        internal void UpdateNameIndex()
        {
            List<BlockInformation> blocks = blockToIndexMap.Values.ToList();
            blockToIndexMap.Clear();
            foreach (BlockInformation block in blocks)
                blockToIndexMap[block.BlockName] = block;
        }
    }
}
