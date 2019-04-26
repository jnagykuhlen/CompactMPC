using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class BitArrayTest
    {
        [TestMethod]
        public void TestBitArray()
        {
            BitArray array = BitArray.FromBinaryString("100101011100101");

            array[2] = Bit.One;
            array[5] = Bit.Zero;

            byte[] buffer = array.ToBytes();

            Assert.AreEqual(15, array.Length);
            Assert.AreEqual(2, buffer.Length);
            Assert.AreEqual("101100011100101", array.ToBinaryString());
            Assert.AreEqual((byte)Convert.ToInt32("10001101", 2), buffer[0]);
            Assert.AreEqual((byte)Convert.ToInt32("01010011", 2), buffer[1]);
        }

        [TestMethod]
        public void TestBitArrayOperations()
        {
            BitArray left = BitArray.FromBinaryString("1001");
            BitArray right = BitArray.FromBinaryString("1010");

            Assert.AreEqual("1011", (left | right).ToBinaryString());
            Assert.AreEqual("0011", (left ^ right).ToBinaryString());
            Assert.AreEqual("1000", (left & right).ToBinaryString());
            Assert.AreEqual("0110", (~left).ToBinaryString());
        }
    }
}
