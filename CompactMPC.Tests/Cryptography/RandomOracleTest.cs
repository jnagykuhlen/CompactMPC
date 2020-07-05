using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Cryptography
{
    [TestClass]
    public class RandomOracleTest
    {
        [TestMethod]
        public void TestMask()
        {
            byte[] query = { };
            byte[] invokeResponse = {0xa7, 0x38};
            byte[] message = {0x21, 0xf2};

            RandomOracle oracle = new ConstantRandomOracle(invokeResponse);
            byte[] maskedMessage = oracle.Mask(message, query);

            maskedMessage.Should().Equal(0xa7 ^ 0x21, 0x38 ^ 0xf2);
        }
    }
}
