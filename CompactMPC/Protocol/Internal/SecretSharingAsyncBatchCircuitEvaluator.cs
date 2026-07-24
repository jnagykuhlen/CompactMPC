using System.Collections.Generic;
using System.Threading.Tasks;
using CompactMPC.Circuits;
using CompactMPC.Networking;

namespace CompactMPC.Protocol.Internal;

public class SecretSharingAsyncBatchCircuitEvaluator(OrderedMultiPartyNetworkSession session, IMultiplicativeSharing multiplicativeSharing)
    : IAsyncBatchCircuitEvaluator<Bit>
{
    public async Task<IReadOnlyList<Bit>> EvaluateAndGateBatchAsync(IReadOnlyList<GateEvaluationInput<Bit>> evaluationInputs)
    {
        var numberOfInvocations = evaluationInputs.Count;

        var leftShares = new BitArray(numberOfInvocations);
        var rightShares = new BitArray(numberOfInvocations);
        for (var i = 0; i < numberOfInvocations; ++i)
        {
            leftShares[i] = evaluationInputs[i].LeftValue;
            rightShares[i] = evaluationInputs[i].RightValue;
        }

        return await multiplicativeSharing.ComputeMultiplicativeSharesAsync(session, leftShares, rightShares, numberOfInvocations);
    }

    public Bit EvaluateXorGate(Bit leftValue, Bit rightValue) => leftValue ^ rightValue;
    public Bit EvaluateNotGate(Bit value) => session.IsLocalPartyLeading ? ~value : value;
}
