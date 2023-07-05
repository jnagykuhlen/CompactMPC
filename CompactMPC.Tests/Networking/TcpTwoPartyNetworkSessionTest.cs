using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Networking
{
    [TestClass]
    public class TcpTwoPartyNetworkSessionTest
    {
        private const int Port = 12674;
        
        private static readonly Party FirstParty = new Party(0, "First");
        private static readonly Party SecondParty = new Party(1, "Second");

        [TestMethod]
        public async Task TestTcpTwoPartyNetworkSession()
        {
            IPEndPoint endPoint = IPAddress.Loopback.BoundToPort(Port);
            
            using ITwoPartyConnectionListener listener = TcpTwoPartyNetworkSession.CreateListener(SecondParty, endPoint);
            
            Task<TcpTwoPartyNetworkSession> firstSessionTask = TcpTwoPartyNetworkSession.ConnectAsync(FirstParty, endPoint);
            Task<TcpTwoPartyNetworkSession> secondSessionTask = listener.AcceptAsync();

            using TcpTwoPartyNetworkSession firstSession = await firstSessionTask;
            using TcpTwoPartyNetworkSession secondSession = await secondSessionTask;

            firstSession.LocalParty.Should().Be(FirstParty);
            firstSession.RemoteParty.Should().Be(SecondParty);
            
            secondSession.LocalParty.Should().Be(SecondParty);
            secondSession.RemoteParty.Should().Be(FirstParty);
        }
    }
}
