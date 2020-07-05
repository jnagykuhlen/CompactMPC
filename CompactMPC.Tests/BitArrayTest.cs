using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class BitArrayTest
    {
        [TestMethod]
        public void TestByteConversion()
        {
            BitArray array = BitArray.FromBinaryString("100101011100101");

            array[2] = Bit.One;
            array[5] = Bit.Zero;

            array.Should().HaveCount(15);
            array.ToBinaryString().Should().Be("101100011100101");
            array.ToBytes().Should().Equal(0b10001101, 0b01010011);
        }

        [TestMethod]
        public void TestRequiredBytes()
        {
            BitArray.RequiredBytes(0).Should().Be(0);
            BitArray.RequiredBytes(1).Should().Be(1);
            BitArray.RequiredBytes(7).Should().Be(1);
            BitArray.RequiredBytes(8).Should().Be(1);
            BitArray.RequiredBytes(9).Should().Be(2);
        }

        [TestMethod]
        public void TestBitwiseOperations()
        {
            BitArray left = BitArray.FromBinaryString("1001");
            BitArray right = BitArray.FromBinaryString("1010");

            (left | right).ToBinaryString().Should().Be("1011");
            (left ^ right).ToBinaryString().Should().Be("0011");
            (left & right).ToBinaryString().Should().Be("1000");
            (~left).ToBinaryString().Should().Be("0110");
        }
    }
}
