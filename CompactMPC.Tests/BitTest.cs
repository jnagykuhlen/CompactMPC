using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class BitTest
    {
        [TestMethod]
        public void TestBit()
        {
            Assert.IsFalse(Bit.Zero.IsSet);
            Assert.IsTrue(Bit.One.IsSet);
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
