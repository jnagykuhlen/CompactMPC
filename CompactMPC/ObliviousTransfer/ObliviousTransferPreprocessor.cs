using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CompactMPC.ObliviousTransfer
{
    public class ObliviousTransferPreprocessor
    {
        private IObliviousTransfer _obliviousTransfer;
        private RandomNumberGenerator _randomNumberGenerator;

        public ObliviousTransferPreprocessor(IObliviousTransfer obliviousTransfer, CryptoContext cryptoContext)
        {
            if (obliviousTransfer == null)
                throw new ArgumentNullException(nameof(obliviousTransfer));

            _obliviousTransfer = obliviousTransfer;
            _randomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
        }

        public Task<PreprocessedSenderBatch> PreprocessSenderAsync(Stream stream, int numberOfInvocations)
        {
            byte[] randomOptionsBuffer = _randomNumberGenerator.GetBytes(BitQuadrupleArray.RequiredBytes(numberOfInvocations));
            BitQuadrupleArray randomOptions = BitQuadrupleArray.FromBytes(randomOptionsBuffer, numberOfInvocations);

            return _obliviousTransfer.SendAsync(stream, randomOptions, numberOfInvocations)
                .ContinueWith(task => new PreprocessedSenderBatch(randomOptions));
        }

        public Task<PreprocessedReceiverBatch> PreprocessReceiverAsync(Stream stream, int numberOfInvocations)
        {
            byte[] randomSelectionIndicesBuffer = _randomNumberGenerator.GetBytes(QuadrupleIndexArray.RequiredBytes(numberOfInvocations));
            QuadrupleIndexArray randomSelectionIndices = QuadrupleIndexArray.FromBytes(randomSelectionIndicesBuffer, numberOfInvocations);

            return _obliviousTransfer.ReceiveAsync(stream, randomSelectionIndices, numberOfInvocations)
                .ContinueWith(task => new PreprocessedReceiverBatch(randomSelectionIndices, task.Result));
        }
    }
}
