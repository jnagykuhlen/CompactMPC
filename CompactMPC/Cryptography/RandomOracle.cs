using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Cryptography;

public abstract class RandomOracle
{
    public byte[] Mask(byte[] message, byte[] query)
    {
        var result = new byte[message.Length];
        var index = 0;

        foreach (var maskByte in Invoke(query).Take(message.Length))
        {
            result[index] = (byte)(message[index] ^ maskByte);
            index++;
        }

        if (index < message.Length)
            throw new ArgumentException("Random oracle invocation does not provide enough data to mask the given message.", nameof(query));
            
        return result;
    }

    public abstract IEnumerable<byte> Invoke(byte[] query);
}
