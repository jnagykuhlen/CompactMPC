using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.ObliviousTransfer;
using CompactMPC.Networking;
using CompactMPC.UnitTests.Util;

namespace CompactMPC.UnitTests.ObliviousTransfer
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

        [TestMethod]
        public void TestResumedExtendedObliviousTransfer()
        {
            Task.WhenAll(
                Task.Factory.StartNew(RunObliviousTransferParty1oo2Resumed, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(RunObliviousTransferParty1oo2Resumed, TaskCreationOptions.LongRunning)
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
                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    ITwoChoicesObliviousTransferChannel baseOT = new StatelessTwoChoiceObliviousObliviousTransferChannelAdapter(new InsecureObliviousTransfer(), session.Channel);
                    ITwoChoicesObliviousTransferChannel obliviousTransfer = new TwoChoicesExtendedObliviousTransferChannel(baseOT, 8, cryptoContext);

                    if (session.LocalParty.Id == 0)
                    {
                        obliviousTransfer.SendAsync(options, numberOfInvocations, 6).Wait();
                    }
                    else
                    {
                        PairIndexArray indices = new PairIndexArray(new[] { 0, 1, 0 });
                        byte[][] results = obliviousTransfer.ReceiveAsync(indices, numberOfInvocations, 6).Result;

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

        private void RunObliviousTransferParty1oo2Resumed()
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
                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    ITwoChoicesObliviousTransferChannel baseOT = new StatelessTwoChoiceObliviousObliviousTransferChannelAdapter(new InsecureObliviousTransfer(), session.Channel);
                    ITwoChoicesObliviousTransferChannel obliviousTransfer = new TwoChoicesExtendedObliviousTransferChannel(baseOT, 8, cryptoContext);

                    if (session.LocalParty.Id == 0)
                    {
                        for (int i = 0; i < 2; ++i)
                            obliviousTransfer.SendAsync(options, numberOfInvocations, 6).Wait();
                    }
                    else
                    {
                        PairIndexArray[] allIndices = new[] { new PairIndexArray(new[] { 0, 1, 0 }), new PairIndexArray(new[] { 1, 0, 0 }) };
                        for (int i = 0; i < 2; ++i)
                        {
                            PairIndexArray indices = allIndices[i];
                            byte[][] results = obliviousTransfer.ReceiveAsync(indices, numberOfInvocations, 6).Result;

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
}
