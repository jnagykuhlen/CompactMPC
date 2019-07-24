using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.ObliviousTransfer;

namespace CompactMPC.UnitTests.ObliviousTransfer
{
    [TestClass]
    public class HashRandomOracleTest
    {
        [TestMethod]
        public void TestInvoke()
        {
            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                RandomOracle oracle = new HashRandomOracle(cryptoContext.HashAlgorithm);

                byte[] firstQuery = { 235, 12, 13, 72, 138, 13, 62, 13, 39, 147, 198, 173, 23, 87, 27, 99 };
                byte[] secondQuery = { 84, 23, 123, 85, 62, 28, 54, 98, 187, 238, 18, 5, 78, 1, 78, 243 };

                byte[] firstResponse = oracle.Invoke(firstQuery).Take(10).ToArray();
                byte[] secondResponse = oracle.Invoke(secondQuery).Take(10).ToArray();

                CollectionAssert.AreNotEqual(firstResponse, secondResponse);
            }
        }

        [TestMethod]
        public void TestThreadsafe()
        {
            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                RandomOracle oracle = new HashRandomOracle(cryptoContext.HashAlgorithm);

                byte[] query = new[] { (byte)0x34, (byte)0x2f, (byte)0xab, (byte)0x25, (byte)0x33 };
                byte[] expected = oracle.Invoke(query).Take(20).ToArray();

                int count = 1000;
                Parallel.For(0, 2, i =>
                {
                    for (int j = 0; j < count; ++j)
                    {
                        byte[] result = oracle.Invoke(query).Take(20).ToArray();
                        CollectionAssert.AreEqual(expected, result);
                    }
                });
            }
        }

        [TestMethod]
        public void TestThreadsafeMultipleInstancesSameHash()
        {
            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                RandomOracle[] oracle = new[] { new HashRandomOracle(cryptoContext.HashAlgorithm), new HashRandomOracle(cryptoContext.HashAlgorithm) };

                byte[] query = new[] { (byte)0x34, (byte)0x2f, (byte)0xab, (byte)0x25, (byte)0x33 };
                byte[] expected = oracle[0].Invoke(query).Take(20).ToArray();

                int count = 1000;
                Parallel.For(0, 2, i =>
                {
                    for (int j = 0; j < count; ++j)
                    {
                        byte[] result = oracle[i].Invoke(query).Take(20).ToArray();
                        CollectionAssert.AreEqual(expected, result);
                    }
                });
            }
        }
    }
}
