using System.Security.Cryptography;

namespace CompactMPC.Cryptography
{
    public class ThreadSafeRandomNumberGenerator : RandomNumberGenerator
    {
        private readonly RandomNumberGenerator _randomNumberGenerator;
        private readonly object _randomNumberGeneratorLock;

        public ThreadSafeRandomNumberGenerator(RandomNumberGenerator randomNumberGenerator)
        {
            _randomNumberGenerator = randomNumberGenerator;
            _randomNumberGeneratorLock = new object();
        }

        public override void GetBytes(byte[] data)
        {
            lock (_randomNumberGeneratorLock)
            {
                _randomNumberGenerator.GetBytes(data);
            }
        }
    }
}
