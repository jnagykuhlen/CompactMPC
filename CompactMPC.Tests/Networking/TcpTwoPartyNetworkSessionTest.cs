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

        private static Task<TcpTwoPartyNetworkSession> CreateFirstTwoPartySessionAsync()
        {
            return TcpTwoPartyNetworkSession.ConnectLoopbackAsync(FirstParty, Port);
        }

        private static Task<TcpTwoPartyNetworkSession> CreateSecondTwoPartySessionAsync()
        {
            return TcpTwoPartyNetworkSession.AcceptLoopbackAsync(SecondParty, Port);
        }
    }
}
