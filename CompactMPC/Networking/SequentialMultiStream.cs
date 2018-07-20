using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public class SequentialMultiStream : MultiStream
    {
        private Stream _innerStream;
        private BinaryWriter _writer;
        private BinaryReader _reader;
        private MemoryStream _readBuffer;
        private int _currentInstanceId;

        public SequentialMultiStream(Stream innerStream)
            : base(innerStream)
        {
            _innerStream = innerStream;
            _writer = new BinaryWriter(_innerStream, Encoding.UTF8, true);
            _reader = new BinaryReader(_innerStream, Encoding.UTF8, true);
            _readBuffer = null;
            _currentInstanceId = 0;
        }

        public override Stream GetSubstream(int instanceId)
        {
            return new SequentialSubstream(this, instanceId);
        }

        public void Write(int instanceId, byte[] buffer, int offset, int count)
        {
            lock (_writer)
            {
                _writer.Write(instanceId);
                _writer.Write(count);
                _writer.Write(buffer, offset, count);
            }
        }

        public int Read(int instanceId, byte[] buffer, int offset, int count)
        {
            lock (_reader)
            {
                while (true)
                {
                    if (_readBuffer != null && _readBuffer.Position < _readBuffer.Length)
                    {
                        if (instanceId == _currentInstanceId)
                        {
                            return _readBuffer.Read(buffer, offset, count);
                        }
                        else
                        {
                            throw new InvalidOperationException("Buffered data is not associated with requested instance.");
                        }
                    }

                    int readInstanceId = _reader.ReadInt32();
                    int readCount = _reader.ReadInt32();
                    byte[] readData = _reader.ReadBytes(readCount);

                    if (_readBuffer != null)
                        _readBuffer.Dispose();

                    _readBuffer = new MemoryStream(readData);
                    _currentInstanceId = readInstanceId;
                }
            }
        }
    }
}
