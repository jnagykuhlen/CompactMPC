using System;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Protocol.Primitives;

[TestClass]
public class SecureIntegerTest
{
    private readonly SecureInteger _expression = SecureInteger.AssignableUpTo(15);

    [DataRow(0, "0000")]
    [DataRow(3, "1100")]
    [DataRow(10, "0101")]
    [TestMethod]
    public void TestWriteTo(int value, string expected)
    {
        var bits = new BitArray(4);
        _expression.WriteTo(value, bits);
        bits.ToBinaryString().Should().Be(expected);
    }

    [TestMethod]
    public void TestWriteToOutOfBounds()
    {
        var bits = new BitArray(2);
        var writeAction = () => _expression.WriteTo(14, bits);
        writeAction.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void TestWriteToOverflow()
    {
        var bits = new BitArray(4);
        var writeAction = () => _expression.WriteTo(17, bits);
        writeAction.Should().Throw<ArgumentException>();
    }

    [DataRow("0000", 0)]
    [DataRow("0110", 6)]
    [DataRow("1101", 11)]
    [TestMethod]
    public void TestReadValue(string bits, int expected) =>
        _expression.ReadFrom(BitArray.FromBinaryString(bits)).Should().Be(expected);
}
