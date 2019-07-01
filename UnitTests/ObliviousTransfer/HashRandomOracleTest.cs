using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CompactMPC.ObliviousTransfer.UnitTests
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

                byte[] query1Bytes = { 235, 12, 13, 72, 138, 13, 62, 13, 39, 147, 198, 173, 23, 87, 27, 99 };
                byte[] query2Bytes = { 84, 23, 123, 85, 62, 28, 54, 98, 187, 238, 18, 5, 78, 1, 78, 243 };

                byte[] response1 = oracle.Invoke(query1Bytes).Take(10).ToArray();
                byte[] response2 = oracle.Invoke(query2Bytes).Take(10).ToArray();
                CollectionAssert.AreNotEqual(response1, response2);
            }
        }
    }
}
