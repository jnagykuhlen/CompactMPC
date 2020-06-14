using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class QuadrupleIndexArrayTest
    {
        [TestMethod]
        public void TestByteConversion()
        {
            QuadrupleIndexArray indexArray = new QuadrupleIndexArray(new[] {3, 1, 2, 1, 3, 0, 1})
            {
                [2] = 0,
                [5] = 2
            };


            byte[] buffer = indexArray.ToBytes();

            Assert.AreEqual(7, indexArray.Length);
            Assert.AreEqual(2, buffer.Length);
            Assert.AreEqual((byte)Convert.ToInt32("01000111", 2), buffer[0]);
            Assert.AreEqual((byte)Convert.ToInt32("00011011", 2), buffer[1]);
        }

        [TestMethod]
        public void TestRequiredBytes()
        {
            Assert.AreEqual(0, QuadrupleIndexArray.RequiredBytes(0));
            Assert.AreEqual(1, QuadrupleIndexArray.RequiredBytes(1));
            Assert.AreEqual(1, QuadrupleIndexArray.RequiredBytes(3));
            Assert.AreEqual(1, QuadrupleIndexArray.RequiredBytes(4));
            Assert.AreEqual(2, QuadrupleIndexArray.RequiredBytes(5));
        }
    }
}
