using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class BitTest
    {
        [TestMethod]
        public void TestBit()
        {
            Assert.IsFalse(Bit.Zero.Value);
            Assert.IsTrue(Bit.One.Value);
            Assert.AreEqual(new Bit(), Bit.Zero);
            Assert.AreEqual(new Bit(false), Bit.Zero);
            Assert.AreEqual(new Bit(true), Bit.One);
            Assert.AreEqual(new Bit(0), Bit.Zero);
            Assert.AreEqual(new Bit(1), Bit.One);
            Assert.AreEqual(new Bit(2), Bit.Zero);
            Assert.AreEqual(new Bit(3), Bit.One);
        }
    }
}
