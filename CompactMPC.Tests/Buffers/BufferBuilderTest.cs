using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Buffers
{
    [TestClass]
    public class BufferBuilderTest
    {
        [TestMethod]
        public void TestCombineBufferWithIds()
        {
            byte[] prefix = { 0x01, 0x03, 0x05, 0xf7, 0x59 };
            int firstId = 0x00bbdf19;
            int secondId = 0x7bc00127;
            byte[] suffix = { 0x43, 0xb1 };

            byte[] result = BufferBuilder.From(prefix).With(firstId).With(secondId).With(suffix).Create();

            result.Should().Equal(
                0x01, 0x03, 0x05, 0xf7, 0x59,
                0x19, 0xdf, 0xbb, 0x00,
                0x27, 0x01, 0xc0, 0x7b,
                0x43, 0xb1
            );
        }
    }
}
