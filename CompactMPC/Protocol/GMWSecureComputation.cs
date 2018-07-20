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

        public override BitArray Evaluate(IBooleanEvaluable evaluable, InputPartyMapping inputMapping, OutputPartyMapping outputMapping, BitArray localInputValues)
        {
            if (inputMapping.NumberOfInputs != evaluable.NumberOfInputs)
            {
                throw new ArgumentException(
                    "The number of inputs in input mapping does not match the number of declared inputs in the circuit.",
                    nameof(inputMapping)
                );
            }

            if (outputMapping.NumberOfOutputs != evaluable.NumberOfOutputs)
            {
                throw new ArgumentException(
                    "The number of outputs in output mapping does not match the number of declared outputs in the circuit.",
                    nameof(outputMapping)
                );
            }
            
            GMWBooleanCircuitEvaluator evaluator = new GMWBooleanCircuitEvaluator(Session, _batchObliviousTransfer, _cryptoContext, evaluable.CircuitContext);

            Task<Bit>[] inputTasks = MaskInputs(inputMapping, localInputValues);
            Task<Bit>[] outputTasks = evaluable.Evaluate(evaluator, inputTasks);
            BitArray localOutputValues = UnmaskOutputs(outputMapping, outputTasks);

            return localOutputValues;
        }

        private Task<Bit>[] MaskInputs(InputPartyMapping inputMapping, BitArray localInputValues)
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

            Task<Bit>[] inputTasks = new Task<Bit>[inputMapping.NumberOfInputs];

            // --- Share local inputs and send via network ---
            if (localInputIds.Count > 0)
            {
                BitArray localSharesOfLocalInput = localInputValues;
                
                foreach (Party remoteParty in Session.RemoteParties)
                {
                    BitArray remoteSharesOfLocalInput = _cryptoContext.RandomNumberGenerator.GetBits(localInputIds.Count);
                    localSharesOfLocalInput = localSharesOfLocalInput.Xor(remoteSharesOfLocalInput);

                    BitArrayHelper.WriteToStream(remoteSharesOfLocalInput, Session.GetConnection(remoteParty.Id));
                }
                
                for (int localInputId = 0; localInputId < localInputIds.Count; ++localInputId)
                    inputTasks[localInputIds[localInputId]] = Task.FromResult(new Bit(localSharesOfLocalInput[localInputId]));
            }

            // --- Receive shares of remote inputs via network ---
            foreach (Party remoteParty in Session.RemoteParties)
            {
                List<int> remoteInputIds = inputIds[remoteParty.Id];

                if (remoteInputIds.Count > 0)
                {
                    BitArray localSharesOfRemoteInput = BitArrayHelper.ReadFromStream(Session.GetConnection(remoteParty.Id));

                    if (localSharesOfRemoteInput.Length != remoteInputIds.Count)
                        throw new ProtocolException("Number of input shares received from remote party does not match number of declared inputs in the circuit.");

                    for (int remoteInputId = 0; remoteInputId < remoteInputIds.Count; ++remoteInputId)
                        inputTasks[remoteInputIds[remoteInputId]] = Task.FromResult(new Bit(localSharesOfRemoteInput[remoteInputId]));
                }
            }

            return inputTasks;
        }

        private BitArray UnmaskOutputs(OutputPartyMapping outputMapping, Task<Bit>[] outputTasks)
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

            BitArray localSharesOfOutput = new BitArray(Task.WhenAll(outputTasks).Result.Select(share => share.Value).ToArray());

            foreach (Party remoteParty in Session.RemoteParties)
            {
                List<int> remoteOutputIds = outputIds[remoteParty.Id];
                if (remoteOutputIds.Count > 0)
                {
                    BitArray localSharesOfRemoteOutput = new BitArray(remoteOutputIds.Count);
                    for (int i = 0; i < remoteOutputIds.Count; ++i)
                        localSharesOfRemoteOutput[i] = localSharesOfOutput[remoteOutputIds[i]];

                    BitArrayHelper.WriteToStream(localSharesOfRemoteOutput, Session.GetConnection(remoteParty.Id));
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
                    BitArray remoteSharesOfLocalOutput = BitArrayHelper.ReadFromStream(Session.GetConnection(remoteParty.Id));

                    if (remoteSharesOfLocalOutput.Length != localOutputIds.Count)
                        throw new ProtocolException("Number of output shares received from remote party does not match number of declared outputs in the circuit.");

                    localOutputValues = localOutputValues.Xor(remoteSharesOfLocalOutput);
                }
            }

            return localOutputValues;
        }
    }
}
