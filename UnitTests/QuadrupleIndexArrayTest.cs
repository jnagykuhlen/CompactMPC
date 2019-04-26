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
    public class QuadrupleIndexArrayTest
    {
        [TestMethod]
        public void TestQuadrupleIndexArray()
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
    }
}
