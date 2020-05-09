using System.Collections.Generic;
using CompactMPC.Cryptography;

namespace CompactMPC.UnitTests.Cryptography
{
    public class ConstantRandomOracle : RandomOracle
    {
        private readonly byte[] _invokeResponse;

        public ConstantRandomOracle(byte[] invokeResponse)
        {
            _invokeResponse = invokeResponse;
        }

        public override IEnumerable<byte> Invoke(byte[] query)
        {
            return _invokeResponse;
        }

        public override void Dispose() { }
    }
}
