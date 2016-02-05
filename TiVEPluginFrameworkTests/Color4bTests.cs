using NUnit.Framework;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Tests for the Color4b class
    /// </summary>
    [TestFixture]
    public class Color4bTests
    {
        /// <summary>
        /// Tests multiplying a Color4b by another Color4b
        /// </summary>
        [Test]
        public void Multiplication_Color4b()
        {
            Color4b c1 = new Color4b(0, 255, 100, 255);
            Color4b c2 = new Color4b(100, 20, 130, 50);

            Assert.That(c1 * c2, Is.EqualTo(new Color4b(0, 20, 50, 50)), "Multiplication of two Color4b was wrong");
        }

        /// <summary>
        /// Tests multiplying a Color4b by a float value
        /// </summary>
        [Test]
        public void Multiplication_Float()
        {
            Color4b c = new Color4b(130, 20, 255, 190);

            Assert.That(c * 0.5f, Is.EqualTo(new Color4b(65, 10, 127, 190)), "Multiplication of Color4b by float was wrong");
        }

        /// <summary>
        /// Tests adding a Color4b to another Color4b
        /// </summary>
        [Test]
        public void Addition_Color4b()
        {
            Color4b c1 = new Color4b(0, 255, 100, 25);
            Color4b c2 = new Color4b(100, 20, 130, 50);

            Assert.That(c1 + c2, Is.EqualTo(new Color4b(100, 255, 230, 75)), "Addition of two Color4b was wrong");
        }
    }
}
