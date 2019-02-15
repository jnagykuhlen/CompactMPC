using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using CompactMPC.Circuits;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;

namespace CompactMPC.Protocol.Internal
{
    public class GMWBooleanCircuitEvaluator : ICircuitEvaluator<Task<Bit>>
    {
        private INetworkSession _session;
        private CryptoContext _cryptoContext;
        private IObliviousTransferStream[] _obliviousTransferStreams;
        
        public GMWBooleanCircuitEvaluator(
            INetworkSession session,
            IBatchObliviousTransfer batchObliviousTransfer,
            CryptoContext cryptoContext,
            CircuitContext circuitContext)
        {
            _session = session;
            _cryptoContext = cryptoContext;
            _obliviousTransferStreams = PreprocessObliviousTransfers(batchObliviousTransfer, circuitContext.NumberOfAndGates);
        }

        private PreprocessedObliviousTransferStream[] PreprocessObliviousTransfers(IBatchObliviousTransfer batchObliviousTransfer, int numberOfInstances)
        {
            PreprocessedObliviousTransferStream[] preprocessedStreams = new PreprocessedObliviousTransferStream[_session.NumberOfParties];
            ObliviousTransferPreprocessor preprocessor = new ObliviousTransferPreprocessor(batchObliviousTransfer, _cryptoContext);
            
            Task[] preprocessingTasks = new Task[_session.NumberOfParties];
            preprocessingTasks[_session.LocalParty.Id] = Task.CompletedTask;

            Parallel.ForEach(_session.RemoteParties, remoteParty =>
            {
                Stream baseStream = _session.GetConnection(remoteParty.Id);
                MultiStream multiStream = new BufferedMultiStream(baseStream);

                if (remoteParty.Id < _session.LocalParty.Id)
                {
                    Task preprocessSenderTask = preprocessor.PreprocessSenderAsync(baseStream, numberOfInstances).ContinueWith(task =>
                    {
                        preprocessedStreams[remoteParty.Id] = new PreprocessedObliviousTransferStream(multiStream, task.Result, null);
                    });

                    preprocessingTasks[remoteParty.Id] = preprocessSenderTask;
                }
                else
                {
                    Task preprocessReceiverTask = preprocessor.PreprocessReceiverAsync(baseStream, numberOfInstances).ContinueWith(task =>
                    {
                        preprocessedStreams[remoteParty.Id] = new PreprocessedObliviousTransferStream(multiStream, null, task.Result);
                    });

                    preprocessingTasks[remoteParty.Id] = preprocessReceiverTask;
                }
            });
            
            Task.WaitAll(preprocessingTasks.ToArray());
            
            return preprocessedStreams;
        }

        public async Task<Bit> EvaluateAndGate(Task<Bit> leftValue, Task<Bit> rightValue, GateContext gateContext, CircuitContext circuitContext)
        {
            Bit leftShare = await leftValue.ConfigureAwait(false);
            Bit rightShare = await rightValue.ConfigureAwait(false);

            BitArray randomBits = _cryptoContext.RandomNumberGenerator.GetBits(_session.LocalParty.Id);
            
            Task<Bit>[] shareTasks = new Task<Bit>[_session.NumberOfParties];
            shareTasks[_session.LocalParty.Id] = Task.FromResult(leftShare & rightShare);
            
            foreach (Party remoteParty in _session.RemoteParties)
            {
                IObliviousTransferStream obliviousTransferStream = _obliviousTransferStreams[remoteParty.Id];

                if (remoteParty.Id < _session.LocalParty.Id)
                {
                    Bit randomShare = new Bit(randomBits[remoteParty.Id]);
                    shareTasks[remoteParty.Id] = obliviousTransferStream.SendAsync(
                        gateContext.TypeUniqueId,
                        new BitQuadruple(
                            randomShare,                             // 00
                            randomShare ^ leftShare,                 // 01
                            randomShare ^ rightShare,                // 10
                            randomShare ^ leftShare ^ rightShare     // 11
                        )
                    ).ContinueWith(task => randomShare);
                }
                else
                {
                    int selectedIndex = 2 * (byte)leftShare + (byte)rightShare;
                    shareTasks[remoteParty.Id] = obliviousTransferStream.ReceiveAsync(
                        gateContext.TypeUniqueId,
                        selectedIndex
                    );
                }
            }
            
            Bit result = (await Task.WhenAll(shareTasks).ConfigureAwait(false)).Aggregate((left, right) => left ^ right);
#if DEBUG
            Console.WriteLine("Evaluated AND gate {0} / {1}.", gateContext.TypeUniqueId + 1, circuitContext.NumberOfAndGates);
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
