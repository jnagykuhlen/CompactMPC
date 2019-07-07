using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CompactMPC.ObliviousTransfer.UnitTests
{
    internal class RandomOracleStub : RandomOracle
    {
        private Dictionary<byte[], byte[]> _valueDict;

        public RandomOracleStub(Dictionary<byte[], byte[]> valueDict)
        {
            _valueDict = new Dictionary<byte[], byte[]>(valueDict);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <exception cref="KeyNotFoundException">If the query is not found in the value dictionary passed into the constructor.</exception>
        /// </remarks>
        public override IEnumerable<byte> Invoke(byte[] query)
        {
            return _valueDict[query];
        }
    }

    [TestClass]
    public class RandomOracleTest
    {
        [TestMethod]
        public void TestStub()
        {
            Dictionary<byte[], byte[]> keyValuePairs = new Dictionary<byte[], byte[]>();
            keyValuePairs[BitArray.FromBinaryString("1001").ToBytes()] = BitArray.FromBinaryString("100101011100101").ToBytes();
            keyValuePairs[BitArray.FromBinaryString("1010").ToBytes()] = BitArray.FromBinaryString("101011110101100").ToBytes();

            RandomOracle oracle = new RandomOracleStub(keyValuePairs);
            foreach (KeyValuePair<byte[], byte[]> pair in keyValuePairs)
            {
                Assert.AreEqual(pair.Value, oracle.Invoke(pair.Key));
            }
        }

        [TestMethod]
        public void TestMask()
        {
            var query1 = BitArray.FromBinaryString("1001");
            var query2 = BitArray.FromBinaryString("1010");
            byte[] query1Bytes = query1.ToBytes();
            byte[] query2Bytes = query2.ToBytes();

            var value1 = BitArray.FromBinaryString("1001010111001011");
            var value2 = BitArray.FromBinaryString("1010111101011001");

            Dictionary<byte[], byte[]> keyValuePairs = new Dictionary<byte[], byte[]>();
            keyValuePairs[query1Bytes] = value1.ToBytes();
            keyValuePairs[query2Bytes] = value2.ToBytes();

            RandomOracle oracle = new RandomOracleStub(keyValuePairs);

            var message1 = BitArray.FromBinaryString("0000000000000000");
            var message2 = BitArray.FromBinaryString("1100110011001100");
            byte[] message1Bytes = message1.ToBytes();
            byte[] message2Bytes = message2.ToBytes();

            byte[] masked11 = oracle.Mask(message1Bytes, query1Bytes);
            byte[] masked12 = oracle.Mask(message1Bytes, query2Bytes);
            CollectionAssert.AreEqual(keyValuePairs[query1Bytes], masked11);
            CollectionAssert.AreEqual(keyValuePairs[query2Bytes], masked12);


            byte[] masked21 = oracle.Mask(message2Bytes, query1Bytes);
            byte[] masked22 = oracle.Mask(message2Bytes, query2Bytes);

            BitArray expected12 = BitArray.FromBytes(keyValuePairs[query2Bytes], 16);
            expected12.Xor(message1);
            BitArray expected22 = BitArray.FromBytes(keyValuePairs[query2Bytes], 16);
            expected22.Xor(message2);

            CollectionAssert.AreEqual(expected12.ToBytes(), masked12);
            CollectionAssert.AreEqual(expected22.ToBytes(), masked22);
        }
    }
}
