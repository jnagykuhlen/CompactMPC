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
            {
                new Quadruple<Message>(
                    CreateMessage("Zebra"),
                    CreateMessage("Mouse"),
                    CreateMessage("Whale"),
                    CreateMessage("Sheep")
                ),
                new Quadruple<Message>(
                    CreateMessage("China"),
                    CreateMessage("India"),
                    CreateMessage("Japan"),
                    CreateMessage("Nepal")
                ),
                new Quadruple<Message>(
                    CreateMessage("Apple"),
                    CreateMessage("Pizza"),
                    CreateMessage("Melon"),
                    CreateMessage("Bread")
                )
            };

            IMessageObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                SecurityParameters.CreateDefault768Bit()
            );

            if (session.LocalParty.Id == 0)
            {
                await obliviousTransfer.SendAsync(session.Channel, options, 3, 5);
            }
            else
            {
                QuadrupleIndexArray indices = new QuadrupleIndexArray(new[] { 0, 3, 2 });
                Message[] results = await obliviousTransfer.ReceiveAsync(session.Channel, indices, 3, 5);

                results.Should().BeEquivalentTo(options[0][0], options[1][3], options[2][2]);
            }
        }

        private static Message CreateMessage(string messageText)
        {
            return new Message(Encoding.ASCII.GetBytes(messageText));
        }
    }
}