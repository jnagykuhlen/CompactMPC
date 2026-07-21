using System.Threading.Tasks;
using CompactMPC.Circuits;
using CompactMPC.Networking;

namespace CompactMPC.Protocol.Internal;

public class SecretSharingAsyncBatchCircuitEvaluator(IMultiPartyNetworkSession session, IMultiplicativeSharing multiplicativeSharing)
    : IAsyncBatchCircuitEvaluator<Bit>
{
    public async Task<Bit[]> EvaluateAndGateBatchAsync(GateEvaluationInput<Bit>[] evaluationInputs)
    {
        var numberOfInvocations = evaluationInputs.Length;

        var leftShares = new BitArray(numberOfInvocations);
        var rightShares = new BitArray(numberOfInvocations);
        for (var i = 0; i < numberOfInvocations; ++i)
        {
            leftShares[i] = evaluationInputs[i].LeftValue;
            rightShares[i] = evaluationInputs[i].RightValue;
        }

        var multiplicativeShares =
            await multiplicativeSharing.ComputeMultiplicativeSharesAsync(session, leftShares, rightShares, numberOfInvocations);

        // TODO: Conversion to array necessary?
        return multiplicativeShares.ToArray();
    }

    public Bit EvaluateXorGate(Bit leftValue, Bit rightValue) => leftValue ^ rightValue;
    public Bit EvaluateNotGate(Bit value) => session.LocalParty.IsFirstParty() ? ~value : value;
}
