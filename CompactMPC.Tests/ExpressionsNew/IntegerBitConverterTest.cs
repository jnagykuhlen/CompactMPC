using System.Collections.Generic;
using CompactMPC.ExpressionsNew.Internal;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew
{
    [TestClass]
    public class IntegerBitConverterTest
    {
        [TestMethod]
        public void TestToBits()
        {
            IReadOnlyList<Bit> bits = IntegerBitConverter.Instance.ToBits(81, 7);
            bits.Should().Equal(BitArray.FromBinaryString("1000101"));
        }
        
        [TestMethod]
        public void TestFromBits()
        {
            int value = IntegerBitConverter.Instance.FromBits(BitArray.FromBinaryString("1000101"));
            value.Should().Be(81);
        }
    }
}
