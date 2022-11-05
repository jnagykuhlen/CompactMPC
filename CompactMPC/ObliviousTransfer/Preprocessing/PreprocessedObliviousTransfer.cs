using System;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer.Preprocessing
{
    public class PreprocessedObliviousTransfer : IBitObliviousTransfer
    {
        private readonly PreprocessedSenderBatch _senderBatch;
        private readonly PreprocessedReceiverBatch _receiverBatch;
        private int _nextSenderInstanceId;
        private int _nextReceiverInstanceId;

        public PreprocessedObliviousTransfer(PreprocessedSenderBatch senderBatch, PreprocessedReceiverBatch receiverBatch)
        {
            _senderBatch = senderBatch;
            _receiverBatch = receiverBatch;
            _nextSenderInstanceId = 0;
            _nextReceiverInstanceId = 0;
        }

        public async Task SendAsync(IMessageChannel channel, BitQuadrupleArray options, int numberOfInvocations)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.", nameof(options));

            if (_senderBatch == null || _nextSenderInstanceId + numberOfInvocations > _senderBatch.NumberOfInstances)
                throw new InvalidOperationException("Not enough preprocessed sender data available.");

            Message packedDeltaSelectionIndices = await channel.ReadMessageAsync();
            if (packedDeltaSelectionIndices.Length != QuadrupleIndexArray.RequiredBytes(numberOfInvocations))
                throw new DesynchronizationException("Received incorrect number of delta selection indices.");

            QuadrupleIndexArray deltaSelectionIndices =
                QuadrupleIndexArray.FromBytes(packedDeltaSelectionIndices.ToBuffer(), numberOfInvocations);

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
            
            await channel.WriteMessageAsync(new Message(maskedOptionQuadruples.ToBytes()));

            _nextSenderInstanceId += numberOfInvocations;
        }

        public async Task<BitArray> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations)
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

            await channel.WriteMessageAsync(new Message(deltaSelectionIndices.ToBytes()));

            Message packedMaskedOptionQuadruples = await channel.ReadMessageAsync();
            if (packedMaskedOptionQuadruples.Length != BitQuadrupleArray.RequiredBytes(numberOfInvocations))
                throw new DesynchronizationException("Received incorrect number of masked option quadruples.");

            BitQuadrupleArray maskedOptionQuadruples =
                BitQuadrupleArray.FromBytes(packedMaskedOptionQuadruples.ToBuffer(), numberOfInvocations);

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
