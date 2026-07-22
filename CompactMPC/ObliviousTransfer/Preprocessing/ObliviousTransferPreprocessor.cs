using System.Threading.Tasks;
using CompactMPC.Cryptography;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer.Preprocessing;

public class ObliviousTransferPreprocessor(IBitObliviousTransfer obliviousTransfer)
{
    public Task<PreprocessedSenderBatch> PreprocessSenderAsync(IMessageChannel channel, int numberOfInvocations)
    {
        var randomOptionsBuffer = RandomNumberGenerator.GetBytes(BitQuadrupleArray.RequiredBytes(numberOfInvocations));
        var randomOptions = BitQuadrupleArray.FromBytes(randomOptionsBuffer, numberOfInvocations);

        return obliviousTransfer.SendAsync(channel, randomOptions, numberOfInvocations)
            .ContinueWith(_ => new PreprocessedSenderBatch(randomOptions));
    }

    public Task<PreprocessedReceiverBatch> PreprocessReceiverAsync(IMessageChannel channel, int numberOfInvocations)
    {
        var randomSelectionIndicesBuffer = RandomNumberGenerator.GetBytes(QuadrupleIndexArray.RequiredBytes(numberOfInvocations));
        var randomSelectionIndices = QuadrupleIndexArray.FromBytes(randomSelectionIndicesBuffer, numberOfInvocations);

        return obliviousTransfer.ReceiveAsync(channel, randomSelectionIndices, numberOfInvocations)
            .ContinueWith(task => new PreprocessedReceiverBatch(randomSelectionIndices, task.Result));
    }
}
