using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class QuadrupleIndexArrayTest
    {
        [TestMethod]
        public void TestByteConversion()
        {
            QuadrupleIndexArray array = new QuadrupleIndexArray(new int[] { 3, 1, 2, 1, 3, 0, 1 });

            array[2] = 0;
            array[5] = 2;

            byte[] buffer = array.ToBytes();

            Assert.AreEqual(7, array.Length);
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
