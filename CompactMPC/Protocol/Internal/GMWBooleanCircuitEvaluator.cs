using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;

namespace CompactMPC.Protocol.Internal
{
    public class GMWBooleanCircuitEvaluator : IBatchCircuitEvaluator<Task<Bit>>
    {
        private IMultiPartyNetworkSession _session;
        private IMultiplicativeSharing _multiplicativeSharing;
        
        public GMWBooleanCircuitEvaluator(IMultiPartyNetworkSession session, IMultiplicativeSharing multiplicativeSharing)
        {
            _session = session;
            _multiplicativeSharing = multiplicativeSharing;
        }

        public Task<Bit>[] EvaluateAndGateBatch(GateEvaluationInput<Task<Bit>>[] evaluationInputs)
        {
            return EvaluateAndGateBatchAsync(evaluationInputs).ToSubTasks(bits => bits.ToArray(), evaluationInputs.Length);
        }

        private async Task<BitArray> EvaluateAndGateBatchAsync(GateEvaluationInput<Task<Bit>>[] evaluationInputs)
        {
            int numberOfInvocations = evaluationInputs.Length;

            BitArray leftShares = new BitArray(numberOfInvocations);
            BitArray rightShares = new BitArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                leftShares[i] = await evaluationInputs[i].LeftValue.ConfigureAwait(false);
                rightShares[i] = await evaluationInputs[i].RightValue.ConfigureAwait(false);
            }

            return await _multiplicativeSharing.ComputeMultiplicativeSharesAsync(_session, leftShares, rightShares, numberOfInvocations);
        }

        public async Task<Bit> EvaluateXorGate(Task<Bit> leftValue, Task<Bit> rightValue)
        {
            Bit leftShare = await leftValue.ConfigureAwait(false);
            Bit rightShare = await rightValue.ConfigureAwait(false);
            return leftShare ^ rightShare;
        }

        public async Task<Bit> EvaluateNotGate(Task<Bit> value)
        {
            Bit share = await value.ConfigureAwait(false);
            if (_session.LocalParty.IsFirstParty())
                return ~share;

            return share;
        }
    }
}
