using System;

namespace CompactMPC.Buffers
{
    public class Message
    {
        public static readonly Message Empty = new Message(new byte[] { });

        private readonly byte[] _buffer;
        private readonly int _startIndex;
        private readonly int _length;
        private readonly Message? _previous;

        private Message(byte[] buffer, int startIndex = 0, Message? previous = null)
        {
            if (startIndex > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            _buffer = buffer;
            _startIndex = startIndex;
            _length = buffer.Length - startIndex + (previous?.Length ?? 0);
            _previous = previous;
        }

        public Message Write(byte[] bytes)
        {
            return new Message(bytes, 0, _length == 0 ? null : this);
        }

        public Message Write(int value)
        {
            return Write(BitConverter.GetBytes(value));
        }

        public Message ReadBytes(int numberOfBytes, out byte[] bytes)
        {
            if (_previous != null)
                return new Message(ToBuffer()).ReadBytes(numberOfBytes, out bytes);
            
            int endIndex = _startIndex + numberOfBytes;
            if (endIndex > _buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(numberOfBytes));

            bytes = new byte[numberOfBytes];
            Buffer.BlockCopy(_buffer, _startIndex, bytes, 0, numberOfBytes);

            return SubMessage(endIndex);
        }

        public Message ReadInt(out int value)
        {
            if (_previous != null)
                return new Message(ToBuffer()).ReadInt(out value);
            
            value = BitConverter.ToInt32(_buffer, _startIndex);
            return SubMessage(_startIndex + sizeof(int));
        }

        private Message SubMessage(int startIndex)
        {
            if (startIndex == _buffer.Length)
                return Empty;

            return new Message(_buffer, startIndex);
        }

        public byte[] ToBuffer()
        {
            if (_startIndex == 0 && _previous == null)
                return _buffer;

            byte[] buffer = new byte[_length];
            CopyTo(buffer);
            return buffer;
        }

        private void CopyTo(byte[] buffer)
        {
            int offset = 0;
            if (_previous != null)
            {
                _previous.CopyTo(buffer);
                offset = _previous.Length;
            }
            
            Buffer.BlockCopy(_buffer, _startIndex, buffer, offset, _buffer.Length - _startIndex);
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }
    }
}
