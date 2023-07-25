using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Networking
{
    [TestClass]
    public class TcpMultiPartyNetworkSessionTest
    {
        private const int StartPort = 12684;
        
        private static readonly Party FirstParty = new Party(0);
        private static readonly Party SecondParty = new Party(1);
        private static readonly Party ThirdParty = new Party(2);

        [TestMethod]
        public async Task TestTcpMultiPartyNetworkSession()
        {
            Task<TcpMultiPartyNetworkSession> firstSessionTask = TcpMultiPartyNetworkSession.EstablishLoopbackAsync(FirstParty, StartPort, 3);
            Task<TcpMultiPartyNetworkSession> secondSessionTask = TcpMultiPartyNetworkSession.EstablishLoopbackAsync(FirstParty, StartPort, 3);
            Task<TcpMultiPartyNetworkSession> thirdSessionTask = TcpMultiPartyNetworkSession.EstablishLoopbackAsync(FirstParty, StartPort, 3);

            using TcpMultiPartyNetworkSession firstSession = await firstSessionTask;
            using TcpMultiPartyNetworkSession secondSession = await secondSessionTask;
            using TcpMultiPartyNetworkSession thirdSession = await thirdSessionTask;

            firstSession.LocalParty.Should().Be(FirstParty);
            firstSession.RemotePartySessions.Select(session => session.RemoteParty)
                .Should()
                .Equal(SecondParty, ThirdParty);
            
            secondSession.LocalParty.Should().Be(SecondParty);
            secondSession.RemotePartySessions.Select(session => session.RemoteParty)
                .Should()
                .Equal(FirstParty, ThirdParty);
            
            thirdSession.LocalParty.Should().Be(ThirdParty);
            thirdSession.RemotePartySessions.Select(session => session.RemoteParty)
                .Should()
                .Equal(FirstParty, SecondParty);
        }
    }
}
