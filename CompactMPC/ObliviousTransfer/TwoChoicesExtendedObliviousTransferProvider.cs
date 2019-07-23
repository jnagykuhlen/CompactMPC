using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class TwoChoicesExtendedObliviousTransferProvider : ITwoChoicesObliviousTransferProvider
    {
        private ITwoChoicesObliviousTransferProvider _baseOT;
        private int _securityParameter;
        private CryptoContext _cryptoContext;

        public TwoChoicesExtendedObliviousTransferProvider(ITwoChoicesObliviousTransferProvider baseOT, int securityParameter, CryptoContext cryptoContext)
        {
            _baseOT = baseOT;
            _securityParameter = securityParameter;
            _cryptoContext = cryptoContext;
        }

        public ITwoChoicesObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new TwoChoicesExtendedObliviousTransferChannel(_baseOT.CreateChannel(channel), _securityParameter, _cryptoContext);
        }
    }
}
