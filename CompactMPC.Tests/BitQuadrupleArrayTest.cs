using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class BitQuadrupleArrayTest
    {
        [TestMethod]
        public void TestByteConversion()
        {
            BitQuadrupleArray array = new BitQuadrupleArray(new[]
            {
                new BitQuadruple(Bit.Zero, Bit.One, Bit.One, Bit.Zero),
                new BitQuadruple(Bit.One, Bit.One, Bit.Zero, Bit.One),
                new BitQuadruple(Bit.Zero, Bit.One, Bit.Zero, Bit.One)
            })
            {
                [1] = new BitQuadruple(Bit.One, Bit.Zero, Bit.One, Bit.One)
            };
            
            array.Should().HaveCount(3);
            array.ToBytes().Should().Equal(0b11010110, 0b00001010);
        }

        [TestMethod]
        public void TestRequiredBytes()
        {
            BitQuadrupleArray.RequiredBytes(0).Should().Be(0);
            BitQuadrupleArray.RequiredBytes(1).Should().Be(1);
            BitQuadrupleArray.RequiredBytes(2).Should().Be(1);
            BitQuadrupleArray.RequiredBytes(3).Should().Be(2);
        }
    }
}
