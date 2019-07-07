using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.ObliviousTransfer;
using CompactMPC.Networking;
using CompactMPC.UnitTests.Util;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class ExtendedObliviousTransferTest
    {
        private static readonly string[] TestOptions = { "Alicia", "Briann" };

        [TestMethod]
        public void TestExtendedObliviousTransfer()
        {
            Task.WhenAll(
                Task.Factory.StartNew(RunObliviousTransferParty1oo2, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(RunObliviousTransferParty1oo2, TaskCreationOptions.LongRunning)
            ).Wait();
        }

        private void RunObliviousTransferParty1oo2()
        {
            const int numberOfInvocations = 3;
            const int numberOfOptions = 2;
            Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
            options = new Pair<byte[]>[]
            {
                new Pair<byte[]>(TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s)).ToArray()),
                new Pair<byte[]>(TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToLower())).ToArray()),
                new Pair<byte[]>(TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToUpper())).ToArray()),
            };

            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                IGeneralizedObliviousTransfer baseOT = new InsecureObliviousTransfer();

                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    IGeneralizedObliviousTransfer obliviousTransfer = new ExtendedObliviousTransfer(
                        baseOT,
                        8,
                        session.Channel,
                        cryptoContext
                    );

                    if (session.LocalParty.Id == 0)
                    {
                        obliviousTransfer.SendAsync(session.Channel, options, numberOfInvocations, 6).Wait();
                    }
                    else
                    {
                        PairIndexArray indices = new PairIndexArray(new[] { 0, 1, 0 });
                        byte[][] results = obliviousTransfer.ReceiveAsync(session.Channel, indices, numberOfInvocations, 6).Result;

                        Assert.IsNotNull(results, "Result is null.");
                        Assert.AreEqual(numberOfInvocations, results.Length, "Result does not match the correct number of invocations.");

                        for (int j = 0; j < numberOfInvocations; ++j)
                        {
                            CollectionAssert.AreEqual(
                                results[j],
                                options[j][indices[j]],
                                "Incorrect message content {0} (should be {1}).",
                                Encoding.ASCII.GetString(results[j]),
                                Encoding.ASCII.GetString(options[j][indices[j]])
                            );
                        }
                    }
                }
            }
        }
    }
}
