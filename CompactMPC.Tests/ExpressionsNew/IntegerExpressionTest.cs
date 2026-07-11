using System;
using CompactMPC.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew;

[TestClass]
public class IntegerExpressionTest
{
    [TestMethod]
    public void TestWriteTo()
    {
        var expression = IntegerExpression.AssignableUpTo(15);

        var bits = BitArray.FromBinaryString("011011");

        expression.WriteTo(10, bits.WriteOnlySlice(2, 4));
        bits.ToBinaryString().Should().Be("010101");

        expression.WriteTo(3, bits.WriteOnlySlice(0, 4));
        bits.ToBinaryString().Should().Be("110001");
    }
    
    [TestMethod]
    public void TestWriteToOverflow()
    {
        var expression = IntegerExpression.AssignableUpTo(15);

        var bits = BitArray.FromBinaryString("011011");

        var writeAction = () => expression.WriteTo(17, bits.WriteOnlySlice(0, 4));

        writeAction.Should().Throw<ArgumentException>();
    }
    
    [TestMethod]
    public void TestWriteOutOfBounds()
    {
        var expression = IntegerExpression.AssignableUpTo(15);

        var bits = BitArray.FromBinaryString("011011");

        var writeAction = () => expression.WriteTo(3, bits.WriteOnlySlice(4, 4));

        writeAction.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void TestReadValue()
    {
        var expression = IntegerExpression.AssignableUpTo(15);
        var bits = BitArray.FromBinaryString("011010");

        expression.ReadFrom(bits.ReadOnlySlice(0, 4)).Should().Be(6);
        expression.ReadFrom(bits.ReadOnlySlice(1, 4)).Should().Be(11);
        expression.ReadFrom(bits.ReadOnlySlice(2, 4)).Should().Be(5);
    }
}
