using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CompactMPC.Buffers;

namespace CompactMPC.Cryptography;

public class HashRandomOracle : RandomOracle
{
    public override IEnumerable<byte> Invoke(byte[] query)
    {
        var seed = SHA256.HashData(query);

        var seedMessage = new Message(seed.Length + sizeof(int)).Write(seed);

        var counter = 0;
        while (counter < int.MaxValue)
        {
            var seedMessageWithCounter = seedMessage.Write(counter);
            var block = SHA256.HashData(seedMessageWithCounter.ToBuffer());

            foreach (var blockByte in block)
                yield return blockByte;

            counter++;
        }

        throw new InvalidOperationException("Random oracle cannot provide more data since the counter has reached its maximum value.");
    }
}
