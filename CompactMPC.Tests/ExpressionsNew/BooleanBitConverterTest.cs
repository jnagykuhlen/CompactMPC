using System.Collections.Generic;
using CompactMPC.Assertions;
using CompactMPC.ExpressionsNew.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew
{
    [TestClass]
    public class BooleanBitConverterTest
    {
        [TestMethod]
        public void TestFalseToBits()
        {
            IReadOnlyList<Bit> bits = BooleanBitConverter.Instance.ToBits(false, 1);
            EnumerableAssert.AreEquivalent(new[] { Bit.Zero }, bits);
        }
        
        [TestMethod]
        public void TestTrueToBits()
        {
            IReadOnlyList<Bit> bits = BooleanBitConverter.Instance.ToBits(true, 1);
            EnumerableAssert.AreEquivalent(new[] { Bit.One }, bits);
        }
        
        [TestMethod]
        public void TestFalseFromBits()
        {
            bool value = BooleanBitConverter.Instance.FromBits(new [] { Bit.Zero });
            Assert.IsFalse(value);
        }
        
        [TestMethod]
        public void TestTrueFromBits()
        {
            bool value = BooleanBitConverter.Instance.FromBits(new [] { Bit.One });
            Assert.IsTrue(value);
        }
    }
}
