using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class ObliviousTransferStream : IObliviousTransferStream
    {
        private IObliviousTransfer _obliviousTransfer;
        private MultiStream _multiStream;

        public ObliviousTransferStream(IObliviousTransfer obliviousTransfer, MultiStream multiStream)
        {
            _obliviousTransfer = obliviousTransfer;
            _multiStream = multiStream;
        }

        public Task SendAsync(int instanceId, BitQuadruple options)
        {
            Stream stream = _multiStream.GetSubstream(instanceId);
            return _obliviousTransfer.SendAsync(stream, options).ContinueWith((task) => stream.Dispose());
        }

        public Task<Bit> ReceiveAsync(int instanceId, int selectionIndex)
        {
            Stream stream = _multiStream.GetSubstream(instanceId);
            return _obliviousTransfer.ReceiveAsync(stream, selectionIndex).ContinueWith((task) => { stream.Dispose(); return task.Result; });
        }
    }
}
