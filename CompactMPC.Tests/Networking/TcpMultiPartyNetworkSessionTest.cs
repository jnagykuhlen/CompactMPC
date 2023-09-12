using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Networking
{
    [TestClass]
    public class TcpMultiPartyNetworkSessionTest
    {
        private static readonly IPEndPoint FirstEndPoint = new IPEndPoint(IPAddress.Loopback, 12840);
        private static readonly IPEndPoint SecondEndPoint = new IPEndPoint(IPAddress.Loopback, 12841);
        private static readonly IPEndPoint ThirdEndPoint = new IPEndPoint(IPAddress.Loopback, 12842);
        
        private static readonly Party FirstParty = new Party(0);
        private static readonly Party SecondParty = new Party(1);
        private static readonly Party ThirdParty = new Party(2);

        [TestMethod]
        public async Task TestTcpMultiPartyNetworkSession()
        {
            Task<TcpMultiPartyNetworkSession> firstSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(FirstParty, FirstEndPoint, new[] { SecondEndPoint, ThirdEndPoint });
            Task<TcpMultiPartyNetworkSession> secondSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(SecondParty, SecondEndPoint, new[] { FirstEndPoint, ThirdEndPoint });
            Task<TcpMultiPartyNetworkSession> thirdSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(ThirdParty, ThirdEndPoint, new[] { FirstEndPoint, SecondEndPoint });

            using TcpMultiPartyNetworkSession firstSession = await firstSessionTask;
            using TcpMultiPartyNetworkSession secondSession = await secondSessionTask;
            using TcpMultiPartyNetworkSession thirdSession = await thirdSessionTask;

            firstSession.LocalParty.Should().Be(FirstParty);
            firstSession.RemotePartySessions.Select(session => session.RemoteParty)
                .Should()
                .BeEquivalentTo(SecondParty, ThirdParty);
            
            secondSession.LocalParty.Should().Be(SecondParty);
            secondSession.RemotePartySessions.Select(session => session.RemoteParty)
                .Should()
                .BeEquivalentTo(FirstParty, ThirdParty);
            
            thirdSession.LocalParty.Should().Be(ThirdParty);
            thirdSession.RemotePartySessions.Select(session => session.RemoteParty)
                .Should()
                .BeEquivalentTo(FirstParty, SecondParty);
        }
    }
}
