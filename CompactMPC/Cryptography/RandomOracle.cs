using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Cryptography
{
    public abstract class RandomOracle : IDisposable
    {
        public byte[] Mask(byte[] message, byte[] query)
        {
            byte[] result = new byte[message.Length];
            int index = 0;

            foreach (byte maskByte in Invoke(query).Take(message.Length))
            {
                result[index] = (byte)(message[index] ^ maskByte);
                index++;
            }

            if (index < message.Length)
                throw new ArgumentException("Random oracle invocation does not provide enough data to mask the given message.", nameof(query));
            
            return result;
        }

        public abstract IEnumerable<byte> Invoke(byte[] query);
        public abstract void Dispose();
    }
}
