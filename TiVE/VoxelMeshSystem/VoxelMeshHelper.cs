using System;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.VoxelMeshSystem
{
    [Flags]
    internal enum VoxelSides : byte
    {
        None = 0,
        Top = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,
        Front = 1 << 4,
        Back = 1 << 5,
        Unknown = 1 << 7,
        All = Top | Left | Right | Bottom | Front | Back,
    }

    internal abstract class VoxelMeshHelper
    {
        private const int SmallColorDiff = 10;
        private const int BigColorDiff = 20;

        private static readonly VoxelMeshHelper cubifyNonInstancedGeom = new CubifyNonInstancedGeomVoxelMeshHelper();
        private static readonly VoxelMeshHelper cubifyInstanced = new CubifyInstancedVoxelMeshHelper();
        private static readonly VoxelMeshHelper nonCubifyNonInstancedGeom = new NonCubifyNonInstancedGeomVoxelMeshHelper();
        private static readonly VoxelMeshHelper nonCubifyInstanced = new NonCubifyInstancedVoxelMeshHelper();
        private static bool cubifyVoxels;

        static VoxelMeshHelper()
        {
            cubifyVoxels = TiVEController.UserSettings.Get(UserSettings.CubifyVoxelsKey);
            TiVEController.UserSettings.SettingChanged += UserSettings_SettingChanged;
        }

        static void UserSettings_SettingChanged(string settingName, Setting newValue)
        {
            if (settingName == UserSettings.CubifyVoxelsKey)
                cubifyVoxels = newValue;
        }

        /// <summary>
        /// Gets a voxel helper for the specified settings
        /// </summary>
        public static VoxelMeshHelper Get(bool forInstances)
        {
            if (cubifyVoxels)
                return forInstances ? cubifyInstanced : cubifyNonInstancedGeom;

            return forInstances ? nonCubifyInstanced : nonCubifyNonInstancedGeom;
        }

        public abstract string ShaderName { get; }

        public abstract int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize);

        public abstract int AddVoxel2(MeshBuilder2 meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize);

        #region NonCubifyInstancedVoxelMeshHelper class
        private sealed class NonCubifyInstancedVoxelMeshHelper : VoxelMeshHelper
        {
            public override string ShaderName
            {
                get { return "NonShadedInstanced"; }
            }

            public override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize)
            {
                int polygonCount = 0;
                byte x2 = (byte)(x + voxelSize);
                byte y2 = (byte)(y + voxelSize);
                byte z2 = (byte)(z + voxelSize);
                if ((sides & VoxelSides.Front) != 0)
                {
                    meshBuilder.Add(x, y, z2, color);
                    meshBuilder.Add(x2, y2, z2, color);
                    meshBuilder.Add(x2, y, z2, color);

                    meshBuilder.Add(x2, y2, z2, color);
                    meshBuilder.Add(x, y, z2, color);
                    meshBuilder.Add(x, y2, z2, color);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Back) != 0)
                {
                    meshBuilder.Add(x, y, z, color);
                    meshBuilder.Add(x2, y, z, color);
                    meshBuilder.Add(x2, y2, z, color);

                    meshBuilder.Add(x2, y2, z, color);
                    meshBuilder.Add(x, y2, z, color);
                    meshBuilder.Add(x, y, z, color);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Left) != 0)
                {
                    meshBuilder.Add(x, y, z, color);
                    meshBuilder.Add(x, y2, z, color);
                    meshBuilder.Add(x, y2, z2, color);

                    meshBuilder.Add(x, y2, z2, color);
                    meshBuilder.Add(x, y, z2, color);
                    meshBuilder.Add(x, y, z, color);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Right) != 0)
                {
                    meshBuilder.Add(x2, y, z, color);
                    meshBuilder.Add(x2, y2, z2, color);
                    meshBuilder.Add(x2, y2, z, color);

                    meshBuilder.Add(x2, y2, z2, color);
                    meshBuilder.Add(x2, y, z, color);
                    meshBuilder.Add(x2, y, z2, color);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Bottom) != 0)
                {
                    meshBuilder.Add(x, y, z, color);
                    meshBuilder.Add(x2, y, z2, color);
                    meshBuilder.Add(x2, y, z, color);

                    meshBuilder.Add(x, y, z, color);
                    meshBuilder.Add(x, y, z2, color);
                    meshBuilder.Add(x2, y, z2, color);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Top) != 0)
                {
                    meshBuilder.Add(x, y2, z, color);
                    meshBuilder.Add(x2, y2, z, color);
                    meshBuilder.Add(x2, y2, z2, color);

                    meshBuilder.Add(x, y2, z, color);
                    meshBuilder.Add(x2, y2, z2, color);
                    meshBuilder.Add(x, y2, z2, color);
                    polygonCount += 2;
                }

                return polygonCount;
            }

            public override int AddVoxel2(MeshBuilder2 meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize)
            {
                return 0;
            }
        }
        #endregion

        #region CubifyInstancedVoxelMeshHelper class
        private sealed class CubifyInstancedVoxelMeshHelper : VoxelMeshHelper
        {
            public override string ShaderName
            {
                get { return "ShadedInstanced"; }
            }

            public override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize)
            {
                int polygonCount = 0;
                byte x2 = (byte)(x + voxelSize);
                byte y2 = (byte)(y + voxelSize);
                byte z2 = (byte)(z + voxelSize);
                if ((sides & VoxelSides.Front) != 0)
                {
                    meshBuilder.Add(x, y, z2, color);
                    meshBuilder.Add(x2, y2, z2, color);
                    meshBuilder.Add(x2, y, z2, color);

                    meshBuilder.Add(x2, y2, z2, color);
                    meshBuilder.Add(x, y, z2, color);
                    meshBuilder.Add(x, y2, z2, color);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Back) != 0)
                {
                    meshBuilder.Add(x, y, z, color);
                    meshBuilder.Add(x2, y, z, color);
                    meshBuilder.Add(x2, y2, z, color);

                    meshBuilder.Add(x2, y2, z, color);
                    meshBuilder.Add(x, y2, z, color);
                    meshBuilder.Add(x, y, z, color);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Left) != 0)
                {
                    Color4b colorLeft = color + SmallColorDiff;

                    meshBuilder.Add(x, y, z, colorLeft);
                    meshBuilder.Add(x, y2, z, colorLeft);
                    meshBuilder.Add(x, y2, z2, colorLeft);

                    meshBuilder.Add(x, y2, z2, colorLeft);
                    meshBuilder.Add(x, y, z2, colorLeft);
                    meshBuilder.Add(x, y, z, colorLeft);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Right) != 0)
                {
                    Color4b colorRight = color - SmallColorDiff;

                    meshBuilder.Add(x2, y, z, colorRight);
                    meshBuilder.Add(x2, y2, z2, colorRight);
                    meshBuilder.Add(x2, y2, z, colorRight);

                    meshBuilder.Add(x2, y2, z2, colorRight);
                    meshBuilder.Add(x2, y, z, colorRight);
                    meshBuilder.Add(x2, y, z2, colorRight);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Bottom) != 0)
                {
                    Color4b colorBottom = color - BigColorDiff;

                    meshBuilder.Add(x, y, z, colorBottom);
                    meshBuilder.Add(x2, y, z2, colorBottom);
                    meshBuilder.Add(x2, y, z, colorBottom);

                    meshBuilder.Add(x, y, z, colorBottom);
                    meshBuilder.Add(x, y, z2, colorBottom);
                    meshBuilder.Add(x2, y, z2, colorBottom);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Top) != 0)
                {
                    Color4b colorTop = color + BigColorDiff;

                    meshBuilder.Add(x, y2, z, colorTop);
                    meshBuilder.Add(x2, y2, z, colorTop);
                    meshBuilder.Add(x2, y2, z2, colorTop);

                    meshBuilder.Add(x, y2, z, colorTop);
                    meshBuilder.Add(x2, y2, z2, colorTop);
                    meshBuilder.Add(x, y2, z2, colorTop);
                    polygonCount += 2;
                }

                return polygonCount;
            }

            public override int AddVoxel2(MeshBuilder2 meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize)
            {
                return 0;
            }
        }
        #endregion

        #region NonCubifyNonInstancedGeomVoxelMeshHelper class
        private sealed class NonCubifyNonInstancedGeomVoxelMeshHelper : VoxelMeshHelper
        {
            public override string ShaderName
            {
                get { return "NonShadedNonInstancedGeom"; }
            }

            public override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize)
            {
                return 0;
            }

            public override int AddVoxel2(MeshBuilder2 meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize)
            {
                meshBuilder.Add(x, y, z, (byte)sides, color);

                int polygonCount = 0;
                if ((sides & VoxelSides.Front) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Back) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Left) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Right) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Bottom) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Top) != 0)
                    polygonCount += 2;

                return polygonCount;
            }
        }
        #endregion

        #region NonCubifyNonInstancedGeomVoxelMeshHelper class
        private sealed class CubifyNonInstancedGeomVoxelMeshHelper : VoxelMeshHelper
        {
            public override string ShaderName
            {
                get { return "ShadedNonInstancedGeom"; }
            }

            public override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize)
            {
                return 0;
            }

            public override int AddVoxel2(MeshBuilder2 meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize)
            {
                meshBuilder.Add(x, y, z, (byte)sides, color);

                int polygonCount = 0;
                if ((sides & VoxelSides.Front) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Back) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Left) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Right) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Bottom) != 0)
                    polygonCount += 2;

                if ((sides & VoxelSides.Top) != 0)
                    polygonCount += 2;

                return polygonCount;
            }
        }
        #endregion
    }
}
