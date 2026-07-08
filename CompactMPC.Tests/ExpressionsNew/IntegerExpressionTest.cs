using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew;

[TestClass]
public class IntegerExpressionTest
{
    [TestMethod]
    public void TestWriteBits()
    {
        var expression = IntegerExpression.AssignableUpTo(15);

        var bits = BitArray.FromBinaryString("011011");

        expression.WriteBits(10, bits, 2);
        bits.ToBinaryString().Should().Be("010101");

        expression.WriteBits(3, bits, 0);
        bits.ToBinaryString().Should().Be("110001");
    }
    
    [TestMethod]
    public void TestWriteBitsOverflow()
    {
        var expression = IntegerExpression.AssignableUpTo(15);

        var bits = BitArray.FromBinaryString("011011");

        var writeAction = () => expression.WriteBits(17, bits, 0);

        writeAction.Should().Throw<ArgumentException>();
    }
    
    [TestMethod]
    public void TestWriteOutOfBounds()
    {
        var expression = IntegerExpression.AssignableUpTo(15);

        var bits = BitArray.FromBinaryString("011011");

        var writeAction = () => expression.WriteBits(3, bits, 4);

        writeAction.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void TestReadValue()
    {
        var expression = IntegerExpression.AssignableUpTo(15);
        var bits = BitArray.FromBinaryString("011010");

        expression.ReadValue(bits, 0).Should().Be(6);
        expression.ReadValue(bits, 1).Should().Be(11);
        expression.ReadValue(bits, 2).Should().Be(5);
    }
}
