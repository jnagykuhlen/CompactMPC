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
    public class BitQuadrupleArrayTest
    {
        [TestMethod]
        public void TestBitQuadrupleArray()
        {
            BitQuadrupleArray array = new BitQuadrupleArray(new[] {
                new BitQuadruple(Bit.Zero, Bit.One, Bit.One, Bit.Zero),
                new BitQuadruple(Bit.One, Bit.One, Bit.Zero, Bit.One),
                new BitQuadruple(Bit.Zero, Bit.One, Bit.Zero, Bit.One)
            });

            array[1] = new BitQuadruple(Bit.One, Bit.Zero, Bit.One, Bit.One);

            byte[] buffer = array.ToBytes();

            Assert.AreEqual(3, array.Length);
            Assert.AreEqual(2, buffer.Length);
            Assert.AreEqual((byte)Convert.ToInt32("11010110", 2), buffer[0]);
            Assert.AreEqual((byte)Convert.ToInt32("00001010", 2), buffer[1]);
        }
    }
}
