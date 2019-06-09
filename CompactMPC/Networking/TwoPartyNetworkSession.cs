using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CompactMPC.Networking
{
    public class TwoPartyNetworkSession : ITwoPartyNetworkSession
    {
        private IMessageChannel _channel;
        private Party _localParty;
        private Party _remoteParty;

        public TwoPartyNetworkSession(IMessageChannel channel, Party localParty, Party remoteParty)
        {
            _channel = channel;
            _localParty = localParty;
            _remoteParty = remoteParty;
        }

        public IMessageChannel Channel
        {
            get
            {
                return _channel;
            }
        }

        public Party LocalParty
        {
            get
            {
                return _localParty;
            }
        }

        public Party RemoteParty
        {
            get
            {
                return _remoteParty;
            }
        }

        public void Dispose() { }
    }
}
