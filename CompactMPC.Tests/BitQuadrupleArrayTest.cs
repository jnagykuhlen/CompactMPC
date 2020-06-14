using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class BitQuadrupleArrayTest
    {
        [TestMethod]
        public void TestByteConversion()
        {
            BitQuadrupleArray array = new BitQuadrupleArray(new[]
            {
                new BitQuadruple(Bit.Zero, Bit.One, Bit.One, Bit.Zero),
                new BitQuadruple(Bit.One, Bit.One, Bit.Zero, Bit.One),
                new BitQuadruple(Bit.Zero, Bit.One, Bit.Zero, Bit.One)
            })
            {
                [1] = new BitQuadruple(Bit.One, Bit.Zero, Bit.One, Bit.One)
            };


            byte[] buffer = array.ToBytes();

            Assert.AreEqual(3, array.Length);
            Assert.AreEqual(2, buffer.Length);
            Assert.AreEqual((byte)Convert.ToInt32("11010110", 2), buffer[0]);
            Assert.AreEqual((byte)Convert.ToInt32("00001010", 2), buffer[1]);
        }

        [TestMethod]
        public void TestRequiredBytes()
        {
            Assert.AreEqual(0, BitQuadrupleArray.RequiredBytes(0));
            Assert.AreEqual(1, BitQuadrupleArray.RequiredBytes(1));
            Assert.AreEqual(1, BitQuadrupleArray.RequiredBytes(2));
            Assert.AreEqual(2, BitQuadrupleArray.RequiredBytes(3));
        }
    }
}
