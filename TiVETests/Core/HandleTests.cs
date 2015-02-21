using NUnit.Framework;
using ProdigalSoftware.TiVE.Core;

namespace TiVETests.Core
{
    /// <summary>
    /// Tests for the Handle structure
    /// </summary>
    [TestFixture]
    public class HandleTests
    {
        #region Create tests
        [Test]
        public void Create()
        {
            Handle handle = new Handle(289, 22, 12589);
            Assert.AreEqual(289, handle.Type);
            Assert.AreEqual(22, handle.Counter);
            Assert.AreEqual(12589, handle.Index);
        }

        [Test]
        public void Create_MaxedType()
        {
            Handle handle = new Handle(1023, 2, 35);
            Assert.AreEqual(1023, handle.Type);
            Assert.AreEqual(2, handle.Counter);
            Assert.AreEqual(35, handle.Index);
        }

        [Test]
        public void Create_MaxedCounter()
        {
            Handle handle = new Handle(3, 31, 35);
            Assert.AreEqual(3, handle.Type);
            Assert.AreEqual(31, handle.Counter);
            Assert.AreEqual(35, handle.Index);
        }

        [Test]
        public void Create_MaxedIndex()
        {
            Handle handle = new Handle(3, 1, 65535);
            Assert.AreEqual(3, handle.Type);
            Assert.AreEqual(1, handle.Counter);
            Assert.AreEqual(65535, handle.Index);
        }

        [Test]
        public void Create_MinValues()
        {
            Handle handle = new Handle(0, 0, 0);
            Assert.AreEqual(0, handle.Type);
            Assert.AreEqual(0, handle.Counter);
            Assert.AreEqual(0, handle.Index);
        }
        #endregion

        #region CastToFromInt tests
        [Test]
        public void CastToFromInt()
        {
            Handle handle = new Handle(289, 22, 12589);
            int handleAsInt = handle;

            Handle result = handleAsInt;
            Assert.AreEqual(289, result.Type);
            Assert.AreEqual(22, result.Counter);
            Assert.AreEqual(12589, result.Index);
        }
        #endregion

        #region IncrementCounter tests
        [Test]
        public void IncrementCounter_MaxedType()
        {
            Handle handle = new Handle(1023, 7, 5);
            Handle newHandle = handle.IncrementCounter();
            Assert.AreEqual(1023, newHandle.Type);
            Assert.AreEqual(8, newHandle.Counter);
            Assert.AreEqual(5, newHandle.Index);
        }

        [Test]
        public void IncrementCounter_MaxedIndex()
        {
            Handle handle = new Handle(3, 17, 65535);
            Handle newHandle = handle.IncrementCounter();
            Assert.AreEqual(3, newHandle.Type);
            Assert.AreEqual(18, newHandle.Counter);
            Assert.AreEqual(65535, newHandle.Index);
        }

        [Test]
        public void IncrementCounter_OverCounterMax()
        {
            Handle handle = new Handle(3, 30, 35);
            Handle newHandle = handle.IncrementCounter().IncrementCounter().IncrementCounter();
            Assert.AreEqual(3, newHandle.Type);
            Assert.AreEqual(1, newHandle.Counter);
            Assert.AreEqual(35, newHandle.Index);
        }
        #endregion
    }
}
