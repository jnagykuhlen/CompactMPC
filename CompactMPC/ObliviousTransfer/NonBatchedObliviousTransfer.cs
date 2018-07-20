using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public class NonBatchedObliviousTransfer : IObliviousTransfer
    {
        private IBatchObliviousTransfer _batchObliviousTransfer;

        public NonBatchedObliviousTransfer(IBatchObliviousTransfer batchObliviousTransfer)
        {
            _batchObliviousTransfer = batchObliviousTransfer;
        }

        public Task SendAsync(Stream stream, BitQuadruple options)
        {
            Quadruple<byte[]>[] batchedOptions = new[] { new Quadruple<byte[]>(options.Select(bit => new[] { (byte)bit }).ToArray()) };
            return _batchObliviousTransfer.SendAsync(stream, batchedOptions, 1, 1);
        }

        public Task<Bit> ReceiveAsync(Stream stream, int selectionIndex)
        {
            int[] batchedSelectionIndices = new[] { selectionIndex };
            return _batchObliviousTransfer.ReceiveAsync(stream, batchedSelectionIndices, 1, 1).ContinueWith(task => (Bit)task.Result[0][0]);
        }
    }
}
