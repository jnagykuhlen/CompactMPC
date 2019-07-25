using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CompactMPC
{
    public sealed class CryptoContext : IDisposable
    {
        private RandomNumberGenerator _randomNumberGenerator;
        private ThreadsafeHashAlgorithm _hashAlgorithm;

        public CryptoContext(RandomNumberGenerator randomNumberGenerator, ThreadsafeHashAlgorithm hashAlgorithm)
        {
            if (randomNumberGenerator == null)
                throw new ArgumentNullException(nameof(randomNumberGenerator));

            if (hashAlgorithm == null)
                throw new ArgumentNullException(nameof(hashAlgorithm));

            _randomNumberGenerator = randomNumberGenerator;
            _hashAlgorithm = hashAlgorithm;
        }

        public static CryptoContext CreateDefault()
        {
            return new CryptoContext(
                RandomNumberGenerator.Create(),
                new ThreadsafeSHA256()
            );
        }

        public void Dispose()
        {
            _randomNumberGenerator.Dispose();
            _hashAlgorithm.Dispose();
        }

        public RandomNumberGenerator RandomNumberGenerator
        {
            get
            {
                return _randomNumberGenerator;
            }
        }

        public ThreadsafeHashAlgorithm HashAlgorithm
        {
            get
            {
                return _hashAlgorithm;
            }
        }
    }
}
