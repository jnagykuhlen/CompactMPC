using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CompactMPC.ObliviousTransfer.UnitTests
{
    internal class ConstantRandomOracleMock : RandomOracle
    {
        private byte[] _response;

        public ConstantRandomOracleMock(byte[] staticResponse)
        {
            _response = staticResponse;
        }
        
        public override IEnumerable<byte> Invoke(byte[] query)
        {
            return _response;
        }
    }

    [TestClass]
    public class RandomOracleTest
    {

        [TestMethod]
        public void TestMask()
        {
            byte[] query = new byte[] { 0x13, 0xf4 };
            BitArray randomOracleOutput = BitArray.FromBinaryString("1001010111001011");
            BitArray message = BitArray.FromBinaryString("1100110011001100");

            BitArray expectedMaskedMessage = message ^ randomOracleOutput;

            RandomOracle oracle = new ConstantRandomOracleMock(randomOracleOutput.ToBytes());
            byte[] maskedMessage = oracle.Mask(message.ToBytes(), query);

            CollectionAssert.AreEqual(expectedMaskedMessage.ToBytes(), maskedMessage);
        }
    }
}
