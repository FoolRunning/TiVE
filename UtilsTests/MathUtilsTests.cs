using NUnit.Framework;
using ProdigalSoftware.Utils;

namespace UtilsTests
{
    /// <summary>
    /// Tests for the MathUtils class
    /// </summary>
    [TestFixture]
    public class MathUtilsTests
    {
        #region ClosestPow2 tests
        /// <summary>
        /// Tests the ClosestPow2 method with a value that is negative
        /// </summary>
        [Test]
        public void ClosestPow2_Negative()
        {
            Assert.That(MathUtils.ClosestPow2(-5), Is.EqualTo(1), "Value should be at least one");
        }

        /// <summary>
        /// Tests the ClosestPow2 method with a value of zero
        /// </summary>
        [Test]
        public void ClosestPow2_Zero()
        {
            Assert.That(MathUtils.ClosestPow2(0), Is.EqualTo(1), "Value should be at least one");
        }

        /// <summary>
        /// Tests the ClosestPow2 method with a value of one
        /// </summary>
        [Test]
        public void ClosestPow2_One()
        {
            Assert.That(MathUtils.ClosestPow2(1), Is.EqualTo(1), "One should be a valid power-of-two");
        }

        /// <summary>
        /// Tests the ClosestPow2 method with values that are already a power-of-two
        /// </summary>
        [Test]
        public void ClosestPow2_Equal()
        {
            Assert.That(MathUtils.ClosestPow2(2), Is.EqualTo(2), "Value is already a power-of-two");
            Assert.That(MathUtils.ClosestPow2(8), Is.EqualTo(8), "Value is already a power-of-two");
            Assert.That(MathUtils.ClosestPow2(128), Is.EqualTo(128), "Value is already a power-of-two");
            Assert.That(MathUtils.ClosestPow2(1024), Is.EqualTo(1024), "Value is already a power-of-two");
        }

        /// <summary>
        /// Tests the ClosestPow2 method to make sure the values are always rounded up
        /// </summary>
        [Test]
        public void ClosestPow2_RoundsUp()
        {
            Assert.That(MathUtils.ClosestPow2(3), Is.EqualTo(4), "Value should round upwards");
            Assert.That(MathUtils.ClosestPow2(5), Is.EqualTo(8), "Value should round upwards");
            Assert.That(MathUtils.ClosestPow2(7), Is.EqualTo(8), "Value should round upwards");
            Assert.That(MathUtils.ClosestPow2(513), Is.EqualTo(1024), "Value should round upwards");
            Assert.That(MathUtils.ClosestPow2(1000), Is.EqualTo(1024), "Value should round upwards");
        }

        /// <summary>
        /// Tests the ClosestPow2 method with a value that would overflow an int
        /// </summary>
        [Test]
        public void ClosestPow2_LargeValue()
        {
            const int largestPowOfTwo = (1 << 30);
            Assert.That(MathUtils.ClosestPow2(largestPowOfTwo - 1), Is.EqualTo(largestPowOfTwo), "Should handle up to 2^30 - 1");
            Assert.That(MathUtils.ClosestPow2(largestPowOfTwo), Is.EqualTo(largestPowOfTwo), "Should handle up to 2^30");
            Assert.That(() => MathUtils.ClosestPow2(largestPowOfTwo + 1), Throws.ArgumentException, "Should not allow values above 2^30");
            Assert.That(() => MathUtils.ClosestPow2(int.MaxValue), Throws.ArgumentException, "Should not allow values above 2^30");
        }
        #endregion
    }
}
