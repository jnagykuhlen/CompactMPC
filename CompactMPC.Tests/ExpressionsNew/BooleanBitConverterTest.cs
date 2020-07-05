using System.Collections.Generic;
using CompactMPC.ExpressionsNew.Internal;
using FluentAssertions;
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
            bits.Should().Equal(Bit.Zero);
        }
        
        [TestMethod]
        public void TestTrueToBits()
        {
            IReadOnlyList<Bit> bits = BooleanBitConverter.Instance.ToBits(true, 1);
            bits.Should().Equal(Bit.One);
        }
        
        [TestMethod]
        public void TestFalseFromBits()
        {
            bool value = BooleanBitConverter.Instance.FromBits(new [] { Bit.Zero });
            value.Should().BeFalse();
        }
        
        [TestMethod]
        public void TestTrueFromBits()
        {
            bool value = BooleanBitConverter.Instance.FromBits(new [] { Bit.One });
            value.Should().BeTrue();
        }
    }
}
