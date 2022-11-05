using System.Numerics;
using System.Security.Cryptography;

namespace CompactMPC.Cryptography
{
    public static class RandomNumberGeneratorHelper
    {
        public static byte[] GetBytes(this RandomNumberGenerator randomNumberGenerator, int numberOfBytes)
        {
            byte[] randomBytes = new byte[numberOfBytes];
            randomNumberGenerator.GetBytes(randomBytes);
            return randomBytes;
        }

        public static BitArray GetBits(this RandomNumberGenerator randomNumberGenerator, int numberOfBits)
        {
            byte[] randomBytes = randomNumberGenerator.GetBytes(BitArray.RequiredBytes(numberOfBits));
            return BitArray.FromBytes(randomBytes, numberOfBits);
        }

        public static BigInteger GetBigInteger(this RandomNumberGenerator randomNumberGenerator, int sizeInBytes)
        {
            byte[] randomBytes = randomNumberGenerator.GetBytes(sizeInBytes);
            return new BigInteger(randomBytes, true);
        }
    }
}
