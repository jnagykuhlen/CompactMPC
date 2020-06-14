using System.Collections.Generic;
using CompactMPC.ExpressionsNew;
using CompactMPC.ExpressionsNew.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests.ExpressionsNew
{
    [TestClass]
    public class IntegerBitConverterTest
    {
        [TestMethod]
        public void TestToBits()
        {
            IReadOnlyList<Bit> bits = IntegerBitConverter.Instance.ToBits(81, 7);
            Assert.AreEqual("1000101", new BitArray(bits).ToBinaryString());
        }
        
        [TestMethod]
        public void TestFromBits()
        {
            int value = IntegerBitConverter.Instance.FromBits(BitArray.FromBinaryString("1000101"));
            Assert.AreEqual(81, value);
        }
    }
}
