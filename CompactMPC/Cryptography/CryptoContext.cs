using System;
using System.Security.Cryptography;

namespace CompactMPC.Cryptography
{
    public sealed class CryptoContext : IDisposable
    {
        public RandomNumberGenerator RandomNumberGenerator { get; }
        public IHashAlgorithmProvider HashAlgorithmProvider { get; }
        
        public CryptoContext(RandomNumberGenerator randomNumberGenerator, IHashAlgorithmProvider hashAlgorithmProvider)
        {
            RandomNumberGenerator = randomNumberGenerator;
            HashAlgorithmProvider = hashAlgorithmProvider;
        }

        public static CryptoContext CreateDefault()
        {
            return new CryptoContext(
                RandomNumberGenerator.Create(),
                new SHA256HashAlgorithmProvider()
            );
        }

        public void Dispose()
        {
            RandomNumberGenerator.Dispose();
        }
    }
}
