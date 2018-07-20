using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class UnsafeObliviousTransfer : IObliviousTransfer
    {
        public Task SendAsync(Stream stream, BitQuadruple options)
        {
            stream.Write(options.Select(bit => (byte)bit).ToArray(), 0, 4);
            return Task.CompletedTask;
        }

        public async Task<Bit> ReceiveAsync(Stream stream, int selectionIndex)
        {
            if (selectionIndex < 0 || selectionIndex >= 4)
                throw new ArgumentOutOfRangeException(nameof(selectionIndex), "Invalid selection index for 1-out-of-4 oblivious transfer.");
            
            byte[] buffer = await stream.ReadAsync(4);
            return (Bit)buffer[selectionIndex];
        }
    }
}
