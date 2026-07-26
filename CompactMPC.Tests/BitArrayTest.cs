using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC;

[TestClass]
public class BitArrayTest
{
    [TestMethod]
    public void TestByteConversion()
    {
        var array = BitArray.FromBinaryString("100101011100101");

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
        BitArray.FromBinaryString("1001").Xor(BitArray.FromBinaryString("1010")).ToBinaryString().Should().Be("0011");
        BitArray.FromBinaryString("1001").And(BitArray.FromBinaryString("1010")).ToBinaryString().Should().Be("1000");
        BitArray.FromBinaryString("1001").Not().ToBinaryString().Should().Be("0110");
    }
}
