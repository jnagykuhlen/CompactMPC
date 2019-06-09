using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.Networking;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class NetworkTest
    {
        private static readonly Party FirstParty = new Party(0, "First");
        private static readonly Party SecondParty = new Party(1, "Second");
        private static readonly Party ThirdParty = new Party(2, "Third");
        private const int Port = 12674;

        [TestMethod]
        public void TestTcpTwoPartyNetworkSession()
        {
            Task<TcpTwoPartyNetworkSession> firstSessionTask = TcpTwoPartyNetworkSession.ConnectAsync(FirstParty, IPAddress.Loopback, Port);
            Task<TcpTwoPartyNetworkSession> secondSessionTask = TcpTwoPartyNetworkSession.AcceptAsync(SecondParty, Port);

            TcpTwoPartyNetworkSession firstSession = firstSessionTask.Result;
            TcpTwoPartyNetworkSession secondSession = secondSessionTask.Result;

            Assert.AreEqual(FirstParty, firstSession.LocalParty);
            Assert.AreEqual(SecondParty, firstSession.RemoteParty);
            Assert.AreEqual(SecondParty, secondSession.LocalParty);
            Assert.AreEqual(FirstParty, secondSession.RemoteParty);

            firstSession.Dispose();
            secondSession.Dispose();
        }

        [TestMethod]
        public void TestTcpMultiPartyNetworkSession()
        {
            Task<TcpMultiPartyNetworkSession> firstSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(FirstParty, IPAddress.Loopback, Port, 3);
            Task<TcpMultiPartyNetworkSession> secondSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(SecondParty, IPAddress.Loopback, Port, 3);
            Task<TcpMultiPartyNetworkSession> thirdSessionTask = TcpMultiPartyNetworkSession.EstablishAsync(ThirdParty, IPAddress.Loopback, Port, 3);

            TcpMultiPartyNetworkSession firstSession = firstSessionTask.Result;
            TcpMultiPartyNetworkSession secondSession = secondSessionTask.Result;
            TcpMultiPartyNetworkSession thirdSession = thirdSessionTask.Result;

            Assert.AreEqual(FirstParty, firstSession.LocalParty);
            Assert.AreEqual(SecondParty, secondSession.LocalParty);
            Assert.AreEqual(ThirdParty, thirdSession.LocalParty);

            CollectionAssert.AreEquivalent(new[] { SecondParty, ThirdParty }, firstSession.RemotePartySessions.Select(session => session.RemoteParty).ToArray());
            CollectionAssert.AreEquivalent(new[] { FirstParty, ThirdParty }, secondSession.RemotePartySessions.Select(session => session.RemoteParty).ToArray());
            CollectionAssert.AreEquivalent(new[] { FirstParty, SecondParty }, thirdSession.RemotePartySessions.Select(session => session.RemoteParty).ToArray());

            firstSessionTask.Dispose();
            secondSessionTask.Dispose();
            thirdSession.Dispose();
        }
    }
}
