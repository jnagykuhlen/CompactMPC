using System;
using System.Numerics;

namespace CompactMPC.Buffers
{
    public static class BigIntegerHelper
    {
        public static Message Write(this Message message, int numberOfBytes, BigInteger bigInteger)
        {
            byte[] bytes = bigInteger.ToByteArray(true);
            if (numberOfBytes < bytes.Length)
                throw new ArgumentException("Cannot represent integer value with given number of bytes.", nameof(numberOfBytes));

            if (numberOfBytes == bytes.Length)
                return message.Write(bytes);

            byte[] padding = new byte[numberOfBytes - bytes.Length];
            return message
                .Write(bytes)
                .Write(padding);
        }

        public static Message ReadBigInteger(this Message message, int numberOfBytes, out BigInteger bigInteger)
        {
            Message remainingMessage = message.ReadBytes(numberOfBytes, out byte[] bytes);
            bigInteger = new BigInteger(bytes, true);
            return remainingMessage;
        }
    }
}
