using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol
{
    public class GMWSecureComputation : ISecureComputation
    {
        private IMultiPartyNetworkSession _multiPartySession;
        private IMultiplicativeSharing _multiplicativeSharing;
        private CryptoContext _cryptoContext;
        
        public GMWSecureComputation(IMultiPartyNetworkSession multiPartySession, IMultiplicativeSharing multiplicativeSharing, CryptoContext cryptoContext)
        {
            _multiPartySession = multiPartySession;
            _multiplicativeSharing = multiplicativeSharing;
            _cryptoContext = cryptoContext;
        }

        public async Task<BitArray> EvaluateAsync(IBatchEvaluableCircuit evaluable, InputPartyMapping inputMapping, OutputPartyMapping outputMapping, BitArray localInputValues)
        {
            if (inputMapping.NumberOfInputs != evaluable.Context.NumberOfInputGates)
            {
                throw new ArgumentException(
                    "The number of inputs in input mapping does not match the number of declared inputs in the circuit.",
                    nameof(inputMapping)
                );
            }

            if (outputMapping.NumberOfOutputs != evaluable.Context.NumberOfOutputGates)
            {
                throw new ArgumentException(
                    "The number of outputs in output mapping does not match the number of declared outputs in the circuit.",
                    nameof(outputMapping)
                );
            }
            
            GMWBooleanCircuitEvaluator evaluator = new GMWBooleanCircuitEvaluator(_multiPartySession, _multiplicativeSharing);

            BitArray maskedInputs = await MaskInputs(inputMapping, localInputValues);

            Task<Bit>[] inputTasks = maskedInputs.Select(bit => Task.FromResult(bit)).ToArray();
            Task<Bit>[] outputTasks = evaluable.Evaluate(evaluator, inputTasks);
            BitArray maskedOutputs = new BitArray(await Task.WhenAll(outputTasks));

            return await UnmaskOutputs(outputMapping, maskedOutputs);
        }

        private async Task<BitArray> MaskInputs(InputPartyMapping inputMapping, BitArray localInputValues)
        {
            List<int>[] inputIds = new List<int>[_multiPartySession.NumberOfParties];
            for (int partyId = 0; partyId < inputIds.Length; ++partyId)
                inputIds[partyId] = new List<int>();

            for (int inputId = 0; inputId < inputMapping.NumberOfInputs; ++inputId)
            {
                int partyId = inputMapping.GetAssignedParty(inputId);
                if (partyId < 0 || partyId >= _multiPartySession.NumberOfParties)
                    throw new ArgumentException("Input mapping assigns inputs to party not participating in current session.", nameof(inputMapping));

                inputIds[partyId].Add(inputId);
            }

            List<int> localInputIds = inputIds[_multiPartySession.LocalParty.Id];
            if (localInputValues.Length != localInputIds.Count)
            {
                throw new ArgumentException(
                    "Number of provided inputs does not match the number of declared inputs in the circuit for the local party.",
                    nameof(localInputValues)
                );
            }

            BitArray localSharesOfInput = new BitArray(inputMapping.NumberOfInputs);

            // --- Share local inputs and send via network ---
            if (localInputIds.Count > 0)
            {
                BitArray localSharesOfLocalInput = localInputValues.Clone();
                
                foreach (ITwoPartyNetworkSession session in _multiPartySession.RemotePartySessions)
                {
                    BitArray remoteSharesOfLocalInput = _cryptoContext.RandomNumberGenerator.GetBits(localInputIds.Count);
                    localSharesOfLocalInput.Xor(remoteSharesOfLocalInput);

                    await session.Channel.WriteMessageAsync(remoteSharesOfLocalInput.ToBytes());
                }
                
                for (int localInputId = 0; localInputId < localInputIds.Count; ++localInputId)
                    localSharesOfInput[localInputIds[localInputId]] = localSharesOfLocalInput[localInputId];
            }
            
            // --- Receive shares of remote inputs via network ---
            foreach (ITwoPartyNetworkSession session in _multiPartySession.RemotePartySessions)
            {
                List<int> remoteInputIds = inputIds[session.RemoteParty.Id];

                if (remoteInputIds.Count > 0)
                {
                    BitArray localSharesOfRemoteInput = BitArray.FromBytes(await session.Channel.ReadMessageAsync(), remoteInputIds.Count);

                    if (localSharesOfRemoteInput.Length != remoteInputIds.Count)
                        throw new ProtocolException("Number of input shares received from remote party does not match number of declared inputs in the circuit.");

                    for (int remoteInputId = 0; remoteInputId < remoteInputIds.Count; ++remoteInputId)
                        localSharesOfInput[remoteInputIds[remoteInputId]] = localSharesOfRemoteInput[remoteInputId];
                }
            }

            return localSharesOfInput;
        }

        private async Task<BitArray> UnmaskOutputs(OutputPartyMapping outputMapping, BitArray localSharesOfOutput)
        {
            List<int>[] outputIds = new List<int>[_multiPartySession.NumberOfParties];
            for (int partyId = 0; partyId < outputIds.Length; ++partyId)
                outputIds[partyId] = new List<int>();

            for (int outputId = 0; outputId < outputMapping.NumberOfOutputs; ++outputId)
            {
                for (int partyId = 0; partyId < _multiPartySession.NumberOfParties; ++partyId)
                {
                    if (outputMapping.GetAssignedParties(outputId).Contains(partyId))
                        outputIds[partyId].Add(outputId);
                }
            }
            
            foreach (ITwoPartyNetworkSession session in _multiPartySession.RemotePartySessions)
            {
                List<int> remoteOutputIds = outputIds[session.RemoteParty.Id];
                if (remoteOutputIds.Count > 0)
                {
                    BitArray localSharesOfRemoteOutput = new BitArray(remoteOutputIds.Count);
                    for (int i = 0; i < remoteOutputIds.Count; ++i)
                        localSharesOfRemoteOutput[i] = localSharesOfOutput[remoteOutputIds[i]];

                    await session.Channel.WriteMessageAsync(localSharesOfRemoteOutput.ToBytes());
                }
            }

            List<int> localOutputIds = outputIds[_multiPartySession.LocalParty.Id];
            BitArray localOutputValues = new BitArray(localOutputIds.Count);
            for (int i = 0; i < localOutputIds.Count; ++i)
                localOutputValues[i] = localSharesOfOutput[localOutputIds[i]];

            if (localOutputIds.Count > 0)
            {
                foreach (ITwoPartyNetworkSession session in _multiPartySession.RemotePartySessions)
                {
                    BitArray remoteSharesOfLocalOutput = BitArray.FromBytes(await session.Channel.ReadMessageAsync(), localOutputIds.Count);
                    localOutputValues.Xor(remoteSharesOfLocalOutput);
                }
            }

            return localOutputValues;
        }

        public IMultiPartyNetworkSession MultiPartySession
        {
            get
            {
                return _multiPartySession;
            }
        }
    }
}
