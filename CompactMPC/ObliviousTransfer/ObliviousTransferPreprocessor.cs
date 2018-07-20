using System;
using System.Collections;
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
        private IBatchObliviousTransfer _batchObliviousTransfer;
        private RandomNumberGenerator _randomNumberGenerator;

        public ObliviousTransferPreprocessor(IBatchObliviousTransfer batchObliviousTransfer, CryptoContext cryptoContext)
        {
            if (batchObliviousTransfer == null)
                throw new ArgumentNullException(nameof(batchObliviousTransfer));

            _batchObliviousTransfer = batchObliviousTransfer;
            _randomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
        }

        public Task<PreprocessedSenderBatch> PreprocessSenderAsync(Stream stream, int numberOfInvocations)
        {
            BitArray randomBits = _randomNumberGenerator.GetBits(4 * numberOfInvocations);
            
            Quadruple<byte[]>[] randomOptions = new Quadruple<byte[]>[numberOfInvocations];
            for(int j = 0; j < numberOfInvocations; ++j)
            {
                randomOptions[j] = new Quadruple<byte[]>
                (
                    new[] { (byte)new Bit(randomBits[4 * j + 0]) },
                    new[] { (byte)new Bit(randomBits[4 * j + 1]) },
                    new[] { (byte)new Bit(randomBits[4 * j + 2]) },
                    new[] { (byte)new Bit(randomBits[4 * j + 3]) }
                );
            }
            
            return _batchObliviousTransfer.SendAsync(stream, randomOptions, 1, numberOfInvocations)
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

            return _batchObliviousTransfer.ReceiveAsync(stream, randomSelectionIndices, 1, numberOfInvocations)
                .ContinueWith(task => new PreprocessedReceiverBatch(randomSelectionIndices, task.Result));
        }
    }
}
