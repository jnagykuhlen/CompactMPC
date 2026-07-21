using System.Text;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class ObliviousTransferTest
    {
        private static readonly Quadruple<Message>[] Options = [
            new(
                CreateMessage("Zebra"),
                CreateMessage("Mouse"),
                CreateMessage("Whale"),
                CreateMessage("Sheep")
            ),
            new(
                CreateMessage("China"),
                CreateMessage("India"),
                CreateMessage("Japan"),
                CreateMessage("Nepal")
            ),
            new(
                CreateMessage("Apple"),
                CreateMessage("Pizza"),
                CreateMessage("Melon"),
                CreateMessage("Bread")
            )
        ];

        [TestMethod]
        public Task TestNaorPinkasObliviousTransfer()
        {
            return LocalNetworkRunner.RunTwoPartyNetwork(PerformSenderAsync, PerformReceiverAsync);
        }

        private static async Task PerformSenderAsync(ITwoPartyNetworkSession session)
        {
            var obliviousTransfer = new NaorPinkasObliviousTransfer(SecurityParameters.CreateDefault768Bit());
            await obliviousTransfer.SendAsync(session.Channel, Options, 3, 5);
        }

        private static async Task PerformReceiverAsync(ITwoPartyNetworkSession session)
        {
            var obliviousTransfer = new NaorPinkasObliviousTransfer(SecurityParameters.CreateDefault768Bit());

            var indices = new QuadrupleIndexArray([0, 3, 2]);
            var results = await obliviousTransfer.ReceiveAsync(session.Channel, indices, 3, 5);

            results.Should().Equal([Options[0][0], Options[1][3], Options[2][2]]);
        }

        private static Message CreateMessage(string messageText) => new(Encoding.ASCII.GetBytes(messageText));
    }
}