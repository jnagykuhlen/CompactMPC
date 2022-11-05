using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CompactMPC.Buffers;

namespace CompactMPC.Cryptography
{
    public class HashRandomOracle : RandomOracle
    {
        public override IEnumerable<byte> Invoke(byte[] query)
        {
            byte[] seed = SHA256.HashData(query);

            Message seedMessage = new Message(seed.Length + sizeof(int)).Write(seed);

            int counter = 0;
            while (counter < int.MaxValue)
            {
                Message seedMessageWithCounter = seedMessage.Write(counter);
                byte[] block = SHA256.HashData(seedMessageWithCounter.ToBuffer());

                foreach (byte blockByte in block)
                    yield return blockByte;

                counter++;
            }

            throw new InvalidOperationException("Random oracle cannot provide more data since the counter has reached its maximum value.");
        }
    }
}
