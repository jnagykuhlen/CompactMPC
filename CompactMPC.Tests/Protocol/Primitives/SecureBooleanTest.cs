using System;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Protocol.Primitives;

[TestClass]
public class SecureBooleanTest
{
    private readonly SecureBoolean _expression = SecureBoolean.Assignable();

    [DataRow(false, "0")]
    [DataRow(true, "1")]
    [TestMethod]
    public void TestWriteTo(bool value, string expected)
    {
        var bits = new BitArray(1);
        _expression.WriteTo(value, bits);
        bits.ToBinaryString().Should().Be(expected);
    }

    [TestMethod]
    public void TestWriteToEmptyList()
    {
        var bits = new BitArray(0);
        var writeAction = () => _expression.WriteTo(true, bits);
        writeAction.Should().Throw<ArgumentOutOfRangeException>();
    }

    [DataRow("0", false)]
    [DataRow("1", true)]
    [TestMethod]
    public void TestReadFrom(string bits, bool expected) =>
        _expression.ReadFrom(BitArray.FromBinaryString(bits)).Should().Be(expected);
}
