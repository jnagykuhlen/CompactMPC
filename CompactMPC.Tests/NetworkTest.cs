using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Networking;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class NetworkTest
    {
        private const int Port = 12674;
        
        private static readonly Party FirstParty = new Party(0, "First");
        private static readonly Party SecondParty = new Party(1, "Second");
        private static readonly Party ThirdParty = new Party(2, "Third");

        [TestMethod]
        public void TestTcpTwoPartyNetworkSession()
        {
            Task<TcpTwoPartyNetworkSession> firstSessionTask = CreateFirstTwoPartySessionAsync();
            Task<TcpTwoPartyNetworkSession> secondSessionTask = CreateSecondTwoPartySessionAsync();

            using TcpTwoPartyNetworkSession firstSession = firstSessionTask.Result;
            using TcpTwoPartyNetworkSession secondSession = secondSessionTask.Result;

            firstSession.LocalParty.Should().Be(FirstParty);
            firstSession.RemoteParty.Should().Be(SecondParty);
            
            secondSession.LocalParty.Should().Be(SecondParty);
            secondSession.RemoteParty.Should().Be(FirstParty);
        }

        [TestMethod]
        public void TestTcpMultiPartyNetworkSession()
        {
            Task<TcpMultiPartyNetworkSession> firstSessionTask = CreateMultiPartySessionAsync(FirstParty, 3);
            Task<TcpMultiPartyNetworkSession> secondSessionTask = CreateMultiPartySessionAsync(SecondParty, 3);
            Task<TcpMultiPartyNetworkSession> thirdSessionTask = CreateMultiPartySessionAsync(ThirdParty, 3);

            using TcpMultiPartyNetworkSession firstSession = firstSessionTask.Result;
            using TcpMultiPartyNetworkSession secondSession = secondSessionTask.Result;
            using TcpMultiPartyNetworkSession thirdSession = thirdSessionTask.Result;

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

        private static Task<TcpTwoPartyNetworkSession> CreateFirstTwoPartySessionAsync()
        {
            return TcpTwoPartyNetworkSession.ConnectLoopbackAsync(FirstParty, Port);
        }

        private static Task<TcpTwoPartyNetworkSession> CreateSecondTwoPartySessionAsync()
        {
            return TcpTwoPartyNetworkSession.AcceptLoopbackAsync(SecondParty, Port);
        }

        private static Task<TcpMultiPartyNetworkSession> CreateMultiPartySessionAsync(Party party, int numberOfParties)
        {
            return TcpMultiPartyNetworkSession.EstablishLoopbackAsync(party, Port, numberOfParties);
        }
    }
}
