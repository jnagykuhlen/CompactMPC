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
    public class CorrelatedObliviousTransferTest
    {
        private const int NumberOfMessageBytes = 3;
        private const int NumberOfInvocations = 6;

        [TestMethod]
        public void TestCorreclatedObliviousTransfer()
        {
            BitArray selectionBits = BitArray.FromBinaryString("011001");
            byte[][] correlationStrings = new byte[][] {
                BitArray.FromBinaryString("100111010000110101001001").ToBytes(),
                BitArray.FromBinaryString("010010111000101001010100").ToBytes(),
                BitArray.FromBinaryString("000011110000111100001111").ToBytes(),
                BitArray.FromBinaryString("000000001111111100001111").ToBytes(),
                BitArray.FromBinaryString("001100110011001100110011").ToBytes(),
                BitArray.FromBinaryString("010101010101010101010101").ToBytes()
            };


            Task<byte[][]> receiverTask = Task.Factory.StartNew(() => RunObliviousTransferReceiverParty(selectionBits), TaskCreationOptions.LongRunning);
            Task<Pair<byte[]>[]> senderTask = Task.Factory.StartNew(() => RunObliviousTransferSenderParty(correlationStrings), TaskCreationOptions.LongRunning);

            Task.WhenAll(
                receiverTask,
                senderTask
            ).Wait();

            byte[][] receiverResult = receiverTask.Result;
            Pair<byte[]>[] senderResult = senderTask.Result;
            Assert.AreEqual(NumberOfInvocations, senderResult.Length, "Sender did not return options for all invocations.");
            Assert.AreEqual(NumberOfInvocations, receiverResult.Length, "Receiver did not return results for all invocations.");

            for (int i = 0; i < NumberOfInvocations; ++i)
            {
                Assert.AreEqual(NumberOfMessageBytes, senderResult[i][0].Length, "First sender option does not have the expected length.");
                Assert.AreEqual(NumberOfMessageBytes, senderResult[i][1].Length, "Second sender option does not have the expected length.");
                byte[] expected = senderResult[i][Convert.ToInt32(selectionBits[i].Value)];
                CollectionAssert.AreEqual(
                    expected,
                    receiverResult[i],
                    "Incorrect message content {0} (should be {1}).",
                    BitArray.FromBytes(receiverResult[i], 8 * NumberOfMessageBytes).ToBinaryString(),
                    BitArray.FromBytes(expected, 8 * NumberOfMessageBytes).ToBinaryString()
                );
                byte[] resultXor = (BitArray.FromBytes(senderResult[i][0], 8 * NumberOfMessageBytes) ^ BitArray.FromBytes(senderResult[i][1], 8 * NumberOfMessageBytes)).ToBytes();
                CollectionAssert.AreEqual(
                    correlationStrings[i],
                    resultXor,
                    "Sender options are not correctly correlated. Xor is {0}, should be {1} (options: {2}, {3})",
                    BitArray.FromBytes(resultXor, 8 * NumberOfMessageBytes).ToBinaryString(),
                    BitArray.FromBytes(correlationStrings[i], 8 * NumberOfMessageBytes).ToBinaryString(),
                    BitArray.FromBytes(senderResult[i][0], 8 * NumberOfMessageBytes).ToBinaryString(),
                    BitArray.FromBytes(senderResult[i][1], 8 * NumberOfMessageBytes).ToBinaryString()
                );
            }
        }

        [TestMethod]
        public void TestResumedRandomObliviousTransfer()
        {
            BitArray[] roundSelectionBits = new[] { BitArray.FromBinaryString("011010"), BitArray.FromBinaryString("101101") };
            byte[][][] roundCorrelationStrings = new byte[][][] {
                new byte[][] {
                    BitArray.FromBinaryString("100111010000110101001001").ToBytes(),
                    BitArray.FromBinaryString("010010111000101001010100").ToBytes(),
                    BitArray.FromBinaryString("000011110000111100001111").ToBytes(),
                    BitArray.FromBinaryString("000000001111111100001111").ToBytes(),
                    BitArray.FromBinaryString("001100110011001100110011").ToBytes(),
                    BitArray.FromBinaryString("010101010101010101010101").ToBytes()
                },
                new byte[][]
                {
                    BitArray.FromBinaryString("011010011000010101011100").ToBytes(),
                    BitArray.FromBinaryString("101000100100100001111001").ToBytes(),
                    BitArray.FromBinaryString("100110100011101010010101").ToBytes(),
                    BitArray.FromBinaryString("011001111001100100011110").ToBytes(),
                    BitArray.FromBinaryString("000101110100101010010111").ToBytes(),
                    BitArray.FromBinaryString("000011111010001100011110").ToBytes()
                }
            };

            Task<Pair<byte[][]>> receiverTask = Task.Factory.StartNew(
                () => RunResumedObliviousTransferReceiverParty(roundSelectionBits[0], roundSelectionBits[1]), TaskCreationOptions.LongRunning);
            Task<Pair<Pair<byte[]>[]>> senderTask = Task.Factory.StartNew(
                () => RunResumedObliviousTransferSenderParty(roundCorrelationStrings[0], roundCorrelationStrings[1]), TaskCreationOptions.LongRunning);

            Task.WhenAll(
                receiverTask,
                senderTask
            ).Wait();

            Pair<byte[][]> resumedReceiverResult = receiverTask.Result;
            Pair<Pair<byte[]>[]> resumedSenderResult = senderTask.Result;
            for (int j = 0; j < 2; ++j)
            {
                BitArray selectionBits = roundSelectionBits[j];
                byte[][] correlationStrings = roundCorrelationStrings[j];

                byte[][] receiverResult = resumedReceiverResult[j];
                Pair<byte[]>[] senderResult = resumedSenderResult[j];

                Assert.AreEqual(NumberOfInvocations, senderResult.Length, "Sender did not return options for all invocations.");
                Assert.AreEqual(NumberOfInvocations, receiverResult.Length, "Receiver did not return results for all invocations.");

                for (int i = 0; i < NumberOfInvocations; ++i)
                {
                    Assert.AreEqual(NumberOfMessageBytes, senderResult[i][0].Length, "First sender option does not have the expected length.");
                    Assert.AreEqual(NumberOfMessageBytes, senderResult[i][1].Length, "Second sender option does not have the expected length.");
                    byte[] expected = senderResult[i][Convert.ToInt32(selectionBits[i].Value)];
                    CollectionAssert.AreEqual(
                        expected,
                        receiverResult[i],
                        "Incorrect message content {0} (should be {1}).",
                        BitArray.FromBytes(receiverResult[i], 8 * NumberOfMessageBytes).ToBinaryString(),
                        BitArray.FromBytes(expected, 8 * NumberOfMessageBytes).ToBinaryString()
                    );
                    byte[] resultXor = (BitArray.FromBytes(senderResult[i][0], 8 * NumberOfMessageBytes) ^ BitArray.FromBytes(senderResult[i][1], 8 * NumberOfMessageBytes)).ToBytes();
                    CollectionAssert.AreEqual(
                        correlationStrings[i],
                        resultXor,
                        "Sender options are not correctly correlated. Xor is {0}, should be {1} (options: {2}, {3})",
                        BitArray.FromBytes(resultXor, 8 * NumberOfMessageBytes).ToBinaryString(),
                        BitArray.FromBytes(correlationStrings[i], 8 * NumberOfMessageBytes).ToBinaryString(),
                        BitArray.FromBytes(senderResult[i][0], 8 * NumberOfMessageBytes).ToBinaryString(),
                        BitArray.FromBytes(senderResult[i][1], 8 * NumberOfMessageBytes).ToBinaryString()
                    );
                }
            }
        }

        private byte[][] RunObliviousTransferReceiverParty(BitArray selectionBits)
        {
            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    ITwoChoicesObliviousTransferChannel baseOT = new StatelessTwoChoicesObliviousObliviousTransferChannel(new InsecureObliviousTransfer(), session.Channel);
                    ITwoChoicesCorrelatedObliviousTransferChannel obliviousTransfer = new TwoChoicesCorrelatedExtendedObliviousTransferChannel(baseOT, 8, cryptoContext);

                    return obliviousTransfer.ReceiveAsync(selectionBits, selectionBits.Length, NumberOfMessageBytes).Result;
                }
            }
        }

        private Pair<byte[]>[] RunObliviousTransferSenderParty(byte[][] correlationStrings)
        {
            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    ITwoChoicesObliviousTransferChannel baseOT = new StatelessTwoChoicesObliviousObliviousTransferChannel(new InsecureObliviousTransfer(), session.Channel);
                    ITwoChoicesCorrelatedObliviousTransferChannel obliviousTransfer = new TwoChoicesCorrelatedExtendedObliviousTransferChannel(baseOT, 8, cryptoContext);

                    return obliviousTransfer.SendAsync(
                        correlationStrings, correlationStrings.Length, NumberOfMessageBytes).Result;
                }
            }
        }

        private Pair<byte[][]> RunResumedObliviousTransferReceiverParty(BitArray firstRoundSelectionBits, BitArray secondRoundSelectionBits)
        {
            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    ITwoChoicesObliviousTransferChannel baseOT = new StatelessTwoChoicesObliviousObliviousTransferChannel(new InsecureObliviousTransfer(), session.Channel);
                    ITwoChoicesCorrelatedObliviousTransferChannel obliviousTransfer = new TwoChoicesCorrelatedExtendedObliviousTransferChannel(baseOT, 8, cryptoContext);

                    byte[][] firstRoundResults = obliviousTransfer.ReceiveAsync(
                        firstRoundSelectionBits, firstRoundSelectionBits.Length, NumberOfMessageBytes).Result;
                    byte[][] secondRoundResults = obliviousTransfer.ReceiveAsync(
                        secondRoundSelectionBits, secondRoundSelectionBits.Length, NumberOfMessageBytes).Result;
                    return new Pair<byte[][]>(firstRoundResults, secondRoundResults);
                }
            }
        }

        private Pair<Pair<byte[]>[]> RunResumedObliviousTransferSenderParty(byte[][] firstCorrelationStrings, byte[][] secondCorrelationStrings)
        {
            using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
            {
                using (ITwoPartyNetworkSession session = TestNetworkSession.EstablishTwoParty())
                {
                    ITwoChoicesObliviousTransferChannel baseOT = new StatelessTwoChoicesObliviousObliviousTransferChannel(new InsecureObliviousTransfer(), session.Channel);
                    ITwoChoicesCorrelatedObliviousTransferChannel obliviousTransfer = new TwoChoicesCorrelatedExtendedObliviousTransferChannel(baseOT, 8, cryptoContext);

                    Pair<byte[]>[] firstRoundResults = obliviousTransfer.SendAsync(
                        firstCorrelationStrings, firstCorrelationStrings.Length, NumberOfMessageBytes).Result;
                    Pair<byte[]>[] secondRoundResults = obliviousTransfer.SendAsync(
                        secondCorrelationStrings, secondCorrelationStrings.Length, NumberOfMessageBytes).Result;
                    return new Pair<Pair<byte[]>[]>(firstRoundResults, secondRoundResults);
                }
            }
        }
    }
}
