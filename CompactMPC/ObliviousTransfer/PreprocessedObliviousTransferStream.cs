using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class PreprocessedObliviousTransferStream : IObliviousTransferStream
    {
        private MultiStream _multiStream;
        private PreprocessedSenderBatch _senderBatch;
        private PreprocessedReceiverBatch _receiverBatch;

        public PreprocessedObliviousTransferStream(MultiStream multiStream, PreprocessedSenderBatch senderBatch, PreprocessedReceiverBatch receiverBatch)
        {
            if (multiStream == null)
                throw new ArgumentNullException(nameof(multiStream));

            _multiStream = multiStream;
            _senderBatch = senderBatch;
            _receiverBatch = receiverBatch;
        }
        
        public async Task SendAsync(int instanceId, BitQuadruple options)
        {
            if (_senderBatch == null || instanceId >= _senderBatch.NumberOfInstances)
                throw new InvalidOperationException("Not enough preprocessed sender data available.");

            using (Stream stream = _multiStream.GetSubstream(instanceId))
            {
                int deltaSelectionIndex = await stream.ReadByteAsync() % 4;

                BitQuadruple preprocessedOptions = _senderBatch.GetOptions(instanceId);
                BitQuadruple maskedOptions = new BitQuadruple(
                    options[0] ^ preprocessedOptions[(0 + deltaSelectionIndex) % 4],
                    options[1] ^ preprocessedOptions[(1 + deltaSelectionIndex) % 4],
                    options[2] ^ preprocessedOptions[(2 + deltaSelectionIndex) % 4],
                    options[3] ^ preprocessedOptions[(3 + deltaSelectionIndex) % 4]
                );
                
                await stream.WriteByteAsync(maskedOptions.PackedValue);
            }
        }
        
        public async Task<Bit> ReceiveAsync(int instanceId, int selectionIndex)
        {
            if (_receiverBatch == null || instanceId >= _receiverBatch.NumberOfInstances)
                throw new InvalidOperationException("Not enough preprocessed receiver data available.");

            using (Stream stream = _multiStream.GetSubstream(instanceId))
            {
                int deltaSelectionIndex = (_receiverBatch.GetSelectionIndex(instanceId) - selectionIndex + 4) % 4;
                await stream.WriteByteAsync((byte)deltaSelectionIndex);

                BitQuadruple maskedOptions = BitQuadruple.FromPackedValue((byte)await stream.ReadByteAsync());
                return maskedOptions[selectionIndex] ^ _receiverBatch.GetSelectedOption(instanceId);
            }
        }
    }
}
