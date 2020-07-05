using System.Linq;
using System.Text;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Util;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class ObliviousTransferTest
    {
        private static readonly string[] TestOptions = { "Alicia", "Briann", "Charly", "Dennis" };

        [TestMethod]
        public void TestNaorPinkasObliviousTransfer()
        {
            TestNetworkRunner.RunTwoPartyNetwork(PerformObliviousTransfer);
        }

        private static void PerformObliviousTransfer(ITwoPartyNetworkSession session)
        {
            Quadruple<byte[]>[] options =
            {
                new Quadruple<byte[]>(TestOptions.Select(s => Encoding.ASCII.GetBytes(s)).ToArray()),
                new Quadruple<byte[]>(TestOptions.Select(s => Encoding.ASCII.GetBytes(s.ToLower())).ToArray()),
                new Quadruple<byte[]>(TestOptions.Select(s => Encoding.ASCII.GetBytes(s.ToUpper())).ToArray())
            };

            using CryptoContext cryptoContext = CryptoContext.CreateDefault();

            IGeneralizedObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                SecurityParameters.CreateDefault768Bit(),
                cryptoContext
            );

            if (session.LocalParty.Id == 0)
            {
                obliviousTransfer.SendAsync(session.Channel, options, 3, 6).Wait();
            }
            else
            {
                QuadrupleIndexArray indices = new QuadrupleIndexArray(new[] { 0, 3, 2 });
                byte[][] results = obliviousTransfer.ReceiveAsync(session.Channel, indices, 3, 6).Result;

                results.Should().BeEquivalentTo(options[0][0], options[1][3], options[2][2]);
            }
        }
    }
}
