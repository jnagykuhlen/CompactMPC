using System;
using System.Numerics;

namespace CompactMPC.Buffers;

public static class BigIntegerExtensions
{
    public static Message Write(this Message message, int numberOfBytes, BigInteger bigInteger)
    {
        var bytes = bigInteger.ToByteArray(true);
        if (numberOfBytes < bytes.Length)
            throw new ArgumentException("Cannot represent integer value with given number of bytes.", nameof(numberOfBytes));

        if (numberOfBytes == bytes.Length)
            return message.Write(bytes);

        return message
            .Write(bytes)
            .Pad(numberOfBytes - bytes.Length);
    }

    public static Message ReadBigInteger(this Message message, int numberOfBytes, out BigInteger bigInteger)
    {
        var remainingMessage = message.ReadBytes(numberOfBytes, out var bytes);
        bigInteger = new BigInteger(bytes, true);
        return remainingMessage;
    }
}