using System;

namespace CompactMPC.Buffers
{
    public class Message
    {
        private readonly byte[] _buffer;
        private readonly int _startIndex;
        private readonly int _length;

        public Message(byte[] buffer) : this(buffer, 0, buffer.Length)
        {
        }

        public Message(int capacity) : this(new byte[capacity], 0, 0)
        {
        } 

        private Message(byte[] buffer, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            
            if (length < 0 || length > buffer.Length - startIndex)
                throw new ArgumentOutOfRangeException(nameof(length));

            _buffer = buffer;
            _startIndex = startIndex;
            _length = length;
        }

        public Message Write(byte[] bytes) => Write(bytes, 0, bytes.Length);
        public Message Write(int value) => Write(BitConverter.GetBytes(value));
        public Message Write(Message message) => Write(message._buffer, message._startIndex, message._length);

        private Message Write(byte[] bytes, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (length < 0 || length > bytes.Length - startIndex)
                throw new ArgumentOutOfRangeException(nameof(length));
            
            if (bytes.Length > Capacity)
                throw new ArgumentException("Written bytes exceed message capacity.", nameof(bytes));
            
            Buffer.BlockCopy(bytes, startIndex, _buffer, _startIndex + _length, length);
            
            return new Message(_buffer, _startIndex, _length + bytes.Length);
        }

        public Message Pad(int numberOfBytes)
        {
            if (numberOfBytes > Capacity)
                throw new ArgumentException("Padded bytes exceed message capacity.", nameof(numberOfBytes));
            
            return new Message(_buffer, _startIndex, _length + numberOfBytes);
        }

        public Message ReadBytes(int numberOfBytes, out byte[] bytes)
        {
            var endIndex = _startIndex + numberOfBytes;
            if (endIndex > _buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(numberOfBytes));

            bytes = new byte[numberOfBytes];
            Buffer.BlockCopy(_buffer, _startIndex, bytes, 0, numberOfBytes);

            return SubMessage(endIndex);
        }

        public Message ReadInt(out int value)
        {
            value = BitConverter.ToInt32(_buffer, _startIndex);
            return SubMessage(_startIndex + sizeof(int));
        }

        public Message ReadMessage(int length, out Message message)
        {
            message = new Message(_buffer, _startIndex, length);
            return new Message(_buffer, _startIndex + length, _length - length);
        }

        private Message SubMessage(int startIndex) => new(_buffer, startIndex, _startIndex + _length - startIndex);

        public byte[] ToBuffer()
        {
            if (_startIndex == 0 && _length == _buffer.Length)
                return _buffer;

            var buffer = new byte[_length];
            Buffer.BlockCopy(_buffer, _startIndex, buffer, 0, _length);
            return buffer;
        }

        public ReadOnlySpan<byte> ToSpan() => new(_buffer, _startIndex, _length);
        
        public override string ToString() => $"0x{Convert.ToHexString(_buffer, _startIndex, _length)}";

        public override bool Equals(object? other)
        {
            if (other is Message otherMessage)
                return ToSpan().SequenceEqual(otherMessage.ToSpan());
            return false;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.AddBytes(ToSpan());
            return hash.ToHashCode();
        }

        public int Length => _length;
        
        private int Capacity => _buffer.Length - _startIndex - _length;
    }
}
