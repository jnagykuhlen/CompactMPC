using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.ObliviousTransfer;

namespace CompactMPC.UnitTests.ObliviousTransfer
{
    [TestClass]
    public class RandomOracleTest
    {

        [TestMethod]
        public void TestMask()
        {
            byte[] query = { 0x00 };
            BitArray invokeResponse = BitArray.FromBinaryString("1001010111001011");
            BitArray message = BitArray.FromBinaryString("1100110011001100");

            BitArray expectedMaskedMessage = message ^ invokeResponse;

            RandomOracle oracle = new ConstantRandomOracle(invokeResponse.ToBytes());
            byte[] maskedMessage = oracle.Mask(message.ToBytes(), query);

            CollectionAssert.AreEqual(expectedMaskedMessage.ToBytes(), maskedMessage);
        }
    }
}
