using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class BitTest
    {
        [TestMethod]
        public void TestBitIsSet()
        {
            Bit.Zero.IsSet.Should().BeFalse();
            Bit.One.IsSet.Should().BeTrue();
        }
        
        [TestMethod]
        public void TestConversionFromPrimitives()
        {
            new Bit().Should().Be(Bit.Zero);
            new Bit(false).Should().Be(Bit.Zero);
            new Bit(true).Should().Be(Bit.One);
            new Bit(0).Should().Be(Bit.Zero);
            new Bit(1).Should().Be(Bit.One);
            new Bit(2).Should().Be(Bit.Zero);
            new Bit(3).Should().Be(Bit.One);
        }
    }
}
