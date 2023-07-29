using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Networking
{
    [TestClass]
    public class TcpTwoPartyNetworkSessionTest
    {
        private static readonly Party FirstParty = new Party(0, "First");
        private static readonly Party SecondParty = new Party(1, "Second");

        private static readonly IPEndPoint FirstEndPoint = new IPEndPoint(IPAddress.Loopback, 12674);
        private static readonly IPEndPoint SecondEndPoint = new IPEndPoint(IPAddress.Loopback, 12675);

        [TestMethod]
        public async Task TestTcpTwoPartyNetworkSession()
        {
            Task<TcpTwoPartyNetworkSession> firstSessionTask = TcpTwoPartyNetworkSession.EstablishAsync(FirstParty, FirstEndPoint, SecondEndPoint);
            Task<TcpTwoPartyNetworkSession> secondSessionTask = TcpTwoPartyNetworkSession.EstablishAsync(SecondParty, SecondEndPoint, FirstEndPoint);

            using TcpTwoPartyNetworkSession firstSession = await firstSessionTask;
            using TcpTwoPartyNetworkSession secondSession = await secondSessionTask;

            firstSession.LocalParty.Should().Be(FirstParty);
            firstSession.RemoteParty.Should().Be(SecondParty);

            secondSession.LocalParty.Should().Be(SecondParty);
            secondSession.RemoteParty.Should().Be(FirstParty);
        }
    }
}