using ProdigalSoftware.TiVE.RenderSystem.Meshes;
using ProdigalSoftware.TiVE.Settings;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.RenderSystem.Voxels
{
    internal abstract class VoxelMeshHelper
    {
        private const int SmallColorDiff = 7;
        private const int BigColorDiff = 14;

        private static readonly VoxelMeshHelper cubifyNonInstanced = new CubifyNonInstancedVoxelMeshHelper();
        private static readonly VoxelMeshHelper cubifyInstanced = new CubifyInstancedVoxelMeshHelper();
        private static readonly VoxelMeshHelper nonCubifyNonInstanced = new NonCubifyNonInstancedVoxelMeshHelper();
        private static readonly VoxelMeshHelper nonCubifyInstanced = new NonCubifyInstancedVoxelMeshHelper();
        private static bool cubifyVoxels;

        static VoxelMeshHelper()
        {
            cubifyVoxels = TiVEController.UserSettings.Get(UserSettings.CubifyVoxels);
            TiVEController.UserSettings.SettingChanged += UserSettings_SettingChanged;
        }

        static void UserSettings_SettingChanged(string settingName)
        {
            if (settingName == UserSettings.CubifyVoxels)
                cubifyVoxels = TiVEController.UserSettings.Get(UserSettings.CubifyVoxels);
        }

        /// <summary>
        /// Gets a voxel helper for the specified settings
        /// </summary>
        public static VoxelMeshHelper Get(bool forInstances)
        {
            if (cubifyVoxels)
                return forInstances ? cubifyInstanced : cubifyNonInstanced;

            return forInstances ? nonCubifyInstanced : nonCubifyNonInstanced;
        }

        public abstract string ShaderName { get; }

        public abstract int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize = 1);

        #region NonCubifyNonInstancedVoxelMeshHelper class
        private sealed class NonCubifyNonInstancedVoxelMeshHelper : VoxelMeshHelper
        {
            public override string ShaderName
            {
                get { return "NonShadedNonInstanced"; }
            }

            public override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize = 1)
            {
                byte x2 = (byte)(x + voxelSize);
                byte y2 = (byte)(y + voxelSize);
                byte z2 = (byte)(z + voxelSize);
                int v1 = meshBuilder.Add(x, y2, z, color);
                int v2 = meshBuilder.Add(x2, y2, z, color);
                int v3 = meshBuilder.Add(x2, y2, z2, color);
                int v4 = meshBuilder.Add(x, y2, z2, color);
                int v5 = meshBuilder.Add(x, y, z, color);
                int v6 = meshBuilder.Add(x2, y, z, color);
                int v7 = meshBuilder.Add(x2, y, z2, color);
                int v8 = meshBuilder.Add(x, y, z2, color);

                int polygonCount = 0;
                if ((sides & VoxelSides.Front) != 0)
                {
                    meshBuilder.AddIndex(v8);
                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v7);

                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v8);
                    meshBuilder.AddIndex(v4);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Back) != 0)
                {
                    meshBuilder.AddIndex(v5);
                    meshBuilder.AddIndex(v6);
                    meshBuilder.AddIndex(v2);

                    meshBuilder.AddIndex(v2);
                    meshBuilder.AddIndex(v1);
                    meshBuilder.AddIndex(v5);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Left) != 0)
                {
                    meshBuilder.AddIndex(v5);
                    meshBuilder.AddIndex(v1);
                    meshBuilder.AddIndex(v4);

                    meshBuilder.AddIndex(v4);
                    meshBuilder.AddIndex(v8);
                    meshBuilder.AddIndex(v5);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Right) != 0)
                {
                    meshBuilder.AddIndex(v6);
                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v2);

                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v6);
                    meshBuilder.AddIndex(v7);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Bottom) != 0)
                {
                    meshBuilder.AddIndex(v5);
                    meshBuilder.AddIndex(v7);
                    meshBuilder.AddIndex(v6);

                    meshBuilder.AddIndex(v5);
                    meshBuilder.AddIndex(v8);
                    meshBuilder.AddIndex(v7);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Top) != 0)
                {
                    meshBuilder.AddIndex(v1);
                    meshBuilder.AddIndex(v2);
                    meshBuilder.AddIndex(v3);

                    meshBuilder.AddIndex(v1);
                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v4);
                    polygonCount += 2;
                }

                return polygonCount;
            }
        }
        #endregion

        #region NonCubifyInstancedVoxelMeshHelper class
        private sealed class NonCubifyInstancedVoxelMeshHelper : VoxelMeshHelper
        {
            public override string ShaderName
            {
                get { return "NonShadedInstanced"; }
            }

            public override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize = 1)
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
        }
        #endregion

        #region CubifyNonInstancedVoxelMeshHelper class
        private sealed class CubifyNonInstancedVoxelMeshHelper : VoxelMeshHelper
        {
            public override string ShaderName
            {
                get { return "ShadedNonInstanced"; }
            }

            public override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize = 1)
            {
                int polygonCount = 0;
                byte x2 = (byte)(x + voxelSize);
                byte y2 = (byte)(y + voxelSize);
                byte z2 = (byte)(z + voxelSize);
                if ((sides & VoxelSides.Front) != 0)
                {
                    int v3 = meshBuilder.Add(x2, y2, z2, color);
                    int v4 = meshBuilder.Add(x, y2, z2, color);
                    int v7 = meshBuilder.Add(x2, y, z2, color);
                    int v8 = meshBuilder.Add(x, y, z2, color);

                    meshBuilder.AddIndex(v8);
                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v7);

                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v8);
                    meshBuilder.AddIndex(v4);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Back) != 0)
                {
                    int v1 = meshBuilder.Add(x, y2, z, color);
                    int v2 = meshBuilder.Add(x2, y2, z, color);
                    int v5 = meshBuilder.Add(x, y, z, color);
                    int v6 = meshBuilder.Add(x2, y, z, color);

                    meshBuilder.AddIndex(v5);
                    meshBuilder.AddIndex(v6);
                    meshBuilder.AddIndex(v2);

                    meshBuilder.AddIndex(v2);
                    meshBuilder.AddIndex(v1);
                    meshBuilder.AddIndex(v5);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Left) != 0)
                {
                    Color4b colorLeft = color + SmallColorDiff;
                    int v1 = meshBuilder.Add(x, y2, z, colorLeft);
                    int v4 = meshBuilder.Add(x, y2, z2, colorLeft);
                    int v5 = meshBuilder.Add(x, y, z, colorLeft);
                    int v8 = meshBuilder.Add(x, y, z2, colorLeft);

                    meshBuilder.AddIndex(v5);
                    meshBuilder.AddIndex(v1);
                    meshBuilder.AddIndex(v4);

                    meshBuilder.AddIndex(v4);
                    meshBuilder.AddIndex(v8);
                    meshBuilder.AddIndex(v5);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Right) != 0)
                {
                    Color4b colorRight = color - SmallColorDiff;
                    int v2 = meshBuilder.Add(x2, y2, z, colorRight);
                    int v3 = meshBuilder.Add(x2, y2, z2, colorRight);
                    int v6 = meshBuilder.Add(x2, y, z, colorRight);
                    int v7 = meshBuilder.Add(x2, y, z2, colorRight);

                    meshBuilder.AddIndex(v6);
                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v2);

                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v6);
                    meshBuilder.AddIndex(v7);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Bottom) != 0)
                {
                    Color4b colorBottom = color - BigColorDiff;
                    int v5 = meshBuilder.Add(x, y, z, colorBottom);
                    int v6 = meshBuilder.Add(x2, y, z, colorBottom);
                    int v7 = meshBuilder.Add(x2, y, z2, colorBottom);
                    int v8 = meshBuilder.Add(x, y, z2, colorBottom);

                    meshBuilder.AddIndex(v5);
                    meshBuilder.AddIndex(v7);
                    meshBuilder.AddIndex(v6);

                    meshBuilder.AddIndex(v5);
                    meshBuilder.AddIndex(v8);
                    meshBuilder.AddIndex(v7);
                    polygonCount += 2;
                }

                if ((sides & VoxelSides.Top) != 0)
                {
                    Color4b colorTop = color + BigColorDiff;
                    int v1 = meshBuilder.Add(x, y2, z, colorTop);
                    int v2 = meshBuilder.Add(x2, y2, z, colorTop);
                    int v3 = meshBuilder.Add(x2, y2, z2, colorTop);
                    int v4 = meshBuilder.Add(x, y2, z2, colorTop);

                    meshBuilder.AddIndex(v1);
                    meshBuilder.AddIndex(v2);
                    meshBuilder.AddIndex(v3);

                    meshBuilder.AddIndex(v1);
                    meshBuilder.AddIndex(v3);
                    meshBuilder.AddIndex(v4);
                    polygonCount += 2;
                }

                return polygonCount;
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

            public override int AddVoxel(MeshBuilder meshBuilder, VoxelSides sides, byte x, byte y, byte z, Color4b color, int voxelSize = 1)
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
        }
        #endregion
    }
}
