using CompactMPC.Collections;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Expressions;

[TestClass]
public class BooleanExpressionTest
{
    [TestMethod]
    public void TestWriteBits()
    {
        var expression = BooleanExpression.False;
        var bits = BitArray.FromBinaryString("011");

        expression.WriteTo(false, bits.WriteOnlySlice(1, 1));
        bits.ToBinaryString().Should().Be("001");

        expression.WriteTo(true, bits.WriteOnlySlice(0, 1));
        bits.ToBinaryString().Should().Be("101");
    }

    [TestMethod]
    public void TestReadValue()
    {
        var expression = BooleanExpression.False;
        var bits = BitArray.FromBinaryString("011");

        expression.ReadFrom(bits.ReadOnlySlice(0, 1)).Should().Be(false);
        expression.ReadFrom(bits.ReadOnlySlice(1, 1)).Should().Be(true);
    }
}
