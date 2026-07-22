using System;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer.Preprocessing;

public class PreprocessedObliviousTransfer(PreprocessedSenderBatch senderBatch, PreprocessedReceiverBatch receiverBatch)
    : IBitObliviousTransfer
{
    private int _nextSenderInstanceId;
    private int _nextReceiverInstanceId;

    public async Task SendAsync(IMessageChannel channel, BitQuadrupleArray options, int numberOfInvocations)
    {
        if (options.Length != numberOfInvocations)
            throw new ArgumentException("Provided options must match the specified number of invocations.", nameof(options));

        if (senderBatch == null || _nextSenderInstanceId + numberOfInvocations > senderBatch.NumberOfInstances)
            throw new InvalidOperationException("Not enough preprocessed sender data available.");

        var packedDeltaSelectionIndices = await channel.ReadMessageAsync();
        if (packedDeltaSelectionIndices.Length != QuadrupleIndexArray.RequiredBytes(numberOfInvocations))
            throw new DesynchronizationException("Received incorrect number of delta selection indices.");

        var deltaSelectionIndices =
            QuadrupleIndexArray.FromBytes(packedDeltaSelectionIndices.ToBuffer(), numberOfInvocations);

        var maskedOptionQuadruples = new BitQuadrupleArray(numberOfInvocations);
        for (var i = 0; i < numberOfInvocations; ++i)
        {
            var deltaSelectionIndex = deltaSelectionIndices[i];
            var preprocessedOptions = senderBatch.GetOptions(_nextSenderInstanceId + i);
            var unmaskedOptions = options[i];
            var maskedOptions = new BitQuadruple(
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

        if (receiverBatch == null || _nextReceiverInstanceId + numberOfInvocations > receiverBatch.NumberOfInstances)
            throw new InvalidOperationException("Not enough preprocessed receiver data available.");


        var deltaSelectionIndices = new QuadrupleIndexArray(numberOfInvocations);
        for (var i = 0; i < numberOfInvocations; ++i)
        {
            var deltaSelectionIndex = (receiverBatch.GetSelectionIndex(_nextReceiverInstanceId + i) - selectionIndices[i] + 4) % 4;
            deltaSelectionIndices[i] = deltaSelectionIndex;
        }

        await channel.WriteMessageAsync(new Message(deltaSelectionIndices.ToBytes()));

        var packedMaskedOptionQuadruples = await channel.ReadMessageAsync();
        if (packedMaskedOptionQuadruples.Length != BitQuadrupleArray.RequiredBytes(numberOfInvocations))
            throw new DesynchronizationException("Received incorrect number of masked option quadruples.");

        var maskedOptionQuadruples =
            BitQuadrupleArray.FromBytes(packedMaskedOptionQuadruples.ToBuffer(), numberOfInvocations);

        var selectedBits = new BitArray(numberOfInvocations);
        for (var i = 0; i < numberOfInvocations; ++i)
        {
            var maskedOptions = maskedOptionQuadruples[i];
            selectedBits[i] = maskedOptions[selectionIndices[i]] ^ receiverBatch.GetSelectedOption(_nextReceiverInstanceId + i);
        }

        _nextReceiverInstanceId += numberOfInvocations;

        return selectedBits;
    }
}
