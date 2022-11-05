using System;
using System.Security.Cryptography;

namespace CompactMPC.Cryptography
{
    public sealed class CryptoContext : IDisposable
    {
        public RandomNumberGenerator RandomNumberGenerator { get; }

        public CryptoContext(RandomNumberGenerator randomNumberGenerator)
        {
            RandomNumberGenerator = randomNumberGenerator;
        }

        public static CryptoContext CreateDefault()
        {
            return new CryptoContext(
                RandomNumberGenerator.Create()
            );
        }

        public void Dispose()
        {
            RandomNumberGenerator.Dispose();
        }
    }
}
