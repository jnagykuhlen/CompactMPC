﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Circuits.Batching;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol
{
    public class SecretSharingSecureComputation : ISecureComputation
    {
        private readonly IMultiplicativeSharing _multiplicativeSharing;

        public IMultiPartyNetworkSession MultiPartySession { get; }
        
        public SecretSharingSecureComputation(IMultiPartyNetworkSession multiPartySession, IMultiplicativeSharing multiplicativeSharing)
        {
            MultiPartySession = multiPartySession;
            _multiplicativeSharing = multiplicativeSharing;
        }

        public async Task<BitArray> EvaluateAsync(IBatchEvaluableCircuit evaluable, InputPartyMapping inputMapping, OutputPartyMapping outputMapping, BitArray localInputValues)
        {
            if (inputMapping.NumberOfInputs != evaluable.Context.NumberOfInputWires)
                throw new ArgumentException(
                    "The number of inputs in input mapping does not match the number of declared inputs in the circuit.",
                    nameof(inputMapping)
                );

            if (outputMapping.NumberOfOutputs != evaluable.Context.NumberOfOutputWires)
                throw new ArgumentException(
                    "The number of outputs in output mapping does not match the number of declared outputs in the circuit.",
                    nameof(outputMapping)
                );

            SecretSharingBooleanCircuitEvaluator evaluator = new SecretSharingBooleanCircuitEvaluator(MultiPartySession, _multiplicativeSharing);

            BitArray maskedInputs = await MaskInputs(inputMapping, localInputValues);

            IReadOnlyList<Task<Bit>> inputTasks = maskedInputs.Select(Task.FromResult).ToArray();
            IReadOnlyList<Task<Bit>> outputTasks = evaluable.Evaluate(evaluator, inputTasks);
            BitArray maskedOutputs = new BitArray(await Task.WhenAll(outputTasks));

            return await UnmaskOutputs(outputMapping, maskedOutputs);
        }

        private async Task<BitArray> MaskInputs(InputPartyMapping inputMapping, BitArray localInputValues)
        {
            List<int>[] inputIds = new List<int>[MultiPartySession.NumberOfParties];
            for (int partyId = 0; partyId < inputIds.Length; ++partyId)
                inputIds[partyId] = new List<int>();

            for (int inputId = 0; inputId < inputMapping.NumberOfInputs; ++inputId)
            {
                int partyId = inputMapping.GetAssignedParty(inputId);
                if (partyId < 0 || partyId >= MultiPartySession.NumberOfParties)
                    throw new ArgumentException("Input mapping assigns inputs to party not participating in current session.", nameof(inputMapping));

                inputIds[partyId].Add(inputId);
            }

            List<int> localInputIds = inputIds[MultiPartySession.LocalParty.Id];
            if (localInputValues.Length != localInputIds.Count)
                throw new ArgumentException(
                    "Number of provided inputs does not match the number of declared inputs in the circuit for the local party.",
                    nameof(localInputValues)
                );

            BitArray localSharesOfInput = new BitArray(inputMapping.NumberOfInputs);

            // --- Share local inputs and send via network ---
            if (localInputIds.Count > 0)
            {
                BitArray localSharesOfLocalInput = localInputValues.Clone();
                
                foreach (ITwoPartyNetworkSession session in MultiPartySession.RemotePartySessions)
                {
                    BitArray remoteSharesOfLocalInput = RandomNumberGenerator.GetBits(localInputIds.Count);
                    localSharesOfLocalInput.Xor(remoteSharesOfLocalInput);

                    await session.Channel.WriteMessageAsync(new Message(remoteSharesOfLocalInput.ToBytes()));
                }
                
                for (int localInputId = 0; localInputId < localInputIds.Count; ++localInputId)
                    localSharesOfInput[localInputIds[localInputId]] = localSharesOfLocalInput[localInputId];
            }
            
            // --- Receive shares of remote inputs via network ---
            foreach (ITwoPartyNetworkSession session in MultiPartySession.RemotePartySessions)
            {
                List<int> remoteInputIds = inputIds[session.RemoteParty.Id];

                if (remoteInputIds.Count > 0)
                {
                    Message message = await session.Channel.ReadMessageAsync();
                    BitArray localSharesOfRemoteInput = BitArray.FromBytes(message.ToBuffer(), remoteInputIds.Count);

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
            List<int>[] outputIds = new List<int>[MultiPartySession.NumberOfParties];
            for (int partyId = 0; partyId < outputIds.Length; ++partyId)
                outputIds[partyId] = new List<int>();

            for (int outputId = 0; outputId < outputMapping.NumberOfOutputs; ++outputId)
            for (int partyId = 0; partyId < MultiPartySession.NumberOfParties; ++partyId)
                if (outputMapping.GetAssignedParties(outputId).Contains(partyId))
                    outputIds[partyId].Add(outputId);

            foreach (ITwoPartyNetworkSession session in MultiPartySession.RemotePartySessions)
            {
                List<int> remoteOutputIds = outputIds[session.RemoteParty.Id];
                if (remoteOutputIds.Count > 0)
                {
                    BitArray localSharesOfRemoteOutput = new BitArray(remoteOutputIds.Count);
                    for (int i = 0; i < remoteOutputIds.Count; ++i)
                        localSharesOfRemoteOutput[i] = localSharesOfOutput[remoteOutputIds[i]];

                    await session.Channel.WriteMessageAsync(new Message(localSharesOfRemoteOutput.ToBytes()));
                }
            }

            List<int> localOutputIds = outputIds[MultiPartySession.LocalParty.Id];
            BitArray localOutputValues = new BitArray(localOutputIds.Count);
            for (int i = 0; i < localOutputIds.Count; ++i)
                localOutputValues[i] = localSharesOfOutput[localOutputIds[i]];

            if (localOutputIds.Count > 0)
            {
                foreach (ITwoPartyNetworkSession session in MultiPartySession.RemotePartySessions)
                {
                    Message message = await session.Channel.ReadMessageAsync();
                    BitArray remoteSharesOfLocalOutput = BitArray.FromBytes(message.ToBuffer(), localOutputIds.Count);
                    localOutputValues.Xor(remoteSharesOfLocalOutput);
                }
            }

            return localOutputValues;
        }
    }
}
