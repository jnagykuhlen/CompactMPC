using System.Text;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class ObliviousTransferTest
    {
        [TestMethod]
        public Task TestNaorPinkasObliviousTransfer()
        {
            return LocalNetworkRunner.RunTwoPartyNetwork(PerformObliviousTransfer);
        }

        private static async Task PerformObliviousTransfer(ITwoPartyNetworkSession session)
        {
            Quadruple<Message>[] options =
            [
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

            var obliviousTransfer = new NaorPinkasObliviousTransfer(SecurityParameters.CreateDefault768Bit());

            if (session.LocalParty.Id == 0)
            {
                await obliviousTransfer.SendAsync(session.Channel, options, 3, 5);
            }
            else
            {
                var indices = new QuadrupleIndexArray([0, 3, 2]);
                var results = await obliviousTransfer.ReceiveAsync(session.Channel, indices, 3, 5);

                results.Should().Equal([options[0][0], options[1][3], options[2][2]]);
            }
        }

        private static Message CreateMessage(string messageText) => new(Encoding.ASCII.GetBytes(messageText));
    }
}