using System.Threading.Tasks;
using System.Security.Cryptography;
using CompactMPC.Networking;
using System;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-2 Oblivious Transfer channel implementation.
    /// 
    /// Provides 1oo2-OT on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// </summary>
    public interface ITwoChoicesObliviousTransferChannel
    {
        Task SendAsync(Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);

        IMessageChannel Channel { get; }
    }

    /// <summary>
    /// A 1-out-of-2 Correlated Oblivious Transfer channel implementation.
    /// 
    /// Provides 1oo2-C-OT on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// 
    /// The C-OT functionality receives a correlation string ∆ and chooses the sender’s inputs uniformly under the constraint that their XOR equals ∆. [Asharov et al.]
    /// </summary>
    /// <remarks>
    /// Reference: Gilad Asharov, Yehuda Lindell, Thomas Schneider and Michael Zohner: More Efficient Oblivious Transfer and Extensions for Faster Secure Computation. 2013. Section 5.3. https://thomaschneider.de/papers/ALSZ13.pdf
    /// </remarks>
    public interface ITwoChoicesCorrelatedObliviousTransferChannel
    {
        Task<Pair<byte[]>[]> SendAsync(byte[][] correlationStrings, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);

        IMessageChannel Channel { get; }
    }

    /// <summary>
    /// A 1-out-of-2 Random Oblivious Transfer channel implementation.
    /// 
    /// Provides 1oo2-R-OT on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// 
    /// The R-OT functionality is exactly the same as general OT, except that the sender does not provide inputs but gets two random messages as outputs [Asharov et al.]
    /// </summary>
    /// <remarks>
    /// Reference: Gilad Asharov, Yehuda Lindell, Thomas Schneider and Michael Zohner: More Efficient Oblivious Transfer and Extensions for Faster Secure Computation. 2013. Section 5.3. https://thomaschneider.de/papers/ALSZ13.pdf
    /// </remarks>
    public interface ITwoChoicesRandomObliviousTransferChannel
    {
        Task<Pair<byte[]>[]> SendAsync(int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);

        IMessageChannel Channel { get; }
    }

    //public abstract class TwoChoicesObliviousTransferChannel : ITwoChoicesObliviousTransferChannel, ITwoChoicesCorrelatedObliviousTransferChannel, ITwoChoicesRandomObliviousTransferChannel
    //{
    //    public abstract Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    //    public abstract Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    //    public abstract Task SendAsync(Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
    //    protected abstract RandomNumberGenerator RandomNumberGenerator { get; }
    //    public abstract IMessageChannel Channel { get; }

    //    async Task<Pair<byte[]>[]> ITwoChoicesCorrelatedObliviousTransferChannel.SendAsync(byte[][] correlationStrings, int numberOfInvocations, int numberOfMessageBytes)
    //    {
    //        Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
    //        for (int i = 0; i < numberOfInvocations; ++i)
    //        {
    //            byte[] random = RandomNumberGenerator.GetBytes(numberOfMessageBytes);
    //            byte[] correlated = BitArray.Xor(random, correlationStrings[i]);
    //            options[i] = new Pair<byte[]>(random, correlated);
    //        }
    //        await SendAsync(options, numberOfInvocations, numberOfMessageBytes);
    //        return options;
    //    }

    //    async Task<Pair<byte[]>[]> ITwoChoicesRandomObliviousTransferChannel.SendAsync(int numberOfInvocations, int numberOfMessageBytes)
    //    {
    //        Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
    //        for (int i = 0; i < numberOfInvocations; ++i)
    //        {
    //            options[i] = new Pair<byte[]>(
    //                RandomNumberGenerator.GetBytes(numberOfMessageBytes),
    //                RandomNumberGenerator.GetBytes(numberOfMessageBytes)
    //            );
    //        }
    //        await SendAsync(options, numberOfInvocations, numberOfMessageBytes);
    //        return options;
    //    }
    //}

    //public abstract class TwoChoicesCorrelatedObliviousTransferChannel : ITwoChoicesCorrelatedObliviousTransferChannel, ITwoChoicesRandomObliviousTransferChannel
    //{
    //    public abstract Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    //    public abstract Task<Pair<byte[]>[]> SendAsync(byte[][] correlationStrings, int numberOfInvocations, int numberOfMessageBytes);
    //    protected abstract RandomNumberGenerator RandomNumberGenerator { get; }
    //    public abstract IMessageChannel Channel { get; }

    //    public Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
    //    {
    //        return ReceiveAsync(selectionIndices.ToPairIndexArray(), numberOfInvocations, numberOfMessageBytes);
    //    }

    //    Task<Pair<byte[]>[]> ITwoChoicesRandomObliviousTransferChannel.SendAsync(int numberOfInvocations, int numberOfMessageBytes)
    //    {
    //        byte[][] correlationStrings = new byte[numberOfInvocations][];
    //        for (int i = 0; i < numberOfInvocations; ++i)
    //        {
    //            correlationStrings[i] = RandomNumberGenerator.GetBytes(numberOfMessageBytes);
    //        }
    //        return SendAsync(correlationStrings, numberOfInvocations, numberOfMessageBytes);
    //    }
    //}

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

    public interface ITwoChoicesObliviousTransferProvider
    {
        ITwoChoicesObliviousTransferChannel CreateChannel(IMessageChannel channel);
    }

    public interface ITwoChoicesCorrelatedObliviousTransferProvider
    {
        ITwoChoicesCorrelatedObliviousTransferChannel CreateChannel(IMessageChannel channel);
    }

    public interface ITwoChoicesRandomObliviousTransferProvider
    {
        ITwoChoicesRandomObliviousTransferChannel CreateChannel(IMessageChannel channel);
    }

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
