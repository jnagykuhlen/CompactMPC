using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class TwoChoicesCorrelatedExtendedObliviousTransferProvider : IObliviousTransferProvider<ITwoChoicesCorrelatedObliviousTransferChannel>
    {
        private IObliviousTransferProvider<ITwoChoicesObliviousTransferChannel> _baseOT;
        private int _securityParameter;
        private CryptoContext _cryptoContext;

        public TwoChoicesCorrelatedExtendedObliviousTransferProvider(IObliviousTransferProvider<ITwoChoicesObliviousTransferChannel> baseOT, int securityParameter, CryptoContext cryptoContext)
        {
            _baseOT = baseOT;
            _securityParameter = securityParameter;
            _cryptoContext = cryptoContext;
        }

        public ITwoChoicesCorrelatedObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new TwoChoicesCorrelatedExtendedObliviousTransferChannel(_baseOT.CreateChannel(channel), _securityParameter, _cryptoContext);
        }
    }
}
