using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public interface IMultiChoicesOblivousTransferChannel
    {
        Task SendAsync(byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
    }

    public abstract class MultiChoicesObliviousTransferChannel : IMultiChoicesOblivousTransferChannel
    {
        public Task<byte[][]> ReceiveAsync(int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            // todo: input sanitization here
            throw new NotImplementedException();

            return InternalReceiveAsync(selectionIndices, numberOfOptions, numberOfInvocations, numberOfMessageBytes);
        }

        public Task SendAsync(byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            // todo: input sanitization here
            throw new NotImplementedException();

            return InternalSendAsync(options, numberOfOptions, numberOfInvocations, numberOfMessageBytes);
        }

        protected abstract Task<byte[][]> InternalReceiveAsync(int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
        protected abstract Task InternalSendAsync(byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
    }

    public interface IMultiChoicesObliviousTransfer
    {
        IMultiChoicesOblivousTransferChannel CreateChannel(IMessageChannel channel);
    }
}
