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
        private IMultiPartyNetworkSession _multiPartySession;
        private IPairwiseMultiplicationScheme _multiplicationScheme;
        
        public GMWBooleanCircuitEvaluator(IMultiPartyNetworkSession multiPartySession, IPairwiseMultiplicationScheme multiplicationScheme)
        {
            _multiPartySession = multiPartySession;
            _multiplicationScheme = multiplicationScheme;
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
            BitArray localShares = new BitArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                leftShares[i] = await evaluationInputs[i].LeftValue.ConfigureAwait(false);
                rightShares[i] = await evaluationInputs[i].RightValue.ConfigureAwait(false);
                localShares[i] = leftShares[i] & rightShares[i];
            }

            Task<BitArray>[] pairwiseMultiplicationTasks = new Task<BitArray>[_multiPartySession.NumberOfParties];
            pairwiseMultiplicationTasks[_multiPartySession.LocalParty.Id] = Task.FromResult(localShares);
            
            Parallel.ForEach(_multiPartySession.RemotePartySessions, session =>
            {
                pairwiseMultiplicationTasks[session.RemoteParty.Id] = _multiplicationScheme.ComputeMultiplicationSharesAsync(
                    session,
                    leftShares,
                    rightShares,
                    numberOfInvocations
                );
            });

            BitArray[] shares = await Task.WhenAll(pairwiseMultiplicationTasks).ConfigureAwait(false);
            BitArray result = shares.Aggregate((left, right) => left ^ right);
#if DEBUG
            Console.WriteLine(
                "Evaluated AND gates {0} of {1} total.",
                String.Join(", ", evaluationInputs.Select(input => input.Context.TypeUniqueId + 1)),
                circuitContext.NumberOfAndGates
            );
#endif
            return result;
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
            if (_multiPartySession.LocalParty.Id == 0)
                return ~share;

            return share;
        }
    }
}
