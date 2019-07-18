using System.Threading.Tasks;
using System.Security.Cryptography;
using CompactMPC.Networking;
using System;

namespace CompactMPC.ObliviousTransfer
{
    public interface ITwoChoicesObliviousTransferChannel
    {
        Task SendAsync(Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);

        IMessageChannel Channel { get; }
    }

    public interface ITwoChoicesCorrelatedObliviousTransferChannel
    {
        Task<Pair<byte[]>[]> SendAsync(byte[][] correlationStrings, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);

        IMessageChannel Channel { get; }
    }

    public interface ITwoChoicesRandomObliviousTransferChannel
    {
        Task<Pair<byte[]>[]> SendAsync(int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);

        IMessageChannel Channel { get; }
    }

    public abstract class TwoChoicesObliviousTransferChannel : ITwoChoicesObliviousTransferChannel, ITwoChoicesCorrelatedObliviousTransferChannel, ITwoChoicesRandomObliviousTransferChannel
    {
        public abstract Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        public abstract Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        public abstract Task SendAsync(Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        protected abstract RandomNumberGenerator RandomNumberGenerator { get; }
        public abstract IMessageChannel Channel { get; }

        async Task<Pair<byte[]>[]> ITwoChoicesCorrelatedObliviousTransferChannel.SendAsync(byte[][] correlationStrings, int numberOfInvocations, int numberOfMessageBytes)
        {
            Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                byte[] random = RandomNumberGenerator.GetBytes(numberOfMessageBytes);
                byte[] correlated = BitArray.Xor(random, correlationStrings[i]);
                options[i] = new Pair<byte[]>(random, correlated);
            }
            await SendAsync(options, numberOfInvocations, numberOfMessageBytes);
            return options;
        }

        async Task<Pair<byte[]>[]> ITwoChoicesRandomObliviousTransferChannel.SendAsync(int numberOfInvocations, int numberOfMessageBytes)
        {
            Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                options[i] = new Pair<byte[]>(
                    RandomNumberGenerator.GetBytes(numberOfMessageBytes),
                    RandomNumberGenerator.GetBytes(numberOfMessageBytes)
                );
            }
            await SendAsync(options, numberOfInvocations, numberOfMessageBytes);
            return options;
        }
    }

    public abstract class TwoChoicesCorrelatedObliviousTransferChannel : ITwoChoicesCorrelatedObliviousTransferChannel, ITwoChoicesRandomObliviousTransferChannel
    {
        public abstract Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        public abstract Task<Pair<byte[]>[]> SendAsync(byte[][] correlationStrings, int numberOfInvocations, int numberOfMessageBytes);
        protected abstract RandomNumberGenerator RandomNumberGenerator { get; }
        public abstract IMessageChannel Channel { get; }

        public Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return ReceiveAsync(selectionIndices.ToPairIndexArray(), numberOfInvocations, numberOfMessageBytes);
        }

        Task<Pair<byte[]>[]> ITwoChoicesRandomObliviousTransferChannel.SendAsync(int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[][] correlationStrings = new byte[numberOfInvocations][];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                correlationStrings[i] = RandomNumberGenerator.GetBytes(numberOfMessageBytes);
            }
            return SendAsync(correlationStrings, numberOfInvocations, numberOfMessageBytes);
        }
    }

    //public abstract class TwoChoicesRandomObliviousTransferChannel : ITwoChoicesRandomObliviousTransferChannel
    //{
    //    public Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
    //    {
    //        return ReceiveAsync(selectionIndices.ToPairIndexArray(), numberOfInvocations, numberOfMessageBytes);
    //    }

    //    public Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
    //    {
    //        if (selectionIndices.Length != numberOfInvocations)
    //            throw new ArgumentException("The amount of selection indices is not equal to the number of invocations.");

    //        return InternalReceiveAsync(selectionIndices, numberOfInvocations, numberOfMessageBytes);
    //    }

    //    protected abstract Task<byte[][]> InternalReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    //    public abstract Task<Pair<byte[]>[]> SendAsync(int numberOfInvocations, int numberOfMessageBytes);
    //    public abstract IMessageChannel Channel { get; }
    //}

    public interface ITwoChoicesObliviousTransfer
    {
        ITwoChoicesObliviousTransferChannel CreateChannel(IMessageChannel channel);
    }

    public interface ITwoChoicesCorrelatedObliviousTransfer
    {
        ITwoChoicesCorrelatedObliviousTransferChannel CreateCorrelatedChannel(IMessageChannel channel);
    }

    public interface ITwoChoicesRandomObliviousTransfer
    {
        ITwoChoicesRandomObliviousTransferChannel CreateRandomChannel(IMessageChannel channel);
    }

    public class TwoChoicesExtendedObliviousTransfer : ITwoChoicesObliviousTransfer
    {
        private ITwoChoicesObliviousTransfer _baseOT;
        private int _securityParameter;
        private CryptoContext _cryptoContext;

        public TwoChoicesExtendedObliviousTransfer(ITwoChoicesObliviousTransfer baseOT, int securityParameter, CryptoContext cryptoContext)
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
