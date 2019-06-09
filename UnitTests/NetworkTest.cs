using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.Networking;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class NetworkTest
    {
        [TestMethod]
        public void TestTwoPartyNetworkSession()
        {
            TestNetworkSession(2, i => TcpTwoPartyNetworkSession.FromPort(12348));
        }

        [TestMethod]
        public void TestMultipartyNetworkSession()
        {
            TestNetworkSession(5, i => new TcpMultiPartyNetworkSession(12748, 5, i));
        }

        private void TestNetworkSession(int numberOfParties, Func<int, IMultiPartyNetworkSession> sessionFactory)
        {
            Task.WhenAll(
                Enumerable.Range(0, numberOfParties).Select(i => Task.Factory.StartNew(
                    () => RunSessionConsistencyCheck(i, sessionFactory),
                    TaskCreationOptions.LongRunning)
                )
            ).Wait();
        }

        private void RunSessionConsistencyCheck(int partyId, Func<int, IMultiPartyNetworkSession> sessionFactory)
        {
            using (IMultiPartyNetworkSession multiPartySession = sessionFactory(partyId))
            {
                foreach (ITwoPartyNetworkSession twoPartySession in multiPartySession.RemotePartySessions)
                {
                    IMessageChannel channel = twoPartySession.Channel;
                    channel.WriteMessageAsync(new byte[] { (byte)multiPartySession.LocalParty.Id, (byte)twoPartySession.RemoteParty.Id }).Wait();
                    byte[] reply = channel.ReadMessageAsync().Result;

                    Assert.IsTrue(
                        reply[0] == twoPartySession.RemoteParty.Id && reply[1] == multiPartySession.LocalParty.Id,
                        "Inconsistent party identifier detected."
                    );
                }
            }
        }
    }
}
