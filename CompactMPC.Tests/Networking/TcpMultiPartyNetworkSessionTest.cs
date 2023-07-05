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
        private static readonly IPEndPoint[] EndPoints = {
            IPAddress.Loopback.BoundToPort(12680),
            IPAddress.Loopback.BoundToPort(12681),
            IPAddress.Loopback.BoundToPort(12682)
        };
        
        private static readonly Party FirstParty = new Party(0);
        private static readonly Party SecondParty = new Party(1);
        private static readonly Party ThirdParty = new Party(2);

        [TestMethod]
        public async Task TestTcpMultiPartyNetworkSession()
        {
            Task<TcpMultiPartyNetworkSession> firstSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(FirstParty, EndPoints);
            Task<TcpMultiPartyNetworkSession> secondSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(SecondParty, EndPoints);
            Task<TcpMultiPartyNetworkSession> thirdSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(ThirdParty, EndPoints);

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
