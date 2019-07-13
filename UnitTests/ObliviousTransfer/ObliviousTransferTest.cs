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
    public class ObliviousTransferTest
    {
        // todo(lumip): these tests are mostly copy+paste right now. should be streamlined more. also, it would be
        //  sufficient to test that the wrapper functions of GeneralizedObliviousTransfer (1oo2, 1oo4) forward correctly
        //  and have only one test for the real 1ooN OT implementation

        private static readonly string[] TestOptions = { "Alicia", "Briann", "Charly", "Dennis", "Elenor", "Frieda" };

        [TestMethod]
        public void TestInsecureObliviousTransfer1oo2()
        {
            Task.WhenAll(
                Task.Factory.StartNew(RunInsecureObliviousTransferParty1oo2, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(RunInsecureObliviousTransferParty1oo2, TaskCreationOptions.LongRunning)
            ).Wait();
        }

        [TestMethod]
        public void TestInsecureObliviousTransfer1oo6()
        {
            Task.WhenAll(
                Task.Factory.StartNew(RunInsecureObliviousTransferParty1oo6, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(RunInsecureObliviousTransferParty1oo6, TaskCreationOptions.LongRunning)
            ).Wait();
        }

        [TestMethod]
        public void TestNaorPinkasObliviousTransfer1oo2()
        {
            Task.WhenAll(
                Task.Factory.StartNew(RunObliviousTransferParty1oo2, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(RunObliviousTransferParty1oo2, TaskCreationOptions.LongRunning)
            ).Wait();
        }

        [TestMethod]
        public void TestNaorPinkasObliviousTransfer1oo4()
        {
            Task.WhenAll(
                Task.Factory.StartNew(RunObliviousTransferParty1oo4, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(RunObliviousTransferParty1oo4, TaskCreationOptions.LongRunning)
            ).Wait();
        }

        [TestMethod]
        public void TestNaorPinkasObliviousTransfer1oo6()
        {
            Task.WhenAll(
                Task.Factory.StartNew(RunObliviousTransferParty1oo4, TaskCreationOptions.LongRunning),
                Task.Factory.StartNew(RunObliviousTransferParty1oo4, TaskCreationOptions.LongRunning)
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

        private void RunObliviousTransferParty1oo6()
        {
            const int numberOfInvocations = 4;
            const int numberOfOptions = 6;
            byte[][][] options = new byte[][][]
            {
                TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s)).ToArray(),
                TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToLower())).ToArray(),
                TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToUpper())).ToArray(),
                TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToUpper())).ToArray(),
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
                        obliviousTransfer.SendAsync(session.Channel, options, numberOfOptions, numberOfInvocations, 6).Wait();
                    }
                    else
                    {
                        int[] indices = new[] { 4, 1, 3, 5 };
                        byte[][] results = obliviousTransfer.ReceiveAsync(session.Channel, indices, numberOfOptions, numberOfInvocations, 6).Result;

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

        private void RunInsecureObliviousTransferParty1oo2()
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
                IGeneralizedObliviousTransfer obliviousTransfer = new InsecureObliviousTransfer();

                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
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

        private void RunInsecureObliviousTransferParty1oo6()
        {
            const int numberOfInvocations = 4;
            const int numberOfOptions = 6;
            byte[][][] options = new byte[][][]
            {
                TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s)).ToArray(),
                TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToLower())).ToArray(),
                TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToUpper())).ToArray(),
                TestOptions.Take(numberOfOptions).Select(s => Encoding.ASCII.GetBytes(s.ToUpper())).ToArray(),
            };

            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                IGeneralizedObliviousTransfer obliviousTransfer = new InsecureObliviousTransfer();

                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    if (session.LocalParty.Id == 0)
                    {
                        obliviousTransfer.SendAsync(session.Channel, options, numberOfOptions, numberOfInvocations, 6).Wait();
                    }
                    else
                    {
                        int[] indices = new[] { 4, 1, 3, 5 };
                        byte[][] results = obliviousTransfer.ReceiveAsync(session.Channel, indices, numberOfOptions, numberOfInvocations, 6).Result;

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
