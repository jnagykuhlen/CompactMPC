using System.Collections.Generic;

namespace CompactMPC.Cryptography;

public class ConstantRandomOracle(byte[] invokeResponse) : RandomOracle
{
    public override IEnumerable<byte> Invoke(byte[] query) => invokeResponse;
}
