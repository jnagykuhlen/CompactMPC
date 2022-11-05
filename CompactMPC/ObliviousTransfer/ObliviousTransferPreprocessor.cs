using System.Threading.Tasks;
using CompactMPC.Cryptography;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class ObliviousTransferPreprocessor
    {
        private readonly IBitObliviousTransfer _obliviousTransfer;

        public ObliviousTransferPreprocessor(IBitObliviousTransfer obliviousTransfer)
        {
            _obliviousTransfer = obliviousTransfer;
        }

        public Task<PreprocessedSenderBatch> PreprocessSenderAsync(IMessageChannel channel, int numberOfInvocations)
        {
            byte[] randomOptionsBuffer = RandomNumberGenerator.GetBytes(BitQuadrupleArray.RequiredBytes(numberOfInvocations));
            BitQuadrupleArray randomOptions = BitQuadrupleArray.FromBytes(randomOptionsBuffer, numberOfInvocations);

            return _obliviousTransfer.SendAsync(channel, randomOptions, numberOfInvocations)
                .ContinueWith(task => new PreprocessedSenderBatch(randomOptions));
        }

        public Task<PreprocessedReceiverBatch> PreprocessReceiverAsync(IMessageChannel channel, int numberOfInvocations)
        {
            byte[] randomSelectionIndicesBuffer = RandomNumberGenerator.GetBytes(QuadrupleIndexArray.RequiredBytes(numberOfInvocations));
            QuadrupleIndexArray randomSelectionIndices = QuadrupleIndexArray.FromBytes(randomSelectionIndicesBuffer, numberOfInvocations);

            return _obliviousTransfer.ReceiveAsync(channel, randomSelectionIndices, numberOfInvocations)
                .ContinueWith(task => new PreprocessedReceiverBatch(randomSelectionIndices, task.Result));
        }
    }
}
