using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;

namespace CompactMPC.Protocol
{
    public abstract class SecureComputation
    {
        private IMultiPartyNetworkSession _multiPartySession;

        public SecureComputation(IMultiPartyNetworkSession multiPartySession)
        {
            _multiPartySession = multiPartySession;
        }

        public abstract Task<BitArray> EvaluateAsync(IBatchEvaluableCircuit evaluable, InputPartyMapping inputMapping, OutputPartyMapping outputMapping, BitArray localInputs);

        public IMultiPartyNetworkSession MultiPartySession
        {
            get
            {
                return _multiPartySession;
            }
        }
    }
}
