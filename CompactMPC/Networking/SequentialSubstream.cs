using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public class SequentialSubstream : Stream
    {
        private SequentialMultiStream _multiStream;
        private int _instanceId;
        
        public SequentialSubstream(SequentialMultiStream multiStream, int instanceId)
        {
            _multiStream = multiStream;
            _instanceId = instanceId;
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            _multiStream.Write(_instanceId, buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _multiStream.Read(_instanceId, buffer, offset, count);
        }

        public override void Flush()
        {
            _multiStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Cannot seek in substream.");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Cannot set length of substream.");
        }

        public override bool CanWrite
        {
            get
            {
                return _multiStream.CanWrite;
            }
        }

        public override bool CanRead
        {
            get
            {
                return _multiStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }
        
        public override long Length
        {
            get
            {
                throw new NotSupportedException("Cannot get length of substream.");
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException("Cannot get position in substream.");
            }

            set
            {
                throw new NotSupportedException("Cannot set position in substream.");
            }
        }
    }
}
