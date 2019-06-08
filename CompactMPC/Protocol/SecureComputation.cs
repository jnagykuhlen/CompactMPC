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
        private INetworkSession _session;

        public SecureComputation(INetworkSession session)
        {
            _session = session;
        }

        public abstract Task<BitArray> EvaluateAsync(IBatchEvaluableCircuit evaluable, InputPartyMapping inputMapping, OutputPartyMapping outputMapping, BitArray localInputs);

        public INetworkSession Session
        {
            get
            {
                return _session;
            }
        }
    }
}
