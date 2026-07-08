using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew;

[TestClass]
public class BooleanExpressionTest
{
    [TestMethod]
    public void TestWriteBits()
    {
        var expression = BooleanExpression.False;
        var bits = BitArray.FromBinaryString("011");

        expression.WriteBits(false, bits, 1);
        bits.ToBinaryString().Should().Be("001");

        expression.WriteBits(true, bits, 0);
        bits.ToBinaryString().Should().Be("101");
    }

    [TestMethod]
    public void TestReadValue()
    {
        var expression = BooleanExpression.False;
        var bits = BitArray.FromBinaryString("011");

        expression.ReadValue(bits, 0).Should().Be(false);
        expression.ReadValue(bits, 1).Should().Be(true);
    }
}
