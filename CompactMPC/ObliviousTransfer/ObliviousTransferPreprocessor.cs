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
            BitArray randomBits = _randomNumberGenerator.GetBits(4 * numberOfInvocations);
            
            BitQuadruple[] randomOptions = new BitQuadruple[numberOfInvocations];
            for(int j = 0; j < numberOfInvocations; ++j)
            {
                randomOptions[j] = new BitQuadruple
                (
                    randomBits[4 * j + 0],
                    randomBits[4 * j + 1],
                    randomBits[4 * j + 2],
                    randomBits[4 * j + 3]
                );
            }
            
            return _obliviousTransfer.SendAsync(stream, randomOptions, numberOfInvocations)
                .ContinueWith(task => new PreprocessedSenderBatch(randomOptions));
        }

        public Task<PreprocessedReceiverBatch> PreprocessReceiverAsync(Stream stream, int numberOfInvocations)
        {
            byte[] randomBytes = _randomNumberGenerator.GetBytes((numberOfInvocations - 1) / 4 + 1);
            
            int[] randomSelectionIndices = new int[numberOfInvocations];
            for (int j = 0; j < numberOfInvocations; ++j)
            {
                int byteIndex = j / 4;
                int slotIndex = j % 4;

                randomSelectionIndices[j] = (randomBytes[byteIndex] >> 2 * slotIndex) & 3;
            }

            return _obliviousTransfer.ReceiveAsync(stream, randomSelectionIndices, numberOfInvocations)
                .ContinueWith(task => new PreprocessedReceiverBatch(randomSelectionIndices, task.Result));
        }
    }
}
