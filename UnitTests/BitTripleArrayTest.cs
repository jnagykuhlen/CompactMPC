using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class BitTripleArrayTest
    {
        [TestMethod]
        public void TestByteConversion()
        {
            BitTripleArray array = new BitTripleArray(new[]
            {
                new BitTriple(Bit.Zero, Bit.One, Bit.One),
                new BitTriple(Bit.One, Bit.One, Bit.Zero),
                new BitTriple(Bit.One, Bit.Zero, Bit.One)
            })
            {
                [1] = new BitTriple(Bit.Zero, Bit.One, Bit.Zero)
            };


            byte[] buffer = array.ToBytes();

            Assert.AreEqual(3, array.Length);
            Assert.AreEqual(2, buffer.Length);
            Assert.AreEqual((byte)Convert.ToInt32("01010110", 2), buffer[0]);
            Assert.AreEqual((byte)Convert.ToInt32("00000001", 2), buffer[1]);
        }

        [TestMethod]
        public void TestRequiredBytes()
        {
            Assert.AreEqual(0, BitTripleArray.RequiredBytes(0));
            Assert.AreEqual(1, BitTripleArray.RequiredBytes(1));
            Assert.AreEqual(1, BitTripleArray.RequiredBytes(2));
            Assert.AreEqual(2, BitTripleArray.RequiredBytes(3));
            Assert.AreEqual(2, BitTripleArray.RequiredBytes(4));
            Assert.AreEqual(3, BitTripleArray.RequiredBytes(8));
        }
    }
}