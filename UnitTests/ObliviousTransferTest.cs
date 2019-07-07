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
    public class ObliviousTransferTest
    {
        private static readonly string[] TestOptions = { "Alicia", "Briann", "Charly", "Dennis" };

        [TestMethod]
        public void TestNaorPinkasObliviousTransfer1oo4()
        {
            Task.WhenAll(
                Task.Factory.StartNew(RunObliviousTransferParty1oo4, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(RunObliviousTransferParty1oo4, TaskCreationOptions.LongRunning)
            ).Wait();
        }

        private void RunObliviousTransferParty1oo4()
        {
            const int numberOfInvocations = 3;
            const int numberOfOptions = 4;
            Quadruple<byte[]>[] options = new Quadruple<byte[]>[numberOfInvocations];
            options = new Quadruple<byte[]>[]
            {
                new Quadruple<byte[]>(TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s)).ToArray()),
                new Quadruple<byte[]>(TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToLower())).ToArray()),
                new Quadruple<byte[]>(TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToUpper())).ToArray()),
            };

            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                IGeneralizedObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                    SecurityParameters.CreateDefault768Bit(),
                    cryptoContext
                );

                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    if (session.LocalParty.Id == 0)
                    {
                        obliviousTransfer.SendAsync(session.Channel, options, numberOfInvocations, 6).Wait();
                    }
                    else
                    {
                        QuadrupleIndexArray indices = new QuadrupleIndexArray(new[] { 0, 3, 2 });
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
