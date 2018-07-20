using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CompactMPC.Networking
{
    public class BufferedMultiStream : MultiStream, IDisposable
    {
        private Dictionary<int, BufferedSubStream> _substreams;
        private AutoResetEvent _readLock;
        private bool _isDisposed;

        private BinaryWriter _writer;
        private BinaryReader _reader;

        public BufferedMultiStream(Stream innerStream)
            : base(innerStream)
        {
            _substreams = new Dictionary<int, BufferedSubStream>();
            _readLock = new AutoResetEvent(false);
            _isDisposed = false;

            if (innerStream.CanWrite)
                _writer = new BinaryWriter(innerStream, Encoding.UTF8, true);

            if (innerStream.CanRead)
            {
                _reader = new BinaryReader(innerStream, Encoding.UTF8, true);
                Task.Factory.StartNew(FillReadBuffers, TaskCreationOptions.LongRunning);
            }
        }

        private void FillReadBuffers()
        {
            while (!_isDisposed)
            {
                _readLock.WaitOne();
                
                BufferedSubStream[] substreamsSnapshot;
                lock (_substreams)
                {
                    substreamsSnapshot = _substreams.Values.ToArray();
                }

                foreach (BufferedSubStream substream in substreamsSnapshot)
                {
                    while (substream.RequestedBytes > 0)
                    {
                        int instanceId = _reader.ReadInt32();
                        int count = _reader.ReadInt32();
                        byte[] buffer = _reader.ReadBytes(count);
                        
                        GetOrCreateSubstream(instanceId).AppendBuffer(buffer);
                    }
                }
            }
        }
        
        public void Write(int channelId, byte[] buffer, int offset, int count)
        {
            if (_writer == null)
                throw new NotSupportedException("The underlying stream does not support write operations.");
            
            lock (_writer)
            {
                _writer.Write(channelId);
                _writer.Write(count);
                _writer.Write(buffer, offset, count);
                _writer.Flush();
            }
        }
        
        public void RequestBytes()
        {
            _readLock.Set();
        }

        public override Stream GetSubstream(int instanceId)
        {
            return GetOrCreateSubstream(instanceId);
        }

        private BufferedSubStream GetOrCreateSubstream(int channelId)
        {
            lock (_substreams)
            {
                BufferedSubStream substream;
                if (!_substreams.TryGetValue(channelId, out substream))
                {
                    substream = new BufferedSubStream(this, channelId);
                    _substreams.Add(channelId, substream);
                }

                return substream;
            }
        }

        public void Dispose()
        {
            Flush();
            _readLock.Dispose();
            _isDisposed = true;
        }
    }
}
