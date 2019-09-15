using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.Cryptography;

namespace CompactMPC.UnitTests.Cryptography
{
    [TestClass]
    public class HashRandomOracleTest
    {
        [TestMethod]
        public void TestInvoke()
        {
            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                RandomOracle oracle = new HashRandomOracle(cryptoContext.HashAlgorithmProvider);

                byte[] firstQuery = { 235, 12, 13, 72, 138, 13, 62, 13, 39, 147, 198, 173, 23, 87, 27, 99 };
                byte[] secondQuery = { 84, 23, 123, 85, 62, 28, 54, 98, 187, 238, 18, 5, 78, 1, 78, 243 };

                byte[] firstResponse = oracle.Invoke(firstQuery).Take(10).ToArray();
                byte[] secondResponse = oracle.Invoke(secondQuery).Take(10).ToArray();

                CollectionAssert.AreNotEqual(firstResponse, secondResponse);
            }
        }
    }
}
