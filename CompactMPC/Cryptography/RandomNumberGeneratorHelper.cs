using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

namespace CompactMPC.Cryptography
{
    public static class RandomNumberGeneratorHelper
    {
        public static byte[] GetBytes(this RandomNumberGenerator randomNumberGenerator, int numberOfBytes)
        {
            byte[] randomSource = new byte[numberOfBytes];
            randomNumberGenerator.GetBytes(randomSource);
            return randomSource;
        }

        public static BitArray GetBits(this RandomNumberGenerator randomNumberGenerator, int numberOfBits)
        {
            byte[] randomSource = new byte[BitArray.RequiredBytes(numberOfBits)];
            randomNumberGenerator.GetBytes(randomSource);
            return BitArray.FromBytes(randomSource, numberOfBits);
        }

        public static BigInteger GetBigInteger(this RandomNumberGenerator randomNumberGenerator, int sizeInBytes)
        {
            byte[] randomSource = new byte[sizeInBytes];
            randomNumberGenerator.GetBytes(randomSource);
            randomSource[sizeInBytes - 1] &= 0x7f;

            return new BigInteger(randomSource);
        }
    }
}
