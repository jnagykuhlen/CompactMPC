using System.Numerics;

namespace CompactMPC.Cryptography;

public static class RandomNumberGenerator
{
    public static byte[] GetBytes(int numberOfBytes) =>
        System.Security.Cryptography.RandomNumberGenerator.GetBytes(numberOfBytes);

    public static BitArray GetBits(int numberOfBits)
    {
        var randomBytes = GetBytes(BitArray.RequiredBytes(numberOfBits));
        return BitArray.FromBytes(randomBytes, numberOfBits);
    }

    public static BigInteger GetBigInteger(int sizeInBytes)
    {
        var randomBytes = GetBytes(sizeInBytes);
        return new BigInteger(randomBytes, true);
    }
}
