using NUnit.Framework;
using ProdigalSoftware.Utils;

namespace UtilsTests
{
    /// <summary>
    /// Tests for the ArrayUtils class
    /// </summary>
    [TestFixture]
    public class ArrayUtilsTests
    {
        #region ResizeArray tests
        /// <summary>
        /// Tests the ResizeArray method with arrays of value types
        /// </summary>
        [Test]
        public void ResizeArray_ValueArray()
        {
            int[] array = new int[99];
            for (int i = 0; i < array.Length; i++)
                array[i] = i * 5;

            ArrayUtils.ResizeArray(ref array);
            Assert.That(array.Length, Is.EqualTo(165), "Array had the wrong length");

            for (int i = 0; i < 99; i++)
                Assert.That(array[i], Is.EqualTo(i * 5), "Array did not contain the correct value at index " + i);

            for (int i = 100; i < array.Length; i++)
                Assert.That(array[i], Is.EqualTo(0), "Array did not contain the correct value at index " + i);
        }

        /// <summary>
        /// Tests the ResizeArray method with arrays of reference types
        /// </summary>
        [Test]
        public void ResizeArray_ReferenceArray()
        {
            TestObj[] array = new TestObj[99];
            for (int i = 0; i < array.Length; i++)
                array[i] = new TestObj(i * 0.5f);

            ArrayUtils.ResizeArray(ref array);
            Assert.That(array.Length, Is.EqualTo(165), "Array had the wrong length");

            for (int i = 0; i < 99; i++)
            {
                Assert.That(array[i], Is.Not.Null, "Array did not contain an object at index " + i);
                Assert.That(array[i].Value, Is.EqualTo(i * 0.5f), "Array did not contain the correct value at index " + i);
            }

            for (int i = 100; i < array.Length; i++)
                Assert.That(array[i], Is.Null, "Array did not contain the correct value at index " + i);
        }

        /// <summary>
        /// Tests the ResizeArray method with an array that is really small
        /// </summary>
        [Test]
        public void ResizeArray_TinyArray()
        {
            int[] array = { 73, 181 };

            ArrayUtils.ResizeArray(ref array);
            Assert.That(array.Length, Is.EqualTo(3), "Array did not increase in length");
            Assert.That(array[0], Is.EqualTo(73), "Array did not contain the correct value at index 0");
            Assert.That(array[1], Is.EqualTo(181), "Array did not contain the correct value at index 1");
            Assert.That(array[2], Is.EqualTo(0), "Array did not contain the correct value at index 2");
        }

        /// <summary>
        /// Tests the ResizeArray method with an array that is only one item long
        /// </summary>
        [Test]
        public void ResizeArray_VeryTinyArray()
        {
            int[] array = { 73 };

            ArrayUtils.ResizeArray(ref array);
            Assert.That(array.Length, Is.EqualTo(2), "Array did not increase in length");
            Assert.That(array[0], Is.EqualTo(73), "Array did not contain the correct value at index 0");
            Assert.That(array[1], Is.EqualTo(0), "Array did not contain the correct value at index 1");
        }
        #endregion

        #region TestObj class
        /// <summary>
        /// Small class for testing reference types
        /// </summary>
        private sealed class TestObj
        {
            /// <summary>
            /// The value that this object holds
            /// </summary>
            public readonly float Value;

            /// <summary>
            /// Creates a new TestObj with the specified value
            /// </summary>
            public TestObj(float value)
            {
                Value = value;
            }
        }
        #endregion
    }
}
