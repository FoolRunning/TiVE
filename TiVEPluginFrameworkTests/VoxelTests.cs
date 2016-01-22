using NUnit.Framework;

namespace ProdigalSoftware.TiVEPluginFramework
{
    [TestFixture]
    public class VoxelTests
    {
        [Test]
        public void FromRGBA()
        {
            VerifyVoxel((Voxel)0xFAFBFCFF, 0xFA, 0xFB, 0xFC, 0xFF);
            VerifyVoxel((Voxel)0xFAFBFCFE, 0xFA, 0xFB, 0xFC, 0xFF);
            VerifyVoxel((Voxel)0x0A0B0C1A, 0x0A, 0x0B, 0x0C, 0x1F);
            VerifyVoxel((Voxel)0x0A0B0C11, 0x0A, 0x0B, 0x0C, 0x1F);
            VerifyVoxel((Voxel)0xFAFBFC01, 0xFA, 0xFB, 0xFC, 0x0F);
            VerifyVoxel((Voxel)0xFAFBFC00, 0xFA, 0xFB, 0xFC, 0x0F);
        }

        [Test]
        public void FromBytes()
        {
            VerifyVoxel(new Voxel(0xFA, 0xFB, 0xFC), 0xFA, 0xFB, 0xFC, 0xFF);
            VerifyVoxel(new Voxel(0xFA, 0xFB, 0xFC, 0xFE), 0xFA, 0xFB, 0xFC, 0xFF);
            VerifyVoxel(new Voxel(0x0A, 0x0B, 0x0C, 0x1A), 0x0A, 0x0B, 0x0C, 0x1F);
            VerifyVoxel(new Voxel(0x0A, 0x0B, 0x0C, 0x11), 0x0A, 0x0B, 0x0C, 0x1F);
            VerifyVoxel(new Voxel(0xFA, 0xFB, 0xFC, 0x01), 0xFA, 0xFB, 0xFC, 0x0F);
            VerifyVoxel(new Voxel(0xFA, 0xFB, 0xFC, 0x00), 0xFA, 0xFB, 0xFC, 0x0F);
        }

        private static void VerifyVoxel(Voxel voxel, byte expectedR, byte expectedG, byte expectedB, byte expectedA)
        {
            Assert.That(voxel.R, Is.EqualTo(expectedR), "Wrong value for red");
            Assert.That(voxel.G, Is.EqualTo(expectedG), "Wrong value for green");
            Assert.That(voxel.B, Is.EqualTo(expectedB), "Wrong value for blue");
            Assert.That(voxel.A, Is.EqualTo(expectedA), "Wrong value for alpha");
        }
    }
}
