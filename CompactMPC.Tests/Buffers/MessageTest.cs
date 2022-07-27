using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Buffers
{
    [TestClass]
    public class MessageTest
    {
        [TestMethod]
        public void TestReadAndWrite()
        {
            byte[] prefix = { 0x01, 0x03, 0x05, 0xf7, 0x59 };
            int firstId = 0x00bbdf19;
            int secondId = 0x7bc00127;
            byte[] suffix = { 0x43, 0xb1 };

            Message message = Message.Empty
                .Write(prefix)
                .Write(firstId)
                .Write(secondId)
                .Write(suffix);

            message.Length.Should().Be(15);
            message.ToBuffer().Should().Equal(
                0x01, 0x03, 0x05, 0xf7, 0x59,
                0x19, 0xdf, 0xbb, 0x00,
                0x27, 0x01, 0xc0, 0x7b,
                0x43, 0xb1
            );

            Message remainingMessage = message
                .ReadBytes(5, out byte[] readPrefix)
                .ReadInt(out int readFirstId)
                .ReadInt(out int readSecondId)
                .ReadBytes(2, out byte[] readSuffix);

            readPrefix.Should().Equal(prefix);
            readFirstId.Should().Be(firstId);
            readSecondId.Should().Be(secondId);
            readSuffix.Should().Equal(suffix);

            remainingMessage.Should().BeSameAs(Message.Empty);
        }

        [TestMethod]
        public void TestSubMessage()
        {
            byte[] prefix = { 0x01, 0x03, 0x05, 0x7f, 0x59 };
            int id = 0x00bbdf19;

            Message message = Message.Empty
                .Write(prefix)
                .Write(id);
            
            message.Length.Should().Be(9);

            Message remainingMessage = message.ReadInt(out int readValue);

            readValue.Should().Be(0x7f050301);
            remainingMessage.Length.Should().Be(5);

            remainingMessage.ToBuffer().Should().Equal(0x59, 0x19, 0xdf, 0xbb, 0x00);
        }
    }
}
