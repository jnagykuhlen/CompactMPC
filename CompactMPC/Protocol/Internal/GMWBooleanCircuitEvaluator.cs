using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;

namespace CompactMPC.Protocol.Internal
{
    public class GMWBooleanCircuitEvaluator : IBatchCircuitEvaluator<Task<Bit>>
    {
        private INetworkSession _session;
        private CryptoContext _cryptoContext;
        private IObliviousTransfer[] _obliviousTransfers;
        
        public GMWBooleanCircuitEvaluator(
            INetworkSession session,
            IObliviousTransfer obliviousTransfer,
            CryptoContext cryptoContext,
            CircuitContext circuitContext)
        {
            _session = session;
            _cryptoContext = cryptoContext;
            _obliviousTransfers = PreprocessObliviousTransfers(obliviousTransfer, circuitContext.NumberOfAndGates);
        }

        private PreprocessedObliviousTransfer[] PreprocessObliviousTransfers(IObliviousTransfer obliviousTransfer, int numberOfInstances)
        {
            PreprocessedObliviousTransfer[] obliviousTransfers = new PreprocessedObliviousTransfer[_session.NumberOfParties];
            ObliviousTransferPreprocessor preprocessor = new ObliviousTransferPreprocessor(obliviousTransfer, _cryptoContext);
            
            Task[] preprocessingTasks = new Task[_session.NumberOfParties];
            preprocessingTasks[_session.LocalParty.Id] = Task.CompletedTask;

            Parallel.ForEach(_session.RemoteParties, remoteParty =>
            {
                Stream baseStream = _session.GetConnection(remoteParty.Id);
                
                if (remoteParty.Id < _session.LocalParty.Id)
                {
                    Task preprocessSenderTask = preprocessor.PreprocessSenderAsync(baseStream, numberOfInstances).ContinueWith(task =>
                    {
                        obliviousTransfers[remoteParty.Id] = new PreprocessedObliviousTransfer(task.Result, null);
                    });

                    preprocessingTasks[remoteParty.Id] = preprocessSenderTask;
                }
                else
                {
                    Task preprocessReceiverTask = preprocessor.PreprocessReceiverAsync(baseStream, numberOfInstances).ContinueWith(task =>
                    {
                        obliviousTransfers[remoteParty.Id] = new PreprocessedObliviousTransfer(null, task.Result);
                    });

                    preprocessingTasks[remoteParty.Id] = preprocessReceiverTask;
                }
            });
            
            Task.WaitAll(preprocessingTasks);
            
            return obliviousTransfers;
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

            BitArray randomBits = _cryptoContext.RandomNumberGenerator.GetBits(numberOfInvocations * _session.LocalParty.Id);

            Task<BitArray>[] shareTasks = new Task<BitArray>[_session.NumberOfParties];
            shareTasks[_session.LocalParty.Id] = Task.FromResult(localShares);
            
            Parallel.ForEach(_session.RemoteParties, remoteParty =>
            {
                Stream stream = _session.GetConnection(remoteParty.Id);

                if (remoteParty.Id < _session.LocalParty.Id)
                {
                    BitArray randomShares = new BitArray(numberOfInvocations);
                    BitQuadruple[] options = new BitQuadruple[numberOfInvocations];

                    for (int i = 0; i < numberOfInvocations; ++i)
                    {
                        randomShares[i] = randomBits[numberOfInvocations * remoteParty.Id + i];
                        options[i] = new BitQuadruple(
                            randomShares[i],                                     // 00
                            randomShares[i] ^ leftShares[i],                     // 01
                            randomShares[i] ^ rightShares[i],                    // 10
                            randomShares[i] ^ leftShares[i] ^ rightShares[i]     // 11
                        );
                    }

                    shareTasks[remoteParty.Id] = _obliviousTransfers[remoteParty.Id].SendAsync(stream, options, numberOfInvocations)
                        .ContinueWith(task => randomShares);
                }
                else
                {
                    int[] selectionIndices = new int[numberOfInvocations];
                    for (int i = 0; i < numberOfInvocations; ++i)
                        selectionIndices[i] = 2 * (byte)leftShares[i] + (byte)rightShares[i];

                    shareTasks[remoteParty.Id] = _obliviousTransfers[remoteParty.Id].ReceiveAsync(stream, selectionIndices, numberOfInvocations);
                }
            });

            BitArray[] shares = await Task.WhenAll(shareTasks).ConfigureAwait(false);
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
            if (_session.LocalParty.Id == 0)
                return ~share;

            return share;
        }
    }
}
