using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace CompactMPC.Networking
{
    public class BufferedSubStream : Stream
    {
        private const int InitialCapacity = 1024;
        
        private BufferedMultiStream _multiStream;
        private int _instanceId;

        private MemoryStream _readStream;
        private TaskCompletionSource<int> _readTaskCompletionSource;
        private int _requestedBytes;
        
        public BufferedSubStream(BufferedMultiStream multiStream, int instanceId)
        {
            _multiStream = multiStream;
            _instanceId = instanceId;

            _readStream = new MemoryStream(InitialCapacity);
            _readTaskCompletionSource = null;
            _requestedBytes = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).Result;
        }
        
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Task<int> readTask = null;
            lock (_readStream)
            {
                int bufferedBytes = (int)(_readStream.Length - _readStream.Position);

                int requiredBytes = count - bufferedBytes;
                if (requiredBytes > 0)
                {
                    _requestedBytes = requiredBytes;
                    _readTaskCompletionSource = new TaskCompletionSource<int>();
                    readTask = _readTaskCompletionSource.Task;
                }

                _multiStream.RequestBytes();
            }

            if (readTask != null)
                await readTask;
            
            lock (_readStream)
            {
                return _readStream.Read(buffer, offset, count);
            }
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            _multiStream.Write(_instanceId, buffer, offset, count);
        }

        public void AppendBuffer(byte[] buffer)
        {
            lock (_readStream)
            {
                long readPosition = _readStream.Position;
                _readStream.Position = _readStream.Length;
                _readStream.Write(buffer, 0, buffer.Length);
                _readStream.Position = readPosition;
                _requestedBytes -= buffer.Length;

                if (_readTaskCompletionSource != null)
                {
                    _readTaskCompletionSource.SetResult(buffer.Length);
                    _readTaskCompletionSource = null;
                }
            }
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

        protected override void Dispose(bool disposing)
        {
            _readStream.Dispose();
            base.Dispose(disposing);
        }

        public override bool CanRead
        {
            get
            {
                return _multiStream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _multiStream.CanWrite;
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

        public int RequestedBytes
        {
            get
            {
                return _requestedBytes;
            }
        }
    }
}
