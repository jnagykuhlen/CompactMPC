using System;
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

        public async Task SendAsync(Stream stream, BitQuadrupleArray options, int numberOfInvocations)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.", nameof(options));

            if (_senderBatch == null || _nextSenderInstanceId + numberOfInvocations > _senderBatch.NumberOfInstances)
                throw new InvalidOperationException("Not enough preprocessed sender data available.");

            byte[] deltaSelectionIndicesBuffer = await stream.ReadAsync(QuadrupleIndexArray.RequiredBytes(numberOfInvocations));
            QuadrupleIndexArray deltaSelectionIndices = QuadrupleIndexArray.FromBytes(deltaSelectionIndicesBuffer, numberOfInvocations);

            BitQuadrupleArray maskedOptionQuadruples = new BitQuadrupleArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                int deltaSelectionIndex = deltaSelectionIndices[i];
                BitQuadruple preprocessedOptions = _senderBatch.GetOptions(_nextSenderInstanceId + i);
                BitQuadruple unmaskedOptions = options[i];
                BitQuadruple maskedOptions = new BitQuadruple(
                    unmaskedOptions[0] ^ preprocessedOptions[(0 + deltaSelectionIndex) % 4],
                    unmaskedOptions[1] ^ preprocessedOptions[(1 + deltaSelectionIndex) % 4],
                    unmaskedOptions[2] ^ preprocessedOptions[(2 + deltaSelectionIndex) % 4],
                    unmaskedOptions[3] ^ preprocessedOptions[(3 + deltaSelectionIndex) % 4]
                );

                maskedOptionQuadruples[i] = maskedOptions;
            }
            
            await stream.WriteAsync(maskedOptionQuadruples.ToBytes());

            _nextSenderInstanceId += numberOfInvocations;
        }

        public async Task<BitArray> ReceiveAsync(Stream stream, QuadrupleIndexArray selectionIndices, int numberOfInvocations)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Provided selection indices must match the specified number of invocations.", nameof(selectionIndices));

            if (_receiverBatch == null || _nextReceiverInstanceId + numberOfInvocations > _receiverBatch.NumberOfInstances)
                throw new InvalidOperationException("Not enough preprocessed receiver data available.");


            QuadrupleIndexArray deltaSelectionIndices = new QuadrupleIndexArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                int deltaSelectionIndex = (_receiverBatch.GetSelectionIndex(_nextReceiverInstanceId + i) - selectionIndices[i] + 4) % 4;
                deltaSelectionIndices[i] = deltaSelectionIndex;
            }

            await stream.WriteAsync(deltaSelectionIndices.ToBytes());

            byte[] maskedOptionQuadruplesBuffer = await stream.ReadAsync(BitQuadrupleArray.RequiredBytes(numberOfInvocations));
            BitQuadrupleArray maskedOptionQuadruples = BitQuadrupleArray.FromBytes(maskedOptionQuadruplesBuffer, numberOfInvocations);

            BitArray selectedBits = new BitArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                BitQuadruple maskedOptions = maskedOptionQuadruples[i];
                selectedBits[i] = maskedOptions[selectionIndices[i]] ^ _receiverBatch.GetSelectedOption(_nextReceiverInstanceId + i);
            }

            _nextReceiverInstanceId += numberOfInvocations;

            return selectedBits;
        }
    }
}
