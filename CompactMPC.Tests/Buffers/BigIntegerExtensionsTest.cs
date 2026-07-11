using System;
using System.Numerics;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Buffers;

[TestClass]
public class BigIntegerExtensionsTest
{
    [TestMethod]
    public void TestReadAndWrite()
    {
        var bigIntegerWritten = new BigInteger(41235345);

        var message = new Message(10)
            .Write(10, bigIntegerWritten);

        message.Length.Should().Be(10);

        var remainingMessage = message.ReadBigInteger(10, out var bigIntegerRead);

        remainingMessage.Length.Should().Be(0);
        bigIntegerRead.Should().Be(bigIntegerWritten);
    }

    [TestMethod]
    public void TestNotRepresentableWithNumberOfBytes()
    {
        var bigIntegerWritten = new BigInteger(68123);

        Action write = () => new Message(10).Write(2, bigIntegerWritten);

        write.Should().Throw<ArgumentException>();
    }
}