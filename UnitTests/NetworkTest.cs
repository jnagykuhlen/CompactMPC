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
            TestNetworkSession(2, i => new TwoPartyNetworkSession(12348));
        }

        [TestMethod]
        public void TestMultipartyNetworkSession()
        {
            TestNetworkSession(5, i => new MultiPartyNetworkSession(12748, 5, i));
        }

        private void TestNetworkSession(int numberOfParties, Func<int, INetworkSession> sessionFactory)
        {
            Task.WhenAll(
                Enumerable.Range(0, numberOfParties).Select(i => Task.Factory.StartNew(
                    () => RunSessionConsistencyCheck(i, sessionFactory),
                    TaskCreationOptions.LongRunning)
                )
            ).Wait();
        }

        private void RunSessionConsistencyCheck(int partyId, Func<int, INetworkSession> sessionFactory)
        {
            using (INetworkSession session = sessionFactory(partyId))
            {
                foreach (Party remoteParty in session.RemoteParties)
                {
                    IMessageChannel channel = session.GetChannel(remoteParty.Id);
                    channel.WriteMessageAsync(new byte[] { (byte)session.LocalParty.Id, (byte)remoteParty.Id }).Wait();
                    byte[] reply = channel.ReadMessageAsync().Result;

                    Assert.IsTrue(
                        reply[0] == remoteParty.Id && reply[1] == session.LocalParty.Id,
                        "Inconsistent party identifier detected."
                    );
                }
            }
        }
    }
}
