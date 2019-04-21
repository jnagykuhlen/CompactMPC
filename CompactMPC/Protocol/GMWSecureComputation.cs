using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol
{
    public class GMWSecureComputation : SecureComputation
    {
        private IBatchObliviousTransfer _batchObliviousTransfer;
        private CryptoContext _cryptoContext;
        
        public GMWSecureComputation(INetworkSession session, IBatchObliviousTransfer batchObliviousTransfer, CryptoContext cryptoContext)
            : base(session)
        {
            _batchObliviousTransfer = batchObliviousTransfer;
            _cryptoContext = cryptoContext;
        }

        public override async Task<BitArray> EvaluateAsync(IEvaluableCircuit evaluable, InputPartyMapping inputMapping, OutputPartyMapping outputMapping, BitArray localInputValues)
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
            
            GMWBooleanCircuitEvaluator evaluator = new GMWBooleanCircuitEvaluator(Session, _batchObliviousTransfer, _cryptoContext, evaluable.Context);

            BitArray maskedInputs = await MaskInputs(inputMapping, localInputValues);

            Task<Bit>[] inputTasks = BitArrayHelper.ToBits(maskedInputs).Select(bit => Task.FromResult(bit)).ToArray();
            Task<Bit>[] outputTasks = evaluable.Evaluate(evaluator, inputTasks);
            BitArray maskedOutputs = BitArrayHelper.FromBits(await Task.WhenAll(outputTasks));

            return await UnmaskOutputs(outputMapping, maskedOutputs);
        }

        private async Task<BitArray> MaskInputs(InputPartyMapping inputMapping, BitArray localInputValues)
        {
            List<int>[] inputIds = new List<int>[Session.NumberOfParties];
            for (int partyId = 0; partyId < inputIds.Length; ++partyId)
                inputIds[partyId] = new List<int>();

            for (int inputId = 0; inputId < inputMapping.NumberOfInputs; ++inputId)
            {
                int partyId = inputMapping.GetAssignedParty(inputId);
                if (partyId < 0 || partyId >= Session.NumberOfParties)
                    throw new ArgumentException("Input mapping assigns inputs to party not participating in current session.", nameof(inputMapping));

                inputIds[partyId].Add(inputId);
            }

            List<int> localInputIds = inputIds[Session.LocalParty.Id];
            if (localInputValues.Length != localInputIds.Count)
            {
                throw new ArgumentException(
                    "Number of provided inputs does not match the number of declared inputs in the circuit for the local party.",
                    nameof(localInputValues)
                );
            }

            // Task<Bit>[] inputTasks = new Task<Bit>[inputMapping.NumberOfInputs];

            BitArray localSharesOfInput = new BitArray(inputMapping.NumberOfInputs);

            // --- Share local inputs and send via network ---
            if (localInputIds.Count > 0)
            {
                BitArray localSharesOfLocalInput = localInputValues;
                
                foreach (Party remoteParty in Session.RemoteParties)
                {
                    BitArray remoteSharesOfLocalInput = _cryptoContext.RandomNumberGenerator.GetBits(localInputIds.Count);
                    localSharesOfLocalInput = localSharesOfLocalInput.Xor(remoteSharesOfLocalInput);

                    await Session.GetChannel(remoteParty.Id).WriteMessageAsync(BitArrayHelper.ToBytes(remoteSharesOfLocalInput));
                }
                
                for (int localInputId = 0; localInputId < localInputIds.Count; ++localInputId)
                    localSharesOfInput[localInputIds[localInputId]] = localSharesOfLocalInput[localInputId];
            }
            
            // --- Receive shares of remote inputs via network ---
            foreach (Party remoteParty in Session.RemoteParties)
            {
                List<int> remoteInputIds = inputIds[remoteParty.Id];

                if (remoteInputIds.Count > 0)
                {
                    BitArray localSharesOfRemoteInput = BitArrayHelper.FromBytes(await Session.GetChannel(remoteParty.Id).ReadMessageAsync(), remoteInputIds.Count);

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
            List<int>[] outputIds = new List<int>[Session.NumberOfParties];
            for (int partyId = 0; partyId < outputIds.Length; ++partyId)
                outputIds[partyId] = new List<int>();

            for (int outputId = 0; outputId < outputMapping.NumberOfOutputs; ++outputId)
            {
                for (int partyId = 0; partyId < Session.NumberOfParties; ++partyId)
                {
                    if (outputMapping.GetAssignedParties(outputId).Contains(partyId))
                        outputIds[partyId].Add(outputId);
                }
            }
            
            // BitArray localSharesOfOutput = new BitArray((await Task.WhenAll(outputTasks)).Select(share => share.Value).ToArray());

            foreach (Party remoteParty in Session.RemoteParties)
            {
                List<int> remoteOutputIds = outputIds[remoteParty.Id];
                if (remoteOutputIds.Count > 0)
                {
                    BitArray localSharesOfRemoteOutput = new BitArray(remoteOutputIds.Count);
                    for (int i = 0; i < remoteOutputIds.Count; ++i)
                        localSharesOfRemoteOutput[i] = localSharesOfOutput[remoteOutputIds[i]];

                    await Session.GetChannel(remoteParty.Id).WriteMessageAsync(BitArrayHelper.ToBytes(localSharesOfRemoteOutput));
                }
            }

            List<int> localOutputIds = outputIds[Session.LocalParty.Id];
            BitArray localOutputValues = new BitArray(localOutputIds.Count);
            for (int i = 0; i < localOutputIds.Count; ++i)
                localOutputValues[i] = localSharesOfOutput[localOutputIds[i]];

            if (localOutputIds.Count > 0)
            {
                foreach (Party remoteParty in Session.RemoteParties)
                {
                    BitArray remoteSharesOfLocalOutput = BitArrayHelper.FromBytes(await Session.GetChannel(remoteParty.Id).ReadMessageAsync(), localOutputIds.Count);
                    localOutputValues = localOutputValues.Xor(remoteSharesOfLocalOutput);
                }

                /*
                BitArray[] remoteSharesOfLocalOutputs = await Task.WhenAll(Session.RemoteParties.Select(remoteParty => Session.GetChannel(remoteParty.Id).ReadMessageAsync().ContinueWith(task => BitArrayHelper.FromBytes(task.Result, localOutputIds.Count))));
                foreach (BitArray remoteSharesOfLocalOutput in remoteSharesOfLocalOutputs)
                    localOutputValues = localOutputValues.Xor(remoteSharesOfLocalOutput);
                */
            }

            return localOutputValues;
        }
    }
}
