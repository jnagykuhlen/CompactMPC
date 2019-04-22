using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public class PreprocessedObliviousTransfer : IObliviousTransfer
    {
        private PreprocessedSenderBatch _senderBatch;
        private PreprocessedReceiverBatch _receiverBatch;
        private int _nextSenderInstanceId;
        private int _nextReceiverInstanceId;

        public PreprocessedObliviousTransfer(PreprocessedSenderBatch senderBatch, PreprocessedReceiverBatch receiverBatch)
        {
            _senderBatch = senderBatch;
            _receiverBatch = receiverBatch;
            _nextSenderInstanceId = 0;
            _nextReceiverInstanceId = 0;
        }

        public async Task SendAsync(Stream stream, BitQuadruple[] options, int numberOfInvocations)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.", nameof(options));

            if (_senderBatch == null || _nextSenderInstanceId + numberOfInvocations > _senderBatch.NumberOfInstances)
                throw new InvalidOperationException("Not enough preprocessed sender data available.");

            // TODO: Refactor for more efficient packing

            byte[] packedDeltaSelectionIndices = await stream.ReadAsync(numberOfInvocations);
            byte[] packedMaskedOptions = new byte[numberOfInvocations];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                int deltaSelectionIndex = packedDeltaSelectionIndices[i] % 4;
                BitQuadruple preprocessedOptions = _senderBatch.GetOptions(_nextSenderInstanceId + i);
                BitQuadruple unmaskedOptions = options[i];
                BitQuadruple maskedOptions = new BitQuadruple(
                    unmaskedOptions[0] ^ preprocessedOptions[(0 + deltaSelectionIndex) % 4],
                    unmaskedOptions[1] ^ preprocessedOptions[(1 + deltaSelectionIndex) % 4],
                    unmaskedOptions[2] ^ preprocessedOptions[(2 + deltaSelectionIndex) % 4],
                    unmaskedOptions[3] ^ preprocessedOptions[(3 + deltaSelectionIndex) % 4]
                );

                packedMaskedOptions[i] = maskedOptions.PackedValue;
            }
            
            await stream.WriteAsync(packedMaskedOptions);

            _nextSenderInstanceId += numberOfInvocations;
        }

        public async Task<BitArray> ReceiveAsync(Stream stream, int[] selectionIndices, int numberOfInvocations)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Provided selection indices must match the specified number of invocations.", nameof(selectionIndices));

            if (_receiverBatch == null || _nextReceiverInstanceId + numberOfInvocations > _receiverBatch.NumberOfInstances)
                throw new InvalidOperationException("Not enough preprocessed receiver data available.");


            byte[] packedDeltaSelectionIndices = new byte[numberOfInvocations];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                int deltaSelectionIndex = (_receiverBatch.GetSelectionIndex(_nextReceiverInstanceId + i) - selectionIndices[i] + 4) % 4;
                packedDeltaSelectionIndices[i] = (byte)deltaSelectionIndex;
            }

            await stream.WriteAsync(packedDeltaSelectionIndices);

            byte[] packedMaskedOptions = await stream.ReadAsync(numberOfInvocations);
            BitArray selectedBits = new BitArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                BitQuadruple maskedOptions = BitQuadruple.FromPackedValue(packedMaskedOptions[i]);
                selectedBits[i] = (maskedOptions[selectionIndices[i]] ^ _receiverBatch.GetSelectedOption(_nextReceiverInstanceId + i)).Value;
            }

            _nextReceiverInstanceId += numberOfInvocations;

            return selectedBits;
        }
    }
}
