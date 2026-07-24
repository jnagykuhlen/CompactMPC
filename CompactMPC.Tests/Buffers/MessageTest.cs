using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Buffers;

[TestClass]
public class MessageTest
{
    [TestMethod]
    public void TestReadAndWrite()
    {
        byte[] prefix = [0x01, 0x03, 0x05, 0xf7, 0x59];
        var firstId = 0x00bbdf19;
        var secondId = 0x7bc00127;
        byte[] suffix = [0x43, 0xb1];

        var message = new Message(17)
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

        var remainingMessage = message
            .ReadBytes(5, out var readPrefix)
            .ReadInt(out var readFirstId)
            .ReadInt(out var readSecondId)
            .ReadBytes(2, out var readSuffix);

        readPrefix.Should().Equal(prefix);
        readFirstId.Should().Be(firstId);
        readSecondId.Should().Be(secondId);
        readSuffix.Should().Equal(suffix);

        remainingMessage.Length.Should().Be(0);
    }

    [TestMethod]
    public void TestSubMessage()
    {
        byte[] prefix = [0x01, 0x03, 0x05, 0x7f, 0x59];
        var id = 0x00bbdf19;

        var message = new Message(9)
            .Write(prefix)
            .Write(id);
            
        message.Length.Should().Be(9);

        var remainingMessage = message.ReadInt(out var readValue);

        readValue.Should().Be(0x7f050301);
        remainingMessage.Length.Should().Be(5);

        remainingMessage.ToBuffer().Should().Equal(0x59, 0x19, 0xdf, 0xbb, 0x00);
    }
        
    [TestMethod]
    public void TestPadding()
    {
        var message = new Message(4)
            .Write([123])
            .Pad(3);
            
        message.Length.Should().Be(4);

        var remainingMessage = message.ReadInt(out var readValue);

        readValue.Should().Be(123);
        remainingMessage.Length.Should().Be(0);
    }
}