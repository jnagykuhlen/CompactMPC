using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.UnitTests.Assertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests
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

            Assert.AreEqual(FirstParty, firstSession.LocalParty);
            Assert.AreEqual(SecondParty, firstSession.RemoteParty);
            Assert.AreEqual(SecondParty, secondSession.LocalParty);
            Assert.AreEqual(FirstParty, secondSession.RemoteParty);
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

            Assert.AreEqual(FirstParty, firstSession.LocalParty);
            Assert.AreEqual(SecondParty, secondSession.LocalParty);
            Assert.AreEqual(ThirdParty, thirdSession.LocalParty);

            EnumerableAssert.AreEquivalent(new[] { SecondParty, ThirdParty }, GetRemoteParties(firstSession));
            EnumerableAssert.AreEquivalent(new[] { FirstParty, ThirdParty }, GetRemoteParties(secondSession));
            EnumerableAssert.AreEquivalent(new[] { FirstParty, SecondParty }, GetRemoteParties(thirdSession));
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

        private static IEnumerable<Party> GetRemoteParties(IMultiPartyNetworkSession multiPartySession)
        {
            return multiPartySession.RemotePartySessions.Select(session => session.RemoteParty);
        }
    }
}
