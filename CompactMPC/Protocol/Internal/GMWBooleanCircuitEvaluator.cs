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

        public Task<Bit>[] EvaluateAndGateBatch(GateEvaluationInput<Task<Bit>>[] evaluationInputs, CircuitContext circuitContext)
        {
            return EvaluateAndGateBatchAsync(evaluationInputs, circuitContext).ToSubTasks(bits => bits.ToArray(), evaluationInputs.Length);
        }

        private async Task<BitArray> EvaluateAndGateBatchAsync(GateEvaluationInput<Task<Bit>>[] evaluationInputs, CircuitContext circuitContext)
        {
            int numberOfInvocations = evaluationInputs.Length;

            BitArray leftShares = new BitArray(numberOfInvocations);
            BitArray rightShares = new BitArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                leftShares[i] = await evaluationInputs[i].LeftValue.ConfigureAwait(false);
                rightShares[i] = await evaluationInputs[i].RightValue.ConfigureAwait(false);
            }

            BitArray multiplicativeShares = await _multiplicativeSharing.ComputeMultiplicativeSharesAsync(_session, leftShares, rightShares, numberOfInvocations);
#if DEBUG
            Console.WriteLine(
                "Evaluated AND gates {0} of {1} total.",
                String.Join(", ", evaluationInputs.Select(input => input.Context.TypeUniqueId + 1)),
                circuitContext.NumberOfAndGates
            );
#endif
            return multiplicativeShares;
        }

        public async Task<Bit> EvaluateXorGate(Task<Bit> leftValue, Task<Bit> rightValue, GateContext gateContext, CircuitContext circuitContext)
        {
            Bit leftShare = await leftValue.ConfigureAwait(false);
            Bit rightShare = await rightValue.ConfigureAwait(false);
            return leftShare ^ rightShare;
        }

        public async Task<Bit> EvaluateNotGate(Task<Bit> value, GateContext gateContext, CircuitContext circuitContext)
        {
            Bit share = await value.ConfigureAwait(false);
            if (_session.LocalParty.IsFirstParty())
                return ~share;

            return share;
        }
    }
}
