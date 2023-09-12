using System.Numerics;

namespace CompactMPC.Cryptography
{
    public static class RandomNumberGenerator
    {
        public static byte[] GetBytes(int numberOfBytes)
        {
            return System.Security.Cryptography.RandomNumberGenerator.GetBytes(numberOfBytes);
        }

        public static BitArray GetBits(int numberOfBits)
        {
            byte[] randomBytes = GetBytes(BitArray.RequiredBytes(numberOfBits));
            return BitArray.FromBytes(randomBytes, numberOfBits);
        }

        public static BigInteger GetBigInteger(int sizeInBytes)
        {
            byte[] randomBytes = GetBytes(sizeInBytes);
            return new BigInteger(randomBytes, true);
        }
    }
}
