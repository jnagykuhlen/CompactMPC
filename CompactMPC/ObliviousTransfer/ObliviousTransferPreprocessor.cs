using System.Security.Cryptography;
using System.Threading.Tasks;
using CompactMPC.Cryptography;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class ObliviousTransferPreprocessor
    {
        private readonly IObliviousTransfer _obliviousTransfer;
        private readonly RandomNumberGenerator _randomNumberGenerator;

        public ObliviousTransferPreprocessor(IObliviousTransfer obliviousTransfer, CryptoContext cryptoContext)
        {
            _obliviousTransfer = obliviousTransfer;
            _randomNumberGenerator = new ThreadSafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
        }

        public Task<PreprocessedSenderBatch> PreprocessSenderAsync(IMessageChannel channel, int numberOfInvocations)
        {
            byte[] randomOptionsBuffer = _randomNumberGenerator.GetBytes(BitQuadrupleArray.RequiredBytes(numberOfInvocations));
            BitQuadrupleArray randomOptions = BitQuadrupleArray.FromBytes(randomOptionsBuffer, numberOfInvocations);

            return _obliviousTransfer.SendAsync(channel, randomOptions, numberOfInvocations)
                .ContinueWith(task => new PreprocessedSenderBatch(randomOptions));
        }

        public Task<PreprocessedReceiverBatch> PreprocessReceiverAsync(IMessageChannel channel, int numberOfInvocations)
        {
            byte[] randomSelectionIndicesBuffer = _randomNumberGenerator.GetBytes(QuadrupleIndexArray.RequiredBytes(numberOfInvocations));
            QuadrupleIndexArray randomSelectionIndices = QuadrupleIndexArray.FromBytes(randomSelectionIndicesBuffer, numberOfInvocations);

            return _obliviousTransfer.ReceiveAsync(channel, randomSelectionIndices, numberOfInvocations)
                .ContinueWith(task => new PreprocessedReceiverBatch(randomSelectionIndices, task.Result));
        }
    }
}
